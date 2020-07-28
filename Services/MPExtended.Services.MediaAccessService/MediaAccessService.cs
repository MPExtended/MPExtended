#region Copyright (C) 2012-2013 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Extensions;
using MPExtended.Libraries.Service.Network;
using MPExtended.Libraries.Service.Shared;
using MPExtended.Libraries.Service.Shared.Filters;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Playlist;

namespace MPExtended.Services.MediaAccessService
{
  // Here we implement all the methods, but we don't do any data retrieval, that
  // is handled by the backend library classes. We only do some filtering and
  // sorting.

  [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
  public class MediaAccessService : ProviderHandler, IMediaAccessService
  {
    #region General
    private const int API_VERSION = 5;
    private const string MP_VERSION = "MP1";

    private ILibrary GetLibrary(int? provider, WebMediaType type)
    {
      switch (type)
      {
        case WebMediaType.Movie:
        case WebMediaType.Collection:
          return MovieLibraries[provider];
        case WebMediaType.MusicTrack:
        case WebMediaType.MusicAlbum:
        case WebMediaType.MusicArtist:
          return MusicLibraries[provider];
        case WebMediaType.Picture:
        case WebMediaType.PictureFolder:
        case WebMediaType.MobileVideo:
          return PictureLibraries[provider];
        case WebMediaType.TVShow:
        case WebMediaType.TVSeason:
        case WebMediaType.TVEpisode:
          return TVShowLibraries[provider];
        case WebMediaType.File:
        case WebMediaType.Folder:
        case WebMediaType.Drive:
          return FileSystemLibraries[provider];
        default:
          throw new ArgumentException();
      }
    }

    public WebMediaServiceDescription GetServiceDescription()
    {
      LoadProviders();
      return new WebMediaServiceDescription()
      {
        ApiVersion = API_VERSION,
        ServiceVersion = VersionUtil.GetVersionName(),
        MPVersion = MP_VERSION,

        AvailableFileSystemLibraries = FileSystemLibraries.GetAllAsBackendProvider(),
        AvailableMovieLibraries = MovieLibraries.GetAllAsBackendProvider(),
        AvailableMusicLibraries = MusicLibraries.GetAllAsBackendProvider(),
        AvailablePictureLibraries = PictureLibraries.GetAllAsBackendProvider(),
        AvailableTvShowLibraries = TVShowLibraries.GetAllAsBackendProvider(),

        DefaultFileSystemLibrary = ProviderHandler.GetDefaultProvider(ProviderType.Filesystem),
        DefaultMovieLibrary = ProviderHandler.GetDefaultProvider(ProviderType.Movie),
        DefaultMusicLibrary = ProviderHandler.GetDefaultProvider(ProviderType.Music),
        DefaultPictureLibrary = ProviderHandler.GetDefaultProvider(ProviderType.Picture),
        DefaultTvShowLibrary = ProviderHandler.GetDefaultProvider(ProviderType.TVShow),
      };
    }

    public WebBoolResult TestConnection()
    {
      return true;
    }

    public WebMediaItem GetMediaItem(int? provider, WebMediaType type, string id)
    {
      switch (type)
      {
        case WebMediaType.Movie:
          return GetMovieDetailedById(provider, id).Finalize(provider, ProviderType.Movie).ToWebMediaItem();
        case WebMediaType.MusicTrack:
          return GetMusicTrackDetailedById(provider, id).Finalize(provider, ProviderType.Music).ToWebMediaItem();
        case WebMediaType.Picture:
          return GetPictureDetailedById(provider, id).Finalize(provider, ProviderType.Picture).ToWebMediaItem();
        case WebMediaType.MobileVideo:
          return GetMobileVideoBasicById(provider, id).Finalize(provider, ProviderType.Picture).ToWebMediaItem();
        case WebMediaType.TVEpisode:
          return GetTVEpisodeDetailedById(provider, id).Finalize(provider, ProviderType.TVShow).ToWebMediaItem();
        case WebMediaType.Drive:
          return GetFileSystemDriveBasicById(provider, id).Finalize(provider, ProviderType.Filesystem).ToWebMediaItem();
        case WebMediaType.Folder:
          return GetFileSystemFolderBasicById(provider, id).Finalize(provider, ProviderType.Filesystem).ToWebMediaItem();
        case WebMediaType.File:
          return GetFileSystemFileBasicById(provider, id).Finalize(provider, ProviderType.Filesystem).ToWebMediaItem();
        default:
          Log.Warn("Requested media item for mediatype {0} which isn't a media item", type);
          throw new ArgumentException("No mediaitem available for this mediatype", "mediatype");
      }
    }

    public IList<WebSearchResult> Search(string text)
    {
      if (String.IsNullOrWhiteSpace(text))
      {
        return new List<WebSearchResult>();
      }

      return MovieLibraries.SearchAll(text).AsQueryable()
          .Concat(MusicLibraries.SearchAll(text).AsQueryable())
          .Concat(PictureLibraries.SearchAll(text).AsQueryable())
          .Concat(TVShowLibraries.SearchAll(text).AsQueryable())
          .Concat(FileSystemLibraries.SearchAll(text).AsQueryable())
          .OrderByDescending(x => x.Score)
          .ToList();
    }

    public IList<WebSearchResult> SearchResultsByRange(string text, int start, int end)
    {
      return Search(text).TakeRange(start, end).ToList();
    }

    public WebDictionary<string> GetExternalMediaInfo(int? provider, WebMediaType type, string id)
    {
      return GetLibrary(provider, type).GetExternalMediaInfo(type, id);
    }

    public IList<WebDiskSpaceInformation> GetLocalDiskInformation(string filter = null)
    {
      return DriveInfo.GetDrives()
          .Select(x => DiskSpaceInformation.GetSpaceInformation(x.Name))
          .Filter(filter)
          .ToList();
    }
    #endregion

    #region Movies
    public IList<WebCategory> GetMovieCategories(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MovieLibraries[provider].GetAllCategories().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Movie);
    }

    public WebIntResult GetMovieCount(int? provider, string filter = null)
    {
      return MovieLibraries[provider].GetAllMovies().AsQueryable().Filter(filter).Count();
    }

    public IList<WebMovieBasic> GetMoviesBasic(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MovieLibraries[provider].GetAllMovies().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Movie);
    }

