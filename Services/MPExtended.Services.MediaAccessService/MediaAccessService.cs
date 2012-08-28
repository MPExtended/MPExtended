#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
        private const int API_VERSION = 4;
        List<WebFilterOperator> operators;

        public MediaAccessService()
        {
            operators = new List<WebFilterOperator>();
            operators.Add(new WebFilterOperator() { Operator = "==", Title = "equals", SuitableTypes = new List<string>() { "text", "num", "bool" } });
            operators.Add(new WebFilterOperator() { Operator = "!=", Title = "not", SuitableTypes = new List<string>() { "text", "num", "bool" } });
            operators.Add(new WebFilterOperator() { Operator = ">", Title = "greater than", SuitableTypes = new List<string>() { "text", "num" } });
            operators.Add(new WebFilterOperator() { Operator = "<", Title = "less than", SuitableTypes = new List<string>() { "text", "num" } });
            operators.Add(new WebFilterOperator() { Operator = ">=", Title = "greater or equal than", SuitableTypes = new List<string>() { "text", "num" } });
            operators.Add(new WebFilterOperator() { Operator = "=<", Title = "less or equal than", SuitableTypes = new List<string>() { "text", "num" } });

            //...
            operators.Add(new WebFilterOperator() { Operator = "c", Title = "contains", SuitableTypes = new List<string>() { "list" } });
            operators.Add(new WebFilterOperator() { Operator = "s", Title = "starts with", SuitableTypes = new List<string>() { "text" } });
        }

        private ILibrary GetLibrary(int? provider, WebMediaType type)
        {
            switch (type)
            {
                case WebMediaType.Movie:
                    return MovieLibraries[provider];
                case WebMediaType.MusicTrack:
                case WebMediaType.MusicAlbum:
                case WebMediaType.MusicArtist:
                    return MusicLibraries[provider];
                case WebMediaType.Picture:
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
            return new WebMediaServiceDescription()
            {
                ApiVersion = API_VERSION,
                ServiceVersion = VersionUtil.GetVersionName(),

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

        public WebBoolResult TestConnectionToTVService()
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
                case WebMediaType.TVEpisode:
                    return GetTVEpisodeDetailedById(provider, id).Finalize(provider, ProviderType.TVShow).ToWebMediaItem();
                case WebMediaType.Drive:
                    return GetFileSystemDriveBasicById(provider, id).Finalize(provider, ProviderType.Filesystem).ToWebMediaItem();
                case WebMediaType.Folder:
                    return GetFileSystemFolderBasicById(provider, id).Finalize(provider, ProviderType.Filesystem).ToWebMediaItem();
                case WebMediaType.File:
                    return GetFileSystemFileBasicById(provider, id).Finalize(provider, ProviderType.Filesystem).ToWebMediaItem();
                default:
                    throw new ArgumentException();
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
        #endregion

        #region Movies
        public IList<WebCategory> GetAllMovieCategories(int? provider, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MovieLibraries[provider].GetAllCategories().AsQueryable().SortMediaItemList(sort, order).Finalize(provider, ProviderType.Movie);
        }

        public WebIntResult GetMovieCount(int? provider, string genre, string actor = null, string startsWith = null, string filter = null)
        {
            return MovieLibraries[provider].GetAllMovies().AsQueryable().CommonFilter(genre, actor, startsWith).Filter(filter).Count();
        }

        public IList<WebMovieBasic> GetAllMoviesBasic(int? provider, string genre = null, string actor = null, string startsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MovieLibraries[provider].GetAllMovies().AsQueryable().CommonFilter(genre, actor, startsWith).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Movie);
        }

        public IList<WebMovieDetailed> GetAllMoviesDetailed(int? provider, string genre = null, string actor = null, string startsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MovieLibraries[provider].GetAllMoviesDetailed().AsQueryable().CommonFilter(genre, actor, startsWith).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Movie);
        }

        public IList<WebMovieBasic> GetMoviesBasicByRange(int? provider, int start, int end, string genre = null, string actor = null, string startsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MovieLibraries[provider].GetAllMovies().AsQueryable().CommonFilter(genre, actor, startsWith).Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Movie);
        }

        public IList<WebMovieDetailed> GetMoviesDetailedByRange(int? provider, int start, int end, string genre = null, string actor = null, string startsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MovieLibraries[provider].GetAllMoviesDetailed().AsQueryable().CommonFilter(genre, actor, startsWith).Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Movie);
        }

        public IList<WebGenre> GetAllMovieGenres(int? provider, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MovieLibraries[provider].GetAllGenres().AsQueryable().SortMediaItemList(sort, order).Finalize(provider, ProviderType.Movie).ToList();
        }

        public IList<WebGenre> GetMovieGenresByRange(int? provider, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MovieLibraries[provider].GetAllGenres().AsQueryable().SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Movie);
        }

        public WebIntResult GetMovieGenresCount(int? provider)
        {
            return MovieLibraries[provider].GetAllGenres().AsQueryable().Count();
        }

        public WebMovieBasic GetMovieBasicById(int? provider, string id)
        {
            return MovieLibraries[provider].GetMovieBasicById(id).Finalize(provider, ProviderType.Movie);
        }

        public WebMovieDetailed GetMovieDetailedById(int? provider, string id)
        {
            return MovieLibraries[provider].GetMovieDetailedById(id).Finalize(provider, ProviderType.Movie);
        }

        public IList<WebActor> GetAllMovieActors(int? provider, string startsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MovieLibraries[provider].GetAllMovies().AsQueryable().SelectMany(x => x.Actors).Distinct().FilterStartsWith(startsWith).Filter(filter).SortMediaItemList(sort, order, WebSortField.Title).Finalize(provider, ProviderType.Movie);
        }

        public IList<WebActor> GetMovieActorsByRange(int? provider, int start, int end, string startsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MovieLibraries[provider].GetAllMovies().AsQueryable().SelectMany(x => x.Actors).Distinct().FilterStartsWith(startsWith).Filter(filter).SortMediaItemList(sort, order, WebSortField.Title).TakeRange(start, end).Finalize(provider, ProviderType.Movie);
        }

        public WebIntResult GetMovieActorCount(int? provider, string startsWith = null, string filter = null)
        {
            return GetAllMovieActors(provider).AsQueryable().FilterStartsWith(startsWith).Filter(filter).Count();
        }
        #endregion

        #region Music
        public IList<WebCategory> GetAllMusicCategories(int? provider)
        {
            return MusicLibraries[provider].GetAllCategories().AsQueryable().Finalize(provider, ProviderType.Music);
        }

        public WebIntResult GetMusicTrackCount(int? provider, string genre = null, string beginsWith = null, string filter = null)
        {
            return MusicLibraries[provider].GetAllTracks().AsQueryable().FilterGenre(genre).FilterStartsWith(beginsWith).Filter(filter).Count();
        }

        public WebIntResult GetMusicAlbumCount(int? provider, string genre = null, string beginsWith = null, string filter = null)
        {
            return MusicLibraries[provider].GetAllAlbums().AsQueryable().FilterGenre(genre).FilterStartsWith(beginsWith).Filter(filter).Count();
        }

        public WebIntResult GetMusicArtistCount(int? provider, string beginsWith = null, string filter = null)
        {
            return MusicLibraries[provider].GetAllArtists().AsQueryable().FilterStartsWith(beginsWith).Filter(filter).Count();
        }

        public IList<WebMusicTrackBasic> GetAllMusicTracksBasic(int? provider, string genre = null, string beginsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllTracks().AsQueryable().FilterGenre(genre).FilterStartsWith(beginsWith).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicTrackDetailed> GetAllMusicTracksDetailed(int? provider, string genre = null, string beginsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllTracksDetailed().AsQueryable().FilterGenre(genre).FilterStartsWith(beginsWith).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicTrackBasic> GetMusicTracksBasicByRange(int? provider, int start, int end, string genre = null, string beginsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllTracks().AsQueryable().FilterGenre(genre).FilterStartsWith(beginsWith).Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicTrackDetailed> GetMusicTracksDetailedByRange(int? provider, int start, int end, string genre = null, string beginsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllTracksDetailed().AsQueryable().FilterGenre(genre).FilterStartsWith(beginsWith).Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
        }

        public WebMusicTrackBasic GetMusicTrackBasicById(int? provider, string id)
        {
            return MusicLibraries[provider].GetTrackBasicById(id).Finalize(provider, ProviderType.Music);
        }

        public IList<WebGenre> GetAllMusicGenres(int? provider, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllGenres().AsQueryable().SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public IList<WebGenre> GetMusicGenresByRange(int? provider, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllGenres().AsQueryable().SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
        }

        public WebIntResult GetMusicGenresCount(int? provider)
        {
            return MusicLibraries[provider].GetAllGenres().AsQueryable().Count();
        }

        public WebMusicTrackDetailed GetMusicTrackDetailedById(int? provider, string id)
        {
            return MusicLibraries[provider].GetTrackDetailedById(id).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicAlbumBasic> GetAllMusicAlbumsBasic(int? provider, string genre = null, string beginsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllAlbums().AsQueryable().FilterGenre(genre).FilterStartsWith(beginsWith).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByRange(int? provider, int start, int end, string genre = null, string beginsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllAlbums().AsQueryable().FilterGenre(genre).FilterStartsWith(beginsWith).Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicArtistBasic> GetAllMusicArtistsBasic(int? provider, string beginsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllArtists().AsQueryable().FilterStartsWith(beginsWith).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicArtistBasic> GetMusicArtistsBasicByRange(int? provider, int start, int end, string beginsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllArtists().AsQueryable().FilterStartsWith(beginsWith).Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
        }

        public WebMusicArtistBasic GetMusicArtistBasicById(int? provider, string id)
        {
            return MusicLibraries[provider].GetArtistBasicById(id).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicArtistDetailed> GetAllMusicArtistsDetailed(int? provider, string beginsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllArtistsDetailed().AsQueryable().FilterStartsWith(beginsWith).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicArtistDetailed> GetMusicArtistsDetailedByRange(int? provider, int start, int end, string beginsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllArtistsDetailed().AsQueryable().FilterStartsWith(beginsWith).Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
        }

        public WebMusicArtistDetailed GetMusicArtistDetailedById(int? provider, string id)
        {
            return MusicLibraries[provider].GetArtistDetailedById(id).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicTrackBasic> GetMusicTracksBasicForAlbum(int? provider, string id, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllTracks().AsQueryable().Where(p => p.AlbumId == id).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicTrackDetailed> GetMusicTracksDetailedForAlbum(int? provider, string id, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllTracksDetailed().AsQueryable().Where(p => p.AlbumId == id).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public WebMusicAlbumBasic GetMusicAlbumBasicById(int? provider, string id)
        {
            return MusicLibraries[provider].GetAlbumBasicById(id).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicAlbumBasic> GetMusicAlbumsBasicForArtist(int? provider, string id, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return MusicLibraries[provider].GetAllAlbums().AsQueryable().Where(p => p.AlbumArtistId == id).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }
        #endregion

        #region Pictures
        public WebIntResult GetPictureCount(int? provider)
        {
            return PictureLibraries[provider].GetAllPicturesBasic().AsQueryable().Count();
        }

        public IList<WebPictureBasic> GetAllPicturesBasic(int? provider, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return PictureLibraries[provider].GetAllPicturesBasic().AsQueryable().SortMediaItemList(sort, order).Finalize(provider, ProviderType.Picture);
        }

        public IList<WebPictureDetailed> GetAllPicturesDetailed(int? provider, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return PictureLibraries[provider].GetAllPicturesDetailed().AsQueryable().SortMediaItemList(sort, order).Finalize(provider, ProviderType.Picture);
        }

        public IList<WebCategory> GetAllPictureCategories(int? provider)
        {
            return PictureLibraries[provider].GetAllPictureCategories().AsQueryable().Finalize(provider, ProviderType.Picture);
        }

        public IList<WebCategory> GetPictureSubCategories(int? provider, string id)
        {
            return PictureLibraries[provider].GetSubCategoriesById(id).AsQueryable().Finalize(provider, ProviderType.Picture);
        }

        public IList<WebPictureBasic> GetPicturesBasicByCategory(int? provider, string id)
        {
            return PictureLibraries[provider].GetPicturesBasicByCategory(id).AsQueryable().Finalize(provider, ProviderType.Picture);
        }

        public IList<WebPictureDetailed> GetPicturesDetailedByCategory(int? provider, string id)
        {
            return PictureLibraries[provider].GetPicturesDetailedByCategory(id).AsQueryable().Finalize(provider, ProviderType.Picture);
        }

        public WebPictureBasic GetPictureBasicById(int? provider, string id)
        {
            return PictureLibraries[provider].GetPictureBasic(id).Finalize(provider, ProviderType.Picture);
        }

        public WebPictureDetailed GetPictureDetailedById(int? provider, string id)
        {
            return PictureLibraries[provider].GetPictureDetailed(id).Finalize(provider, ProviderType.Picture);
        }
        #endregion

        #region TVShows
        public IList<WebCategory> GetAllTVShowCategories(int? provider)
        {
            return TVShowLibraries[provider].GetAllCategories().AsQueryable().Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebGenre> GetAllTVShowGenres(int? provider, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllGenres().AsQueryable().SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebGenre> GetTVShowGenresByRange(int? provider, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllGenres().AsQueryable().SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
        }

        public WebIntResult GetTVShowGenresCount(int? provider)
        {
            return TVShowLibraries[provider].GetAllGenres().AsQueryable().Count();
        }

        public IList<WebTVShowBasic> GetAllTVShowsBasic(int? provider, string genre = null, string actor = null, string startsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsBasic().AsQueryable().CommonFilter(genre, actor, startsWith).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVShowDetailed> GetAllTVShowsDetailed(int? provider, string genre = null, string actor = null, string startsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsDetailed().AsQueryable().CommonFilter(genre, actor, startsWith).Filter(filter).SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVShowBasic> GetTVShowsBasicByRange(int? provider, int start, int end, string genre = null, string actor = null, string startsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsBasic().AsQueryable().CommonFilter(genre, actor, startsWith).Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVShowDetailed> GetTVShowsDetailedByRange(int? provider, int start, int end, string genre = null, string actor = null, string startsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsDetailed().AsQueryable().CommonFilter(genre, actor, startsWith).Filter(filter).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
        }

        public WebTVShowDetailed GetTVShowDetailedById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetTVShowDetailed(id).Finalize(provider, ProviderType.TVShow);
        }

        public WebTVShowBasic GetTVShowBasicById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetTVShowBasic(id).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVSeasonBasic> GetTVSeasonsBasicForTVShow(int? provider, string id, WebSortField? sort = WebSortField.TVSeasonNumber, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllSeasonsBasic().AsQueryable().Where(x => x.ShowId == id).SortMediaItemList(sort, order, WebSortField.TVSeasonNumber).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVSeasonDetailed> GetTVSeasonsDetailedForTVShow(int? provider, string id, WebSortField? sort = WebSortField.TVSeasonNumber, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllSeasonsDetailed().AsQueryable().Where(x => x.ShowId == id).SortMediaItemList(sort, order, WebSortField.TVSeasonNumber).Finalize(provider, ProviderType.TVShow);
        }

        public WebTVSeasonDetailed GetTVSeasonDetailedById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetSeasonDetailed(id).Finalize(provider, ProviderType.TVShow);
        }

        public WebTVSeasonBasic GetTVSeasonBasicById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetSeasonBasic(id).Finalize(provider, ProviderType.TVShow);
        }


        public IList<WebTVEpisodeBasic> GetAllTVEpisodesBasic(int? provider, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeDetailed> GetAllTVEpisodesDetailed(int? provider, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesDetailed().AsQueryable().SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesBasicByRange(int? provider, int start, int end, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedByRange(int? provider, int start, int end, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesDetailed().AsQueryable().SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShow(int? provider, string id, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().Where(p => p.ShowId == id).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShow(int? provider, string id, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesDetailed().AsQueryable().Where(p => p.ShowId == id).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShowByRange(int? provider, string id, int start, int end, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().Where(p => p.ShowId == id).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).TakeRange(start, end - start).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShowByRange(int? provider, string id, int start, int end, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesDetailed().AsQueryable().Where(p => p.ShowId == id).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).TakeRange(start, end - start).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesBasicForSeason(int? provider, string id, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().Where(p => p.SeasonId == id).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForSeason(int? provider, string id, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesDetailed().AsQueryable().Where(p => p.SeasonId == id).SortMediaItemList(sort, order, WebSortField.TVEpisodeNumber).Finalize(provider, ProviderType.TVShow);
        }

        public WebTVEpisodeBasic GetTVEpisodeBasicById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetEpisodeBasic(id).Finalize(provider, ProviderType.TVShow);
        }

        public WebTVEpisodeDetailed GetTVEpisodeDetailedById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetEpisodeDetailed(id).Finalize(provider, ProviderType.TVShow);
        }

        public WebIntResult GetTVEpisodeCount(int? provider)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().Count();
        }

        public WebIntResult GetTVEpisodeCountForTVShow(int? provider, string id)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().Where(e => e.ShowId == id).Count();
        }

        public WebIntResult GetTVEpisodeCountForSeason(int? provider, string id)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().AsQueryable().Where(e => e.SeasonId == id).Count();
        }

        public WebIntResult GetTVShowCount(int? provider, string genre = null, string actor = null, string startsWith = null, string filter = null)
        {
            return TVShowLibraries[provider].GetAllTVShowsBasic().AsQueryable().CommonFilter(genre, actor, startsWith).Filter(filter).Count();
        }

        public WebIntResult GetTVSeasonCountForTVShow(int? provider, string id)
        {
            return TVShowLibraries[provider].GetAllSeasonsBasic().AsQueryable().Where(x => x.ShowId == id).Count();
        }

        public IList<WebActor> GetAllTVShowActors(int? provider, string startsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsBasic().AsQueryable().SelectMany(x => x.Actors).Distinct().FilterStartsWith(startsWith).Filter(filter).SortMediaItemList(sort, order, WebSortField.Title).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebActor> GetTVShowActorsByRange(int? provider, int start, int end, string startsWith = null, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsBasic().AsQueryable().SelectMany(x => x.Actors).Distinct().FilterStartsWith(startsWith).Filter(filter).SortMediaItemList(sort, order, WebSortField.Title).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
        }

        public WebIntResult GetTVShowActorCount(int? provider, string startsWith = null, string filter = null)
        {
            return GetAllTVShowActors(provider).AsQueryable().FilterStartsWith(startsWith).Filter(filter).Count();
        }
        #endregion

        #region Filesystem
        public WebIntResult GetFileSystemDriveCount(int? provider)
        {
            return FileSystemLibraries[provider].GetDriveListing().AsQueryable().Count();
        }

        public IList<WebDriveBasic> GetAllFileSystemDrives(int? provider, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return FileSystemLibraries[provider].GetDriveListing().AsQueryable().SortMediaItemList(sort, order).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public IList<WebDriveBasic> GetFileSystemDrivesByRange(int? provider, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return FileSystemLibraries[provider].GetDriveListing().AsQueryable().SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public IList<WebFolderBasic> GetAllFileSystemFolders(int? provider, string id, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return FileSystemLibraries[provider].GetFoldersListing(id).AsQueryable().SortMediaItemList(sort, order).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public IList<WebFolderBasic> GetFileSystemFoldersByRange(int? provider, string id, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return FileSystemLibraries[provider].GetFoldersListing(id).AsQueryable().SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public WebIntResult GetFileSystemFoldersCount(int? provider, string id)
        {
            return FileSystemLibraries[provider].GetFoldersListing(id).AsQueryable().Count();
        }

        public IList<WebFileBasic> GetAllFileSystemFiles(int? provider, string id, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return FileSystemLibraries[provider].GetFilesListing(id).AsQueryable().SortMediaItemList(sort, order).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public IList<WebFileBasic> GetFileSystemFilesByRange(int? provider, string id, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return FileSystemLibraries[provider].GetFilesListing(id).AsQueryable().SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public WebIntResult GetFileSystemFilesCount(int? provider, string id)
        {
            return FileSystemLibraries[provider].GetFilesListing(id).AsQueryable().Count();
        }

        public IList<WebFilesystemItem> GetAllFileSystemFilesAndFolders(int? provider, string id, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            var listA = FileSystemLibraries[provider].GetFilesListing(id).AsQueryable().Select(x => x.ToWebFilesystemItem());
            var listB = FileSystemLibraries[provider].GetFoldersListing(id).AsQueryable().Select(x => x.ToWebFilesystemItem());
            return listA.Concat(listB).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public IList<WebFilesystemItem> GetFileSystemFilesAndFoldersByRange(int? provider, string id, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            var listA = FileSystemLibraries[provider].GetFilesListing(id).AsQueryable().Select(x => x.ToWebFilesystemItem());
            var listB = FileSystemLibraries[provider].GetFoldersListing(id).AsQueryable().Select(x => x.ToWebFilesystemItem());
            return listA.Concat(listB).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public WebIntResult GetFileSystemFilesAndFoldersCount(int? provider, string id)
        {
            var listA = FileSystemLibraries[provider].GetFilesListing(id).AsQueryable().Select(x => x.ToWebFilesystemItem());
            var listB = FileSystemLibraries[provider].GetFoldersListing(id).AsQueryable().Select(x => x.ToWebFilesystemItem());
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
        #endregion

        #region Files
        public IList<string> GetPathList(int? provider, WebMediaType mediatype, WebFileType filetype, string id)
        {
            if (mediatype == WebMediaType.File && filetype == WebFileType.Content)
                return FileSystemLibraries[provider].GetFileBasic(id).Path;
            else if (mediatype == WebMediaType.Movie && filetype == WebFileType.Content)
                return MovieLibraries[provider].GetMovieDetailedById(id).Path;
            else if (mediatype == WebMediaType.Movie && filetype == WebFileType.Backdrop)
                return MovieLibraries[provider].GetMovieDetailedById(id).Artwork.Where(x => x.Type == WebFileType.Backdrop).Select(x => ((WebArtworkDetailed)x).Path).ToList();
            else if (mediatype == WebMediaType.Movie && filetype == WebFileType.Cover)
                return MovieLibraries[provider].GetMovieDetailedById(id).Artwork.Where(x => x.Type == WebFileType.Cover).Select(x => ((WebArtworkDetailed)x).Path).ToList();
            else if (mediatype == WebMediaType.TVShow && filetype == WebFileType.Banner)
                return TVShowLibraries[provider].GetTVShowDetailed(id).Artwork.Where(x => x.Type == WebFileType.Banner).Select(x => ((WebArtworkDetailed)x).Path).ToList();
            else if (mediatype == WebMediaType.TVShow && filetype == WebFileType.Backdrop)
                return TVShowLibraries[provider].GetTVShowDetailed(id).Artwork.Where(x => x.Type == WebFileType.Backdrop).Select(x => ((WebArtworkDetailed)x).Path).ToList();
            else if (mediatype == WebMediaType.TVShow && filetype == WebFileType.Poster)
                return TVShowLibraries[provider].GetTVShowDetailed(id).Artwork.Where(x => x.Type == WebFileType.Poster).Select(x => ((WebArtworkDetailed)x).Path).ToList();
            else if (mediatype == WebMediaType.TVSeason && filetype == WebFileType.Backdrop)
                return TVShowLibraries[provider].GetSeasonDetailed(id).Artwork.Where(x => x.Type == WebFileType.Backdrop).Select(x => ((WebArtworkDetailed)x).Path).ToList();
            else if (mediatype == WebMediaType.TVSeason && filetype == WebFileType.Banner)
                return TVShowLibraries[provider].GetSeasonDetailed(id).Artwork.Where(x => x.Type == WebFileType.Banner).Select(x => ((WebArtworkDetailed)x).Path).ToList();
            else if (mediatype == WebMediaType.TVSeason && filetype == WebFileType.Poster)
                return TVShowLibraries[provider].GetSeasonDetailed(id).Artwork.Where(x => x.Type == WebFileType.Poster).Select(x => ((WebArtworkDetailed)x).Path).ToList();
            else if (mediatype == WebMediaType.TVEpisode && filetype == WebFileType.Content)
                return TVShowLibraries[provider].GetEpisodeBasic(id).Path;
            else if (mediatype == WebMediaType.TVEpisode && filetype == WebFileType.Banner)
                return TVShowLibraries[provider].GetEpisodeDetailed(id).Artwork.Where(x => x.Type == WebFileType.Banner).Select(x => ((WebArtworkDetailed)x).Path).ToList();
            else if (mediatype == WebMediaType.Picture && filetype == WebFileType.Content)
                return PictureLibraries[provider].GetPictureDetailed(id).Path;
            else if (mediatype == WebMediaType.MusicArtist && filetype == WebFileType.Cover)
                return MusicLibraries[provider].GetArtistBasicById(id).Artwork.Where(x => x.Type == WebFileType.Cover).Select(x => ((WebArtworkDetailed)x).Path).ToList();
            else if (mediatype == WebMediaType.MusicAlbum && filetype == WebFileType.Cover)
                return MusicLibraries[provider].GetAlbumBasicById(id).Artwork.Where(x => x.Type == WebFileType.Cover).Select(x => ((WebArtworkDetailed)x).Path).ToList();
            else if (mediatype == WebMediaType.MusicTrack && filetype == WebFileType.Content)
                return MusicLibraries[provider].GetTrackBasicById(id).Path;

            Log.Warn("Invalid combination of filetype {0} and mediatype {1} requested", filetype, mediatype);
            return null;
        }

        public WebFileInfo GetFileInfo(int? provider, WebMediaType mediatype, WebFileType filetype, string id, int offset)
        {
            string path = "";
            try
            {
                path = GetPathList(provider, mediatype, filetype, id).ElementAt(offset);
                WebFileInfo retVal = null;

                try
                {
                    // first try it the usual way
                    retVal = GetLibrary(provider, mediatype).GetFileInfo(path).Finalize(provider, mediatype);
                }
                catch (UnauthorizedAccessException)
                {
                    // access denied, try impersonation
                    if (new Uri(path).IsUnc)
                    {
                        using (NetworkShareImpersonator impersonation = new NetworkShareImpersonator())
                        {
                            retVal = new WebFileInfo(path);
                            retVal.IsLocalFile = Configuration.Services.NetworkImpersonation.ReadInStreamingService;
                            retVal.OnNetworkDrive = true;
                        }
                    }
                }

                // Make sure to always the path property, even if the file doesn't exist. This makes debugging a lot easier, as you get actual paths in your logs now. 
                retVal.Path = path;
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
                Path = String.IsNullOrWhiteSpace(path) ? null : path
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

                // first try to read the file
                if (info.IsLocalFile && File.Exists(path))
                {
                    return new FileStream(path, FileMode.Open, FileAccess.Read);
                }

                // maybe the plugin has some magic
                if (!info.IsLocalFile && info.Exists && !info.OnNetworkDrive)
                {
                    return GetLibrary(provider, mediatype).GetFile(path);
                }

                // try to load it from a network drive
                if (info.OnNetworkDrive && info.Exists)
                {
                    using (NetworkShareImpersonator impersonation = new NetworkShareImpersonator())
                    {
                        return new FileStream(path, FileMode.Open, FileAccess.Read);
                    }
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
        public IList<Interfaces.Playlist.WebPlaylist> GetPlaylists(int? provider)
        {
            return PlaylistLibraries[provider].GetPlaylists().Finalize(provider, ProviderType.Music);
        }

        public IList<Interfaces.Playlist.WebPlaylistItem> GetAllPlaylistItems(int? provider, string playlistId, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return PlaylistLibraries[provider].GetPlaylistItems(playlistId).Finalize(provider, ProviderType.Music);
        }

        public IList<WebPlaylistItem> GetPlaylistItemsByRange(int? provider, string playlistId, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return GetAllPlaylistItems(provider, playlistId).AsQueryable().TakeRange(start, end).Finalize(provider, ProviderType.Music);
        }

        public WebIntResult GetPlaylistItemsCount(int? provider, string playlistId)
        {
            return PlaylistLibraries[provider].GetPlaylistItems(playlistId).AsQueryable().Count();
        }

        private IEnumerable<WebPlaylistItem> GetAllPlaylistItems(int? provider, string playlistId)
        {
            return PlaylistLibraries[provider].GetPlaylistItems(playlistId);
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
                if (position >= 0 && position < playlist.Count)
                {
                    playlist.Insert((int)position, new WebPlaylistItem(track));
                }
                else
                {
                    Log.Warn("Index out of bound for removing playlist item: " + position);
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
        /// Get all available values for a given filter
        /// </summary>
        /// <param name="provider">Provider that is filtered</param>
        /// <param name="mediaType">MediaType that is used</param>
        /// <param name="filterField">Field that is used for filtering</param>
        /// <returns>List of available filter elements</returns>
        public List<WebStringResult> GetFilterElements(int provider, WebMediaType mediaType, string filterField)
        {
            //TODO: return all elements that can be used as a filtered parameter 
            return null;
        }

        /// <summary>
        /// Create a filter from a given set of parameters. This String can then be used as a "filter" parameter
        /// in other MpExtended APIs.
        /// 
        /// To define multiple filters, call this method multiple times and join them in the following format:
        /// [filter1, filter2, filterN]
        /// </summary>
        /// <param name="field">Name of the field that is going to get used for filtering</param>
        /// <param name="op">Operator of this filter</param>
        /// <param name="value">Value of the filter</param>
        /// <param name="conjunction">Conjunction (and, or) to the next filter</param>
        /// <returns>Filter object</returns>
        public WebFilter CreateFilter(string field, string op, string value, string conjunction)
        {
            return new WebFilter() { Field = field, Operator = op, Value = value, Conjunction = conjunction };
        }

        /// <summary>
        /// Create a filter string from a given set of parameters. This String can then be used as a "filter" parameter
        /// in other MpExtended APIs.
        /// 
        /// To define multiple filters, call this method multiple times and join them in the following format:
        /// [filter1, filter2, filterN]
        /// </summary>
        /// <param name="field">Name of the field that is going to get used for filtering</param>
        /// <param name="op">Operator of this filter</param>
        /// <param name="value">Value of the filter</param>
        /// <param name="conjunction">Conjunction (and, or) to the next filter</param>
        /// <returns>Filter as string</returns>
        public WebStringResult CreateFilterString(string field, string op, string value, string conjunction)
        {
            return CreateFilter(field, op, value, conjunction).ToJSON();
        }

        /// <summary>
        /// Get a list of all available filter operators (==, !=, ...)
        /// </summary>
        /// <returns>Available operators</returns>
        public List<WebFilterOperator> GetFilterOperators()
        {
            return operators;
        }

        #endregion

    }
}