    public IList<WebMovieDetailed> GetMoviesDetailed(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MovieLibraries[provider].GetAllMoviesDetailed().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Movie);
    }

    public IList<WebMovieBasic> GetMoviesBasicByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MovieLibraries[provider].GetAllMovies().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Movie);
    }

    public IList<WebMovieDetailed> GetMoviesDetailedByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MovieLibraries[provider].GetAllMoviesDetailed().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Movie);
    }

    public IList<WebGenre> GetMovieGenres(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MovieLibraries[provider].GetAllGenres().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Movie).ToList();
    }

    public IList<WebGenre> GetMovieGenresByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MovieLibraries[provider].GetAllGenres().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Movie);
    }

    public WebIntResult GetMovieGenresCount(int? provider, string filter = null)
    {
      return MovieLibraries[provider].GetAllGenres().AsQueryable().Filter(filter).Count();
    }

    public WebMovieBasic GetMovieBasicById(int? provider, string id)
    {
      return MovieLibraries[provider].GetMovieBasicById(id).Finalize(provider, ProviderType.Movie);
    }

    public WebMovieDetailed GetMovieDetailedById(int? provider, string id)
    {
      return MovieLibraries[provider].GetMovieDetailedById(id).Finalize(provider, ProviderType.Movie);
    }

    public IList<WebActor> GetMovieActors(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MovieLibraries[provider].GetAllMovies().AsQueryable().SelectMany(x => x.Actors).Distinct().Filter(filter).SortMediaItemList(sort, order, WebSortField.Title).Finalize(provider, ProviderType.Movie);
    }

    public IList<WebActor> GetMovieActorsByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MovieLibraries[provider].GetAllMovies().AsQueryable().SelectMany(x => x.Actors).Distinct().Filter(filter).SortMediaItemList(sort, order, WebSortField.Title).TakeRange(start, end).Finalize(provider, ProviderType.Movie);
    }

    public WebIntResult GetMovieActorCount(int? provider, string filter = null)
    {
      return GetMovieActors(provider).AsQueryable().Filter(filter).Count();
    }

    public WebBoolResult SetMovieStoptime(int? provider, string id, int stopTime, Boolean isWatched, int watchedPercent)
    {
      return MovieLibraries[provider].SetMovieStoptime(id, stopTime, isWatched, watchedPercent);
    }

    public WebBoolResult SetWathcedStatus(int? provider, string id, Boolean isWatched)
    {
      return MovieLibraries[provider].SetWathcedStatus(id, isWatched);
    }
    
    public WebCollection GetCollectionById(int? provider, string id)
    {
      return MovieLibraries[provider].GetCollectionById(id).Finalize(provider, ProviderType.Movie);
    }

    public IList<WebCollection> GetCollections(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MovieLibraries[provider].GetAllCollections().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Movie);
    }

    public IList<WebCollection> GetCollectionsByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MovieLibraries[provider].GetAllCollections().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Movie);
    }
    #endregion

    #region Music
    public IList<WebCategory> GetMusicCategories(int? provider, string filter = null)
    {
      return MusicLibraries[provider].GetAllCategories().AsQueryable().Filter(filter).Finalize(provider, ProviderType.Music);
    }

    public WebIntResult GetMusicTrackCount(int? provider, string filter = null)
    {
      return MusicLibraries[provider].GetAllTracks().AsQueryable().Filter(filter).Count();
    }

    public WebIntResult GetMusicAlbumCount(int? provider, string filter = null)
    {
      return MusicLibraries[provider].GetAllAlbums().AsQueryable().Filter(filter).Count();
    }

    public WebIntResult GetMusicArtistCount(int? provider, string filter = null)
    {
      return MusicLibraries[provider].GetAllArtists().AsQueryable().Filter(filter).Count();
    }

    public IList<WebMusicTrackBasic> GetMusicTracksBasic(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllTracks().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
    }

    public IList<WebMusicTrackDetailed> GetMusicTracksDetailed(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllTracksDetailed().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
    }

    public IList<WebMusicTrackBasic> GetMusicTracksBasicByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllTracks().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
    }

    public IList<WebMusicTrackDetailed> GetMusicTracksDetailedByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllTracksDetailed().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
    }

    public WebMusicTrackBasic GetMusicTrackBasicById(int? provider, string id)
    {
      return MusicLibraries[provider].GetTrackBasicById(id).Finalize(provider, ProviderType.Music);
    }

    public IList<WebGenre> GetMusicGenres(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllGenres().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
    }

    public IList<WebGenre> GetMusicGenresByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllGenres().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
    }

    public WebIntResult GetMusicGenresCount(int? provider, string filter = null)
    {
      return MusicLibraries[provider].GetAllGenres().AsQueryable().Filter(filter).Count();
    }

    public WebMusicTrackDetailed GetMusicTrackDetailedById(int? provider, string id)
    {
      return MusicLibraries[provider].GetTrackDetailedById(id).Finalize(provider, ProviderType.Music);
    }

    public IList<WebMusicAlbumBasic> GetMusicAlbumsBasic(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllAlbums().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
    }

    public IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllAlbums().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
    }

    public IList<WebMusicArtistBasic> GetMusicArtistsBasic(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllArtists().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
    }

    public IList<WebMusicArtistBasic> GetMusicArtistsBasicByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllArtists().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
    }

    public WebMusicArtistBasic GetMusicArtistBasicById(int? provider, string id)
    {
      return MusicLibraries[provider].GetArtistBasicById(id).Finalize(provider, ProviderType.Music);
    }

    public IList<WebMusicArtistDetailed> GetMusicArtistsDetailed(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllArtistsDetailed().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
    }

    public IList<WebMusicArtistDetailed> GetMusicArtistsDetailedByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllArtistsDetailed().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
    }

    public WebMusicArtistDetailed GetMusicArtistDetailedById(int? provider, string id)
    {
      return MusicLibraries[provider].GetArtistDetailedById(id).Finalize(provider, ProviderType.Music);
    }

    public IList<WebMusicTrackBasic> GetMusicTracksBasicForAlbum(int? provider, string id, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllTracks().AsQueryable().Where(p => p.AlbumId == id).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
    }

    public IList<WebMusicTrackBasic> GetMusicTracksBasicForArtist(int? provider, string id, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllTracks().AsQueryable().Where(p => p.ArtistId.Contains(id)).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
    }

    public IList<WebMusicTrackDetailed> GetMusicTracksDetailedForAlbum(int? provider, string id, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllTracksDetailed().AsQueryable().Where(p => p.AlbumId == id).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
    }

    public IList<WebMusicTrackDetailed> GetMusicTracksDetailedForArtist(int? provider, string id, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllTracksDetailed().AsQueryable().Where(p => p.ArtistId.Contains(id)).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
    }

    public WebMusicAlbumBasic GetMusicAlbumBasicById(int? provider, string id)
    {
      return MusicLibraries[provider].GetAlbumBasicById(id).Finalize(provider, ProviderType.Music);
    }

    public IList<WebMusicAlbumBasic> GetMusicAlbumsBasicForArtist(int? provider, string id, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return MusicLibraries[provider].GetAllAlbums().AsQueryable().Where(p => p.AlbumArtistId == id).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
    }
    #endregion

    #region Pictures
    public WebIntResult GetPictureCount(int? provider, string filter = null)
    {
      return PictureLibraries[provider].GetAllPicturesBasic().AsQueryable().Filter(filter).Count();
    }

    public IList<WebPictureBasic> GetPicturesBasic(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return PictureLibraries[provider].GetAllPicturesBasic().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Picture);
    }

    public IList<WebPictureBasic> GetPicturesBasicByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return PictureLibraries[provider].GetAllPicturesBasic().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Picture);
    }

    public IList<WebPictureDetailed> GetPicturesDetailed(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return PictureLibraries[provider].GetAllPicturesDetailed().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Picture);
    }

    public IList<WebPictureDetailed> GetPicturesDetailedByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return PictureLibraries[provider].GetAllPicturesDetailed().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Picture);
    }

    public IList<WebCategory> GetPictureCategories(int? provider, string filter = null)
    {
      return PictureLibraries[provider].GetAllPictureCategories().AsQueryable().Filter(filter).Finalize(provider, ProviderType.Picture);
    }

    public IList<WebCategory> GetPictureSubCategories(int? provider, string id, string filter = null)
    {
      return PictureLibraries[provider].GetSubCategoriesById(id).AsQueryable().Filter(filter).Finalize(provider, ProviderType.Picture);
    }

    public IList<WebPictureBasic> GetPicturesBasicByCategory(int? provider, string id, string filter = null)
    {
      return PictureLibraries[provider].GetPicturesBasicByCategory(id).AsQueryable().Filter(filter).Finalize(provider, ProviderType.Picture);
    }

    public IList<WebPictureDetailed> GetPicturesDetailedByCategory(int? provider, string id, string filter = null)
    {
      return PictureLibraries[provider].GetPicturesDetailedByCategory(id).AsQueryable().Filter(filter).Finalize(provider, ProviderType.Picture);
    }

    public WebPictureBasic GetPictureBasicById(int? provider, string id)
    {
      return PictureLibraries[provider].GetPictureBasic(id).Finalize(provider, ProviderType.Picture);
    }

    public WebPictureDetailed GetPictureDetailedById(int? provider, string id)
    {
      return PictureLibraries[provider].GetPictureDetailed(id).Finalize(provider, ProviderType.Picture);
    }

    public WebPictureFolder GetPictureFolderById(int? provider, string id)
    {
      return PictureLibraries[provider].GetPictureFolderById(id).Finalize(provider, ProviderType.Picture);
    }
    
    public IList<WebPictureFolder> GetAllPictureFolders(int? provider, string filter = null)
    {
      return PictureLibraries[provider].GetAllPictureFolders().AsQueryable().Filter(filter).Finalize(provider, ProviderType.Picture);
    }
    
    public IList<WebPictureFolder> GetSubFoldersById(int? provider, string id, string filter = null)
    {
      return PictureLibraries[provider].GetSubFoldersById(id).AsQueryable().Filter(filter).Finalize(provider, ProviderType.Picture);
    }

    public WebIntResult GetMobileVideoCount(int? provider, string filter = null)
    {
      return PictureLibraries[provider].GetAllMobileVideosBasic().AsQueryable().Filter(filter).Count();
    }

    public IList<WebMobileVideoBasic> GetMobileVideosBasic(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return PictureLibraries[provider].GetAllMobileVideosBasic().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Picture);
    }

    public IList<WebMobileVideoBasic> GetMobileVideosBasicByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return PictureLibraries[provider].GetAllMobileVideosBasic().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Picture);
    }

    public IList<WebMobileVideoBasic> GetMobileVideosBasicByCategory(int? provider, string id, string filter = null)
    {
      return PictureLibraries[provider].GetMobileVideosBasicByCategory(id).AsQueryable().Filter(filter).Finalize(provider, ProviderType.Picture);
    }

    public WebMobileVideoBasic GetMobileVideoBasicById(int? provider, string id)
    {
      return PictureLibraries[provider].GetMobileVideoBasic(id).Finalize(provider, ProviderType.Picture);
    }
    #endregion

    #region TVShows
      public IList<WebCategory> GetTVShowCategories(int? provider, string filter = null)
    {
      return TVShowLibraries[provider].GetAllCategories().AsQueryable().Filter(filter).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebGenre> GetTVShowGenres(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllGenres().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebGenre> GetTVShowGenresByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllGenres().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
    }

    public WebIntResult GetTVShowGenresCount(int? provider, string filter = null)
    {
      return TVShowLibraries[provider].GetAllGenres().AsQueryable().Filter(filter).Count();
    }

    public IList<WebTVShowBasic> GetTVShowsBasic(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllTVShowsBasic().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVShowDetailed> GetTVShowsDetailed(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllTVShowsDetailed().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVShowBasic> GetTVShowsBasicByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllTVShowsBasic().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVShowDetailed> GetTVShowsDetailedByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllTVShowsDetailed().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
    }

    public WebTVShowDetailed GetTVShowDetailedById(int? provider, string id)
    {
      return TVShowLibraries[provider].GetTVShowDetailed(id).Finalize(provider, ProviderType.TVShow);
    }

    public WebTVShowBasic GetTVShowBasicById(int? provider, string id)
    {
      return TVShowLibraries[provider].GetTVShowBasic(id).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVSeasonBasic> GetTVSeasonsBasicForTVShow(int? provider, string id, string filter = null, WebSortField? sort = WebSortField.TVSeasonNumber, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllSeasonsBasic().AsQueryable().Where(x => x.ShowId == id).Filter(filter).SortMediaItemList(sort, order, WebSortField.TVSeasonNumber).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVSeasonDetailed> GetTVSeasonsDetailedForTVShow(int? provider, string id, string filter = null, WebSortField? sort = WebSortField.TVSeasonNumber, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllSeasonsDetailed().AsQueryable().Where(x => x.ShowId == id).Filter(filter).SortMediaItemList(sort, order, WebSortField.TVSeasonNumber).Finalize(provider, ProviderType.TVShow);
    }

    public WebTVSeasonDetailed GetTVSeasonDetailedById(int? provider, string id)
    {
      return TVShowLibraries[provider].GetSeasonDetailed(id).Finalize(provider, ProviderType.TVShow);
    }

    public WebTVSeasonBasic GetTVSeasonBasicById(int? provider, string id)
    {
      return TVShowLibraries[provider].GetSeasonBasic(id).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVEpisodeBasic> GetTVEpisodesBasic(int? provider, string filter = null, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().Filter(filter).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailed(int? provider, string filter = null, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllEpisodesDetailed().AsQueryable().Filter(filter).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVEpisodeBasic> GetTVEpisodesBasicByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().Filter(filter).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllEpisodesDetailed().AsQueryable().Filter(filter).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShow(int? provider, string id, string filter = null, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().Where(p => p.ShowId == id).Filter(filter).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShow(int? provider, string id, string filter = null, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllEpisodesDetailed().AsQueryable().Where(p => p.ShowId == id).Filter(filter).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShowByRange(int? provider, string id, int start, int end, string filter = null, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().Where(p => p.ShowId == id).Filter(filter).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).TakeRange(start, end - start).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShowByRange(int? provider, string id, int start, int end, string filter = null, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllEpisodesDetailed().AsQueryable().Where(p => p.ShowId == id).Filter(filter).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).TakeRange(start, end - start).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVEpisodeBasic> GetTVEpisodesBasicForSeason(int? provider, string id, string filter = null, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().Where(p => p.SeasonId == id).Filter(filter).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForSeason(int? provider, string id, string filter = null, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllEpisodesDetailed().AsQueryable().Where(p => p.SeasonId == id).Filter(filter).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).Finalize(provider, ProviderType.TVShow);
    }

    public WebTVEpisodeBasic GetTVEpisodeBasicById(int? provider, string id)
    {
      return TVShowLibraries[provider].GetEpisodeBasic(id).Finalize(provider, ProviderType.TVShow);
    }

    public WebTVEpisodeDetailed GetTVEpisodeDetailedById(int? provider, string id)
    {
      return TVShowLibraries[provider].GetEpisodeDetailed(id).Finalize(provider, ProviderType.TVShow);
    }

    public WebIntResult GetTVEpisodeCount(int? provider, string filter = null)
    {
      return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().Filter(filter).Count();
    }

    public WebIntResult GetTVEpisodeCountForTVShow(int? provider, string id, string filter = null)
    {
      return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().Where(e => e.ShowId == id).Filter(filter).Count();
    }

    public WebIntResult GetTVEpisodeCountForSeason(int? provider, string id, string filter = null)
    {
      return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().Where(e => e.SeasonId == id).Filter(filter).Count();
    }

    public WebIntResult GetTVShowCount(int? provider, string filter = null)
    {
      return TVShowLibraries[provider].GetAllTVShowsBasic().AsQueryable().Filter(filter).Count();
    }

    public WebIntResult GetTVSeasonCountForTVShow(int? provider, string id, string filter = null)
    {
      return TVShowLibraries[provider].GetAllSeasonsBasic().AsQueryable().Where(x => x.ShowId == id).Filter(filter).Count();
    }

    public IList<WebActor> GetTVShowActors(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllTVShowsBasic().AsQueryable().SelectMany(x => x.Actors).Distinct().Filter(filter).SortMediaItemList(sort, order, WebSortField.Title).Finalize(provider, ProviderType.TVShow);
    }

    public IList<WebActor> GetTVShowActorsByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return TVShowLibraries[provider].GetAllTVShowsBasic().AsQueryable().SelectMany(x => x.Actors).Distinct().Filter(filter).SortMediaItemList(sort, order, WebSortField.Title).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
    }

    public WebIntResult GetTVShowActorCount(int? provider, string filter = null)
    {
      return GetTVShowActors(provider).AsQueryable().Filter(filter).Count();
    }
    #endregion

    #region Filesystem
    public WebIntResult GetFileSystemDriveCount(int? provider, string filter = null)
    {
      return FileSystemLibraries[provider].GetDriveListing().AsQueryable().Filter(filter).Count();
    }

    public IList<WebDriveBasic> GetFileSystemDrives(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return FileSystemLibraries[provider].GetDriveListing().AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Filesystem).ToList();
    }

    public IList<WebDriveBasic> GetFileSystemDrivesByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return FileSystemLibraries[provider].GetDriveListing().AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Filesystem).ToList();
    }

    public IList<WebFolderBasic> GetFileSystemFolders(int? provider, string id, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return FileSystemLibraries[provider].GetFoldersListing(id).AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Filesystem).ToList();
    }

    public IList<WebFolderBasic> GetFileSystemFoldersByRange(int? provider, string id, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return FileSystemLibraries[provider].GetFoldersListing(id).AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Filesystem).ToList();
    }

    public WebIntResult GetFileSystemFoldersCount(int? provider, string id, string filter = null)
    {
      return FileSystemLibraries[provider].GetFoldersListing(id).AsQueryable().Filter(filter).Count();
    }

    public IList<WebFileBasic> GetFileSystemFiles(int? provider, string id, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return FileSystemLibraries[provider].GetFilesListing(id).AsQueryable().Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Filesystem).ToList();
    }

    public IList<WebFileBasic> GetFileSystemFilesByRange(int? provider, string id, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return FileSystemLibraries[provider].GetFilesListing(id).AsQueryable().Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Filesystem).ToList();
    }

    public WebIntResult GetFileSystemFilesCount(int? provider, string id, string filter = null)
    {
      return FileSystemLibraries[provider].GetFilesListing(id).AsQueryable().Filter(filter).Count();
    }

    public IList<WebFilesystemItem> GetFileSystemFilesAndFolders(int? provider, string id, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      var listA = FileSystemLibraries[provider].GetFilesListing(id).AsQueryable().Select(x => x.ToWebFilesystemItem());
      var listB = FileSystemLibraries[provider].GetFoldersListing(id).AsQueryable().Select(x => x.ToWebFilesystemItem());
      return listA.Concat(listB).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Filesystem).ToList();
    }

    public IList<WebFilesystemItem> GetFileSystemFilesAndFoldersByRange(int? provider, string id, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      var listA = FileSystemLibraries[provider].GetFilesListing(id).AsQueryable().Select(x => x.ToWebFilesystemItem());
      var listB = FileSystemLibraries[provider].GetFoldersListing(id).AsQueryable().Select(x => x.ToWebFilesystemItem());
      return listA.Concat(listB).Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Filesystem).ToList();
    }

    public WebIntResult GetFileSystemFilesAndFoldersCount(int? provider, string id, string filter = null)
    {
      var listA = FileSystemLibraries[provider].GetFilesListing(id).AsQueryable().Select(x => x.ToWebFilesystemItem()).Filter(filter);
      var listB = FileSystemLibraries[provider].GetFoldersListing(id).AsQueryable().Select(x => x.ToWebFilesystemItem()).Filter(filter);
      return listA.Count() + listB.Count();
    }

    public WebDriveBasic GetFileSystemDriveBasicById(int? provider, string id)
    {
      return FileSystemLibraries[provider].GetDriveBasic(id).Finalize(provider, ProviderType.Filesystem);
    }

    public WebFolderBasic GetFileSystemFolderBasicById(int? provider, string id)
    {
      return FileSystemLibraries[provider].GetFolderBasic(id).Finalize(provider, ProviderType.Filesystem);
    }

    public WebFileBasic GetFileSystemFileBasicById(int? provider, string id)
    {
      return FileSystemLibraries[provider].GetFileBasic(id).Finalize(provider, ProviderType.Filesystem);
    }

    public WebBoolResult DeleteFile(int? provider, string id)
    {
      return FileSystemLibraries[provider].DeleteFile(id);
    }
    #endregion

    #region Files
    private IList<WebArtwork> GetOriginalArtworkList(int? provider, WebMediaType type, string id)
    {
      switch (type)
      {
        // Movie
        case WebMediaType.Movie:
          return MovieLibraries[provider].GetMovieDetailedById(id).Artwork;
        case WebMediaType.Collection:
          return MovieLibraries[provider].GetCollectionById(id).Artwork;
        // Mudic
        case WebMediaType.MusicAlbum:
          return MusicLibraries[provider].GetAlbumBasicById(id).Artwork;
        case WebMediaType.MusicArtist:
          return MusicLibraries[provider].GetArtistDetailedById(id).Artwork;
        case WebMediaType.MusicTrack:
          return MusicLibraries[provider].GetTrackDetailedById(id).Artwork;
        // TV Series
        case WebMediaType.TVEpisode:
          return TVShowLibraries[provider].GetEpisodeDetailed(id).Artwork;
        case WebMediaType.TVSeason:
          return TVShowLibraries[provider].GetSeasonDetailed(id).Artwork;
        case WebMediaType.TVShow:
          return TVShowLibraries[provider].GetTVShowDetailed(id).Artwork;
        // Picture
        case WebMediaType.Picture:
          return PictureLibraries[provider].GetPictureDetailed(id).Artwork;
        case WebMediaType.MobileVideo:
          return PictureLibraries[provider].GetMobileVideoBasic(id).Artwork;
        case WebMediaType.PictureFolder:
          return PictureLibraries[provider].GetPictureFolderById(id).Artwork;
        // File System
        case WebMediaType.Drive:
          return FileSystemLibraries[provider].GetDriveBasic(id).Artwork;
        case WebMediaType.Folder:
          return FileSystemLibraries[provider].GetFolderBasic(id).Artwork;
        case WebMediaType.File:
          return FileSystemLibraries[provider].GetFileBasic(id).Artwork;
        default:
          Log.Warn("Requested artwork for mediatype {0} which does not have artwork", type);
          throw new ArgumentException("No artwork available for this mediatype", "type");
      }
    }

    public IList<WebArtwork> GetArtwork(int? provider, WebMediaType type, string id)
    {
      return GetOriginalArtworkList(provider, type, id).Select(x => new WebArtwork()
      {
        Filetype = x.Filetype,
        Id = x.Id,
        Offset = x.Offset,
        Rating = x.Rating,
        Type = x.Type
      }).ToList();
    }

    public IList<string> GetPathList(int? provider, WebMediaType mediatype, WebFileType filetype, string id)
    {
      if (filetype != WebFileType.Content)
        return GetOriginalArtworkList(provider, mediatype, id).Where(x => x.Type == filetype).Select(x => ((WebArtworkDetailed)x).Path).ToList();
      else
        return GetMediaItem(provider, mediatype, id).Path;
    }

    public WebFileInfo GetFileInfo(int? provider, WebMediaType mediatype, WebFileType filetype, string id, int offset)
    {
      string path = "";
      try
      {
        path = GetPathList(provider, mediatype, filetype, id).ElementAt(offset);
        WebFileInfo retVal = null;

        bool tryImpersonation = false;
        try
        {
          // first try it the usual way
          retVal = GetLibrary(provider, mediatype).GetFileInfo(path).Finalize(provider, mediatype);
          tryImpersonation = PathUtil.MightBeOnNetworkDrive(path) && (retVal == null || !retVal.Exists);
        }
        catch (UnauthorizedAccessException)
        {
          // access denied, try impersonation
          tryImpersonation = true;
        }

        if (tryImpersonation && Configuration.Services.NetworkImpersonation.IsEnabled())
        {
          using (var context = NetworkContextFactory.CreateImpersonationContext())
          {
            retVal = new WebFileInfo(context.RewritePath(path));
            retVal.IsLocalFile = true;
            retVal.OnNetworkDrive = true;
          }
        }

        // Make sure to always the path property, even if the file doesn't exist. This makes debugging a lot easier, as you get actual paths in your logs now. 
        if (String.IsNullOrEmpty(retVal.Path))
          retVal.Path = PathUtil.StripFileProtocolPrefix(path);
        retVal.PID = ProviderHandler.GetProviderId(mediatype.ToProviderType(), provider);
        return retVal;
      }
      catch (ArgumentOutOfRangeException)
      {
        Log.Info("Cannot resolve mediatype={0}, filetype={1}, provider={2}, id={3}, offset={4}", mediatype, filetype, provider, id, offset);
      }
      catch (FileNotFoundException)
      {
        Log.Info("Failed to load fileinfo for non-existing file mediatype={0}, filetype={1}, provider={5}, id={2}, offset={3} (resulting in path={4})", mediatype, filetype, id, offset, path, provider);
      }
      catch (Exception ex)
      {
        Log.Info(String.Format("Failed to load fileinfo for mediatype={0}, filetype={1}, provider={5}, id={2}, offset={3} (resulting in path={4})", mediatype, filetype, id, offset, path, provider), ex);
      }

      return new WebFileInfo()
      {
        Exists = false,
        PID = ProviderHandler.GetProviderId(mediatype.ToProviderType(), provider),
        Path = String.IsNullOrWhiteSpace(path) ? null : PathUtil.StripFileProtocolPrefix(path)
      };
    }

    public WebBoolResult IsLocalFile(int? provider, WebMediaType mediatype, WebFileType filetype, string id, int offset)
    {
      WebFileInfo info = GetFileInfo(provider, mediatype, filetype, id, offset);
      return info.Exists && info.IsLocalFile;
    }

    public Stream RetrieveFile(int? provider, WebMediaType mediatype, WebFileType filetype, string id, int offset)
    {
      try
      {
        string path = GetPathList(provider, mediatype, filetype, id).ElementAt(offset);
        WebFileInfo info = GetFileInfo(provider, mediatype, filetype, id, offset);

        // first try to read the file from the filesystem
        if (info.Exists && info.IsLocalFile && !info.OnNetworkDrive && File.Exists(path))
          return new FileStream(path, FileMode.Open, FileAccess.Read);

        // maybe the plugin has some magic
        if (info.Exists && !info.IsLocalFile)
          return GetLibrary(provider, mediatype).GetFile(path);

        // try to load it from a network drive
        if (info.Exists && info.IsLocalFile && info.OnNetworkDrive)
        {
          using (var context = NetworkContextFactory.Create())
            return new FileStream(context.RewritePath(path), FileMode.Open, FileAccess.Read);
        }

        // fail
        Log.Warn("Requested non-existing or non-accessible file mediatype={0} filetype={1} id={2} offset={3}", mediatype, filetype, id, offset);
        WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
        return Stream.Null;
      }
      catch (Exception ex)
      {
        Log.Info("Failed to retrieve file for mediatype=" + mediatype + ", filetype=" + filetype + ", id=" + id + " and offset=" + offset, ex);
        WCFUtil.SetResponseCode(System.Net.HttpStatusCode.InternalServerError);
        return Stream.Null;
      }
    }
    #endregion

    #region Playlist
    private IEnumerable<WebPlaylistItem> GetAllPlaylistItems(int? provider, string playlistId)
    {
      return PlaylistLibraries[provider].GetPlaylistItems(playlistId);
    }

    public IList<WebPlaylist> GetPlaylists(int? provider, string filter = null)
    {
      return PlaylistLibraries[provider].GetPlaylists().Filter(filter).Finalize(provider, ProviderType.Music);
    }

    public IList<WebPlaylistItem> GetPlaylistItems(int? provider, string playlistId, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return PlaylistLibraries[provider].GetPlaylistItems(playlistId).Filter(filter).Finalize(provider, ProviderType.Music);
    }

    public IList<WebPlaylistItem> GetPlaylistItemsByRange(int? provider, string playlistId, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
    {
      return GetAllPlaylistItems(provider, playlistId).AsQueryable().Filter(filter).TakeRange(start, end).Finalize(provider, ProviderType.Music);
    }

    public WebIntResult GetPlaylistItemsCount(int? provider, string playlistId, string filter = null)
    {
      return PlaylistLibraries[provider].GetPlaylistItems(playlistId).AsQueryable().Filter(filter).Count();
    }

    public WebBoolResult AddPlaylistItem(int? provider, string playlistId, WebMediaType type, string id, int? position)
    {
      IList<WebPlaylistItem> playlist = GetAllPlaylistItems(provider, playlistId).Finalize(provider, type);

      if (AddPlaylistItemToPlaylist(provider, id, position, playlist))
      {
        return PlaylistLibraries[provider].SavePlaylist(playlistId, playlist);
      }
      else
      {
        return false;
      }
    }

    private bool AddPlaylistItemToPlaylist(int? provider, string id, int? position, IList<WebPlaylistItem> playlist)
    {
      WebMusicTrackBasic track = MusicLibraries[provider].GetTrackBasicById(id);
      if (position != null)
      {
        if (position >= 0 && position <= playlist.Count)
        {
          playlist.Insert((int)position, new WebPlaylistItem(track));
        }
        else
        {
          Log.Warn("Index out of bound for adding playlist item: " + position);
          return false;
        }
      }
      else
      {
        playlist.Add(new Interfaces.Playlist.WebPlaylistItem(track));
      }
      return true;
    }

    public WebBoolResult AddPlaylistItems(int? provider, string playlistId, WebMediaType type, int? position, string ids)
    {
      IList<WebPlaylistItem> playlist = GetAllPlaylistItems(provider, playlistId).Finalize(provider, type);
      int pos = position != null ? (int)position : playlist.Count - 1;
      string[] splitIds = ids.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
      for (int i = 0; i < splitIds.Length; i++)
      {
        AddPlaylistItemToPlaylist(provider, splitIds[i], pos + i, playlist);
      }
      return PlaylistLibraries[provider].SavePlaylist(playlistId, playlist);
    }

    public WebBoolResult ClearAndAddPlaylistItems(int? provider, string playlistId, WebMediaType type, int? position, string ids)
    {
      IList<WebPlaylistItem> playlist = GetAllPlaylistItems(provider, playlistId).Finalize(provider, type);
      playlist.Clear();
      string[] splitIds = ids.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
      for (int i = 0; i < splitIds.Length; i++)
      {
        AddPlaylistItemToPlaylist(provider, splitIds[i], i, playlist);
      }
      return PlaylistLibraries[provider].SavePlaylist(playlistId, playlist);
    }

    public WebBoolResult RemovePlaylistItem(int? provider, string playlistId, int position)
    {
      IList<WebPlaylistItem> playlist = GetAllPlaylistItems(provider, playlistId).ToList();

      if (position >= 0 && position < playlist.Count)
      {
        playlist.RemoveAt(position);
      }
      else
      {
        Log.Warn("Index out of bound for removing playlist item: " + position);
        return false;
      }
      return PlaylistLibraries[provider].SavePlaylist(playlistId, playlist);
    }

    public WebBoolResult RemovePlaylistItems(int? provider, string playlistId, string positions)
    {
      IList<WebPlaylistItem> playlist = GetAllPlaylistItems(provider, playlistId).ToList();
      string[] splitIds = positions.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
      foreach (string p in splitIds)
      {
        int pos = Int32.Parse(p);
        if (pos >= 0 && pos < playlist.Count)
        {
          playlist.RemoveAt(pos);
        }
        else
        {
          Log.Warn("Index out of bound for removing playlist item: " + p);
          return new WebBoolResult(false);
        }
      }
      return PlaylistLibraries[provider].SavePlaylist(playlistId, playlist);
    }

    public WebBoolResult MovePlaylistItem(int? provider, string playlistId, int oldPosition, int newPosition)
    {
      IList<WebPlaylistItem> playlist = GetAllPlaylistItems(provider, playlistId).ToList();
      if (oldPosition >= 0 && oldPosition < playlist.Count && newPosition >= 0 && newPosition < playlist.Count)
      {
        WebPlaylistItem item = playlist[oldPosition];
        playlist.RemoveAt(oldPosition);
        playlist.Insert(newPosition, item);
        return PlaylistLibraries[provider].SavePlaylist(playlistId, playlist);
      }
      else
      {
        Log.Warn("Indexes out of bound for moving playlist item");
      }
      return false;
    }

    public WebStringResult CreatePlaylist(int? provider, string playlistName)
    {
      return PlaylistLibraries[provider].CreatePlaylist(playlistName);
    }

    public WebBoolResult DeletePlaylist(int? provider, string playlistId)
    {
      return PlaylistLibraries[provider].DeletePlaylist(playlistId);
    }
    #endregion

    #region Filters
    /// <summary>
    /// Get all available values for a given field
    /// </summary>
    public IList<string> GetFilterValues(int? provider, WebMediaType mediaType, string filterField, string op, int? limit, WebSortOrder? order = WebSortOrder.Asc)
    {
      switch (mediaType)
      {
        case WebMediaType.Drive:
          return AutoSuggestion.GetValuesForField(filterField, GetFileSystemDrives(provider), op, limit).OrderBy(x => x, order).ToList();
        case WebMediaType.Movie:
          return AutoSuggestion.GetValuesForField(filterField, GetMoviesDetailed(provider), op, limit).OrderBy(x => x, order).ToList();
        case WebMediaType.Collection:
          return AutoSuggestion.GetValuesForField(filterField, GetCollections(provider), op, limit).OrderBy(x => x, order).ToList();
        case WebMediaType.MusicAlbum:
          return AutoSuggestion.GetValuesForField(filterField, GetMusicAlbumsBasic(provider), op, limit).OrderBy(x => x, order).ToList();
        case WebMediaType.MusicArtist:
          return AutoSuggestion.GetValuesForField(filterField, GetMusicArtistsDetailed(provider), op, limit).OrderBy(x => x, order).ToList();
        case WebMediaType.MusicTrack:
          return AutoSuggestion.GetValuesForField(filterField, GetMusicTracksDetailed(provider), op, limit).OrderBy(x => x, order).ToList();
        case WebMediaType.Picture:
          return AutoSuggestion.GetValuesForField(filterField, GetPicturesDetailed(provider), op, limit).OrderBy(x => x, order).ToList();
        case WebMediaType.MobileVideo:
          return AutoSuggestion.GetValuesForField(filterField, GetPicturesDetailed(provider), op, limit).OrderBy(x => x, order).ToList();
        case WebMediaType.PictureFolder:
          return AutoSuggestion.GetValuesForField(filterField, GetPicturesDetailed(provider), op, limit).OrderBy(x => x, order).ToList();
        case WebMediaType.Playlist:
          return AutoSuggestion.GetValuesForField(filterField, GetPlaylists(provider), op, limit).OrderBy(x => x, order).ToList();
        case WebMediaType.TVEpisode:
          return AutoSuggestion.GetValuesForField(filterField, GetTVEpisodesDetailed(provider), op, limit).OrderBy(x => x, order).ToList();
        case WebMediaType.TVShow:
          return AutoSuggestion.GetValuesForField(filterField, GetTVShowsDetailed(provider), op, limit).OrderBy(x => x, order).ToList();
        default:
          Log.Info("GetFilterValues() called with unsupported mediaType='{0}' filterField='{1}' op='{2}' limit='{3}'", mediaType, filterField, op, limit);
          throw new ArgumentException("Unsupported MediaType for GetFilterValues()");
      }
    }

    public WebIntResult GetFilterValuesCount(int? provider, WebMediaType mediaType, string filterField, string op = null, int? limit = null)
    {
      return GetFilterValues(provider, mediaType, filterField, op, limit).Count();
    }

    public IList<string> GetFilterValuesByRange(int? provider, int start, int end, WebMediaType mediaType, string filterField, string op = null, int? limit = null, WebSortOrder? order = WebSortOrder.Asc)
    {
      return GetFilterValues(provider, mediaType, filterField, op, limit, order).TakeRange(start, end).ToList();
    }

    /// <summary>
    /// Create a filter string from a given set of parameters. The result of this method can be used as the "filter"
    /// parameter in other MPExtended APIs.
    /// 
    /// A filter consists of a field name (alphabetic, case-sensitive), followed by an operator (only special characters),
    /// followed by the value. Multiple filters are separated with a comma. 
    /// 
    /// To define multiple filters, call this method multiple times and join them together. 
    /// </summary>
    /// <param name="conjunction">Conjuction (and, or) to the next filter (null for the last filter)</param>
    /// <returns>The filter string</returns>
    public WebStringResult CreateFilterString(string field, string op, string value, string conjunction)
    {
      string val = value.Replace("\\", "\\\\").Replace("'", "\\'");
      return conjunction == null ?
          String.Format("{0}{1}'{2}'", field, op, val) :
          String.Format("{0}{1}'{2}'{3} ", field, op, val, conjunction == "and" ? "," : "|");
    }

    /// <summary>
    /// Get a list of all available filter operators (==, !=, ...)
    /// </summary>
    /// <returns>Available operators</returns>
    public IList<WebFilterOperator> GetFilterOperators()
    {
      return Operator.GetAll().Select(x => new WebFilterOperator()
      {
        Operator = x.Syntax,
        SuitableTypes = x.Types.ToList(),
        Title = x.Name
      }).ToList();
    }
    #endregion
  }
}
