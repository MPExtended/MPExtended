#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using MPExtended.Libraries.General;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.Services.MediaAccessService
{
    // Here we implement all the methods, but we don't do any data retrieval, that
    // is handled by the backend library classes. We only do some filtering and
    // sorting.

    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public sealed class MediaAccessService : ProviderHandler, IMediaAccessService
    {
        #region General
        private const int MOVIE_API = 3;
        private const int MUSIC_API = 3;
        private const int PICTURES_API = 3;
        private const int TVSHOWS_API = 3;
        private const int FILESYSTEM_API = 3;

        private ILibrary GetLibrary(int? provider, WebMediaType type)
        {
            switch (type)
            {
                case WebMediaType.Movie:
                    return MovieLibraries[provider];
                case WebMediaType.MusicTrack:
                case WebMediaType.MusicAlbum:
                    return MusicLibraries[provider];
                case WebMediaType.Picture:
                    return PictureLibraries[provider];
                case WebMediaType.TVShow:
                case WebMediaType.TVSeason:
                case WebMediaType.TVEpisode:
                    return TVShowLibraries[provider];
                case WebMediaType.File:
                    return FileSystemLibraries[provider];
                default:
                    throw new ArgumentException();
            }
        }

        public WebMediaServiceDescription GetServiceDescription()
        {
            return new WebMediaServiceDescription()
            {
                MovieApiVersion = MOVIE_API,
                MusicApiVersion = MUSIC_API,
                PicturesApiVersion = PICTURES_API,
                TvShowsApiVersion = TVSHOWS_API,
                FilesystemApiVersion = FILESYSTEM_API,

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

        public WebMediaItem GetMediaItem(int? provider, WebMediaType type, string id)
        {
            switch (type)
            {
                case WebMediaType.Movie:
                    return GetMovieDetailedById(provider, id).SetProvider(provider, ProviderType.Movie).ToWebMediaItem();
                case WebMediaType.MusicTrack:
                    return GetMusicTrackDetailedById(provider, id).SetProvider(provider, ProviderType.Music).ToWebMediaItem();
                case WebMediaType.Picture:
                    return GetPictureDetailedById(provider, id).SetProvider(provider, ProviderType.Picture).ToWebMediaItem();
                case WebMediaType.TVEpisode:
                    return GetTVEpisodeDetailedById(provider, id).SetProvider(provider, ProviderType.TVShow).ToWebMediaItem();
                case WebMediaType.File:
                    return GetFileSystemFileBasicById(provider, id).SetProvider(provider, ProviderType.Filesystem).ToWebMediaItem();
                default:
                    throw new ArgumentException();
            }
        }

        public IList<WebSearchResult> Search(string text)
        {

            return MovieLibraries.SearchAll(text)
                .Concat(MusicLibraries.SearchAll(text))
                .Concat(PictureLibraries.SearchAll(text))
                .Concat(TVShowLibraries.SearchAll(text))
                .Concat(FileSystemLibraries.SearchAll(text))
                .OrderByDescending(x => x.Score)
                .ToList();
        }

        public IList<WebSearchResult> SearchResultsByRange(string text, int start, int end)
        {
            return Search(text).TakeRange(start, end).ToList();
        }

        public WebExternalMediaInfo GetExternalMediaInfo(int? provider, WebMediaType type, string id)
        {
            return GetLibrary(provider, type).GetExternalMediaInfo(type, id);
        }
        #endregion

        #region Movies
        public IList<WebCategory> GetAllMovieCategories(int? provider)
        {
            return MovieLibraries[provider].GetAllCategories().FillProvider(provider, ProviderType.Movie).ToList();
        }

        public WebItemCount GetMovieCount(int? provider, string genre, string category)
        {
            return new WebItemCount() { Count = MovieLibraries[provider].GetAllMovies().FilterGenreCategory(genre, category).Count() };
        }

        public IList<WebMovieBasic> GetAllMoviesBasic(int? provider, string genre = null, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MovieLibraries[provider].GetAllMovies().SortMediaItemList(sort, order).FillProvider(provider, ProviderType.Movie).ToList();
        }

        public IList<WebMovieDetailed> GetAllMoviesDetailed(int? provider, string genre = null, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MovieLibraries[provider].GetAllMoviesDetailed().FilterGenreCategory(genre, category).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.Movie).ToList();
        }

        public IList<WebMovieBasic> GetMoviesBasicByRange(int? provider, int start, int end, string genre = null, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MovieLibraries[provider].GetAllMovies().FilterGenreCategory(genre, category).SortMediaItemList(sort, order).TakeRange(start, end).FillProvider(provider, ProviderType.Movie).ToList();
        }

        public IList<WebMovieDetailed> GetMoviesDetailedByRange(int? provider, int start, int end, string genre = null, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MovieLibraries[provider].GetAllMoviesDetailed().FilterGenreCategory(genre, category).SortMediaItemList(sort, order).TakeRange(start, end).FillProvider(provider, ProviderType.Movie).ToList();
        }

        public IList<WebGenre> GetAllMovieGenres(int? provider)
        {
            return MovieLibraries[provider].GetAllGenres().FillProvider(provider, ProviderType.Movie).ToList();
        }

        public WebMovieBasic GetMovieBasicById(int? provider, string id)
        {
            return MovieLibraries[provider].GetMovieBasicById(id).SetProvider(provider, ProviderType.Movie);
        }

        public WebMovieDetailed GetMovieDetailedById(int? provider, string id)
        {
            return MovieLibraries[provider].GetMovieDetailedById(id).SetProvider(provider, ProviderType.Movie);
        }
        #endregion

        #region Music
        public IList<WebCategory> GetAllMusicCategories(int? provider)
        {
            return MusicLibraries[provider].GetAllCategories().FillProvider(provider, ProviderType.Music).ToList();
        }

        public WebItemCount GetMusicTrackCount(int? provider, string genre = null)
        {
            return new WebItemCount() { Count = MusicLibraries[provider].GetAllTracks().FilterGenre(genre).Count() };
        }

        public WebItemCount GetMusicAlbumCount(int? provider, string genre = null, string category = null)
        {
            return new WebItemCount() { Count = MusicLibraries[provider].GetAllAlbums().FilterGenreCategory(genre, category).Count() };
        }

        public WebItemCount GetMusicArtistCount(int? provider, string category = null)
        {
            return new WebItemCount() { Count = MusicLibraries[provider].GetAllArtists().FilterCategory(category).Count() };
        }

        public IList<WebMusicTrackBasic> GetAllMusicTracksBasic(int? provider, string genre = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllTracks().FilterGenre(genre).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.Music).ToList();
        }

        public IList<WebMusicTrackDetailed> GetAllMusicTracksDetailed(int? provider, string genre = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllTracksDetailed().FilterGenre(genre).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.Music).ToList();
        }

        public IList<WebMusicTrackBasic> GetMusicTracksBasicByRange(int? provider, int start, int end, string genre = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllTracks().FilterGenre(genre).SortMediaItemList(sort, order).TakeRange(start, end).FillProvider(provider, ProviderType.Music).ToList();
        }

        public IList<WebMusicTrackDetailed> GetMusicTracksDetailedByRange(int? provider, int start, int end, string genre = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllTracksDetailed().FilterGenre(genre).SortMediaItemList(sort, order).TakeRange(start, end).FillProvider(provider, ProviderType.Music).ToList();
        }

        public WebMusicTrackBasic GetMusicTrackBasicById(int? provider, string id)
        {
            return MusicLibraries[provider].GetAllTracks().Where(x => x.Id == id).FillProvider(provider, ProviderType.Music).First();
        }

        public IList<WebGenre> GetAllMusicGenres(int? provider)
        {
            return MusicLibraries[provider].GetAllGenres().FillProvider(provider, ProviderType.Music).ToList();
        }

        public WebMusicTrackDetailed GetMusicTrackDetailedById(int? provider, string id)
        {
            return MusicLibraries[provider].GetAllTracksDetailed().Where(p => p.Id == id).FillProvider(provider, ProviderType.Music).First();
        }

        public IList<WebMusicAlbumBasic> GetAllMusicAlbumsBasic(int? provider, string genre = null, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllAlbums().FilterGenreCategory(genre, category).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.Music).ToList();
        }

        public IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByRange(int? provider, int start, int end, string genre = null, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllAlbums().FilterGenreCategory(genre, category).SortMediaItemList(sort, order).TakeRange(start, end).FillProvider(provider, ProviderType.Music).ToList();
        }

        public IList<WebMusicArtistBasic> GetAllMusicArtistsBasic(int? provider, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllArtists().FilterCategory(category).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.Music).ToList();
        }

        public IList<WebMusicArtistBasic> GetMusicArtistsBasicByRange(int? provider, int start, int end, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllArtists().FilterCategory(category).SortMediaItemList(sort, order).TakeRange(start, end).FillProvider(provider, ProviderType.Music).ToList();
        }

        public WebMusicArtistBasic GetMusicArtistBasicById(int? provider, string id)
        {
            return MusicLibraries[provider].GetAllArtists().Where(p => p.Id == id).FillProvider(provider, ProviderType.Music).First();
        }

        public IList<WebMusicTrackBasic> GetMusicTracksBasicForAlbum(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllTracks().Where(p => p.AlbumId == id).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.Music).ToList();
        }

        public IList<WebMusicTrackDetailed> GetMusicTracksDetailedForAlbum(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllTracksDetailed().Where(p => p.AlbumId == id).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.Music).ToList();
        }

        public WebMusicAlbumBasic GetMusicAlbumBasicById(int? provider, string id)
        {
            return MusicLibraries[provider].GetAllAlbums().Where(p => p.Id == id).FillProvider(provider, ProviderType.Music).First();
        }

        public IList<WebMusicAlbumBasic> GetMusicAlbumsBasicForArtist(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllAlbums().Where(p => p.AlbumArtistId == id).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.Music).ToList();
        }
        #endregion

        #region Pictures
        public WebItemCount GetPictureCount(int? provider)
        {
            return new WebItemCount() { Count = PictureLibraries[provider].GetAllPicturesBasic().Count() };
        }

        public IList<WebPictureBasic> GetAllPicturesBasic(int? provider, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return PictureLibraries[provider].GetAllPicturesBasic().SortMediaItemList(sort, order).FillProvider(provider, ProviderType.Picture).ToList();
        }

        public IList<WebPictureDetailed> GetAllPicturesDetailed(int? provider, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return PictureLibraries[provider].GetAllPicturesDetailed().SortMediaItemList(sort, order).FillProvider(provider, ProviderType.Picture).ToList();
        }

        public IList<WebCategory> GetAllPictureCategories(int? provider)
        {
            return PictureLibraries[provider].GetAllPictureCategories().FillProvider(provider, ProviderType.Picture).ToList();
        }

        public IList<WebCategory> GetPictureSubCategories(int? provider, string id)
        {
            return PictureLibraries[provider].GetSubCategoriesById(id).FillProvider(provider, ProviderType.Picture).ToList();
        }

        public IList<WebPictureBasic> GetPicturesBasicByCategory(int? provider, string id)
        {
            return PictureLibraries[provider].GetPicturesBasicByCategory(id).FillProvider(provider, ProviderType.Picture).ToList();
        }

        public IList<WebPictureDetailed> GetPicturesDetailedByCategory(int? provider, string id)
        {
            return PictureLibraries[provider].GetPicturesDetailedByCategory(id).FillProvider(provider, ProviderType.Picture).ToList();
        }

        public WebPictureBasic GetPictureBasicById(int? provider, string id)
        {
            return PictureLibraries[provider].GetPictureBasic(id).SetProvider(provider, ProviderType.Picture);
        }

        public WebPictureDetailed GetPictureDetailedById(int? provider, string id)
        {
            return PictureLibraries[provider].GetPictureDetailed(id).SetProvider(provider, ProviderType.Picture);
        }
        #endregion

        #region TVShows
        public IList<WebCategory> GetAllTVShowCategories(int? provider)
        {
            return TVShowLibraries[provider].GetAllCategories().FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebGenre> GetAllTVShowGenres(int? provider)
        {
            return TVShowLibraries[provider].GetAllGenres().FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVShowBasic> GetAllTVShowsBasic(int? provider, string genre = null, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsBasic().FilterGenreCategory(genre, category).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVShowDetailed> GetAllTVShowsDetailed(int? provider, string genre = null, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsDetailed().FilterGenreCategory(genre, category).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVShowBasic> GetTVShowsBasicByCategory(int? provider, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsBasic().FilterCategory(category).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVShowDetailed> GetTVShowsDetailedByCategory(int? provider, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsDetailed().FilterCategory(category).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVShowBasic> GetTVShowsBasicByGenre(int? provider, string genre = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsBasic().FilterGenre(genre).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVShowDetailed> GetTVShowsDetailedByGenre(int? provider, string genre = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsDetailed().FilterGenre(genre).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVShowBasic> GetTVShowsBasicByRange(int? provider, int start, int end, string genre = null, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsBasic().FilterGenreCategory(genre, category).SortMediaItemList(sort, order).TakeRange(start, end).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVShowDetailed> GetTVShowsDetailedByRange(int? provider, int start, int end, string genre = null, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsDetailed().FilterGenreCategory(genre, category).SortMediaItemList(sort, order).TakeRange(start, end).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public WebTVShowDetailed GetTVShowDetailedById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetTVShowDetailed(id).SetProvider(provider, ProviderType.TVShow);
        }

        public WebTVShowBasic GetTVShowBasicById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetAllTVShowsBasic().Where(x => x.Id == id).FillProvider(provider, ProviderType.TVShow).First();
        }

        public IList<WebTVSeasonBasic> GetTVSeasonsBasicForTVShow(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllSeasonsBasic().Where(x => x.ShowId == id).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVSeasonDetailed> GetTVSeasonsDetailedForTVShow(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllSeasonsDetailed().Where(x => x.ShowId == id).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public WebTVSeasonDetailed GetTVSeasonDetailedById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetSeasonDetailed(id).SetProvider(provider, ProviderType.TVShow);
        }

        public WebTVSeasonBasic GetTVSeasonBasicById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetSeasonBasic(id).SetProvider(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesBasicByRange(int? provider, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().SortMediaItemList(sort, order).TakeRange(start, end).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedByRange(int? provider, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesDetailed().SortMediaItemList(sort, order).TakeRange(start, end).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShow(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().Where(p => p.ShowId == id).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShow(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesDetailed().Where(p => p.ShowId == id).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShowByRange(int? provider, string id, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().Where(p => p.ShowId == id).SortMediaItemList(sort, order).TakeRange(start, end - start).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShowByRange(int? provider, string id, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesDetailed().Where(p => p.ShowId == id).SortMediaItemList(sort, order).TakeRange(start, end - start).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesBasicForSeason(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().Where(p => p.SeasonId == id).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForSeason(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesDetailed().Where(p => p.SeasonId == id).SortMediaItemList(sort, order).FillProvider(provider, ProviderType.TVShow).ToList();
        }

        public WebTVEpisodeBasic GetTVEpisodeBasicById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetEpisodeBasic(id).SetProvider(provider, ProviderType.TVShow);
        }

        public WebTVEpisodeDetailed GetTVEpisodeDetailedById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetEpisodeDetailed(id).SetProvider(provider, ProviderType.TVShow);
        }

        public WebItemCount GetTVEpisodeCount(int? provider)
        {
            return new WebItemCount() { Count = TVShowLibraries[provider].GetAllEpisodesBasic().Count() };
        }

        public WebItemCount GetTVEpisodeCountForTVShow(int? provider, string id)
        {
            return new WebItemCount() { Count = TVShowLibraries[provider].GetAllEpisodesBasic().Where(e => e.ShowId == id).Count() };
        }

        public WebItemCount GetTVEpisodeCountForSeason(int? provider, string id)
        {
            return new WebItemCount() { Count = TVShowLibraries[provider].GetAllEpisodesBasic().Where(e => e.SeasonId == id).Count() };
        }

        public WebItemCount GetTVShowCount(int? provider, string genre = null, string category = null)
        {
            return new WebItemCount() { Count = TVShowLibraries[provider].GetAllTVShowsBasic().FilterGenreCategory(genre, category).Count() };
        }

        public WebItemCount GetTVSeasonCountForTVShow(int? provider, string id)
        {
            return new WebItemCount() { Count = TVShowLibraries[provider].GetAllSeasonsBasic().Where(x => x.ShowId == id).Count() };
        }
        #endregion

        #region Filesystem
        public IList<WebDriveBasic> GetFileSystemDrives(int? provider)
        {
            return FileSystemLibraries[provider].GetLocalDrives().FillProvider(provider, ProviderType.Filesystem).ToList();
        }

        public IList<WebFolderBasic> GetFileSystemFoldersListing(int? provider, string id)
        {
            return FileSystemLibraries[provider].GetFoldersListing(id).FillProvider(provider, ProviderType.Filesystem).ToList();
        }

        public IList<WebFileBasic> GetFileSystemFilesListing(int? provider, string id)
        {
            return FileSystemLibraries[provider].GetFilesListing(id).FillProvider(provider, ProviderType.Filesystem).ToList();
        }

        public WebFileBasic GetFileSystemFileBasicById(int? provider, string id)
        {
            return FileSystemLibraries[provider].GetFileBasic(id).SetProvider(provider, ProviderType.Filesystem);
        }
        #endregion

        #region Files
        public IList<string> GetPathList(int? provider, WebMediaType mediatype, WebFileType filetype, string id)
        {
            if (mediatype == WebMediaType.File && filetype == WebFileType.Content)
                return GetFileSystemFileBasicById(provider, id).Path;
            else if (mediatype == WebMediaType.Movie && filetype == WebFileType.Content)
                return GetMovieDetailedById(provider, id).Path;
            else if (mediatype == WebMediaType.Movie && filetype == WebFileType.Backdrop)
                return GetMovieDetailedById(provider, id).BackdropPaths;
            else if (mediatype == WebMediaType.Movie && filetype == WebFileType.Cover)
                return GetMovieDetailedById(provider, id).CoverPaths;
            else if (mediatype == WebMediaType.TVShow && filetype == WebFileType.Banner)
                return GetTVShowDetailedById(provider, id).BannerPaths;
            else if (mediatype == WebMediaType.TVShow && filetype == WebFileType.Backdrop)
                return GetTVShowDetailedById(provider, id).BackdropPaths;
            else if (mediatype == WebMediaType.TVShow && filetype == WebFileType.Poster)
                return GetTVShowDetailedById(provider, id).PosterPaths;
            else if (mediatype == WebMediaType.TVSeason && filetype == WebFileType.Backdrop)
                return GetTVSeasonDetailedById(provider, id).BackdropPaths;
            else if (mediatype == WebMediaType.TVSeason && filetype == WebFileType.Banner)
                return GetTVSeasonDetailedById(provider, id).BannerPaths;
            else if (mediatype == WebMediaType.TVSeason && filetype == WebFileType.Poster)
                return GetTVSeasonDetailedById(provider, id).PosterPaths;
            else if (mediatype == WebMediaType.TVEpisode && filetype == WebFileType.Content)
                return GetTVEpisodeBasicById(provider, id).Path;
            else if (mediatype == WebMediaType.TVEpisode && filetype == WebFileType.Banner)
                return GetTVEpisodeBasicById(provider, id).BannerPaths;
            else if (mediatype == WebMediaType.Picture && filetype == WebFileType.Content)
                return GetPictureBasicById(provider, id).Path;
            else if (mediatype == WebMediaType.MusicAlbum && filetype == WebFileType.Cover)
                return GetMusicAlbumBasicById(provider, id).CoverPaths;
            else if (mediatype == WebMediaType.MusicTrack && filetype == WebFileType.Content)
                return GetMusicTrackBasicById(provider, id).Path;

            Log.Warn("Invalid combination of filetype {0} and mediatype {1} requested", filetype, mediatype);
            return null;
        }

        public WebFileInfo GetFileInfo(int? provider, WebMediaType mediatype, WebFileType filetype, string id, int offset)
        {
            try
            {
                return GetLibrary(provider, mediatype).GetFileInfo(GetPathList(provider, mediatype, filetype, id).ElementAt(offset)).SetProvider(provider, mediatype);
            }
            catch (Exception ex)
            {
                Log.Info("Failed to get file info for mediatype=" + mediatype + ", filetype=" + filetype + ", id=" + id + " and offset=" + offset, ex);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return new WebFileInfo();
            }
        }

        public bool IsLocalFile(int? provider, WebMediaType mediatype, WebFileType filetype, string id, int offset)
        {
            WebFileInfo info = GetFileInfo(provider, mediatype, filetype, id, offset);
            return info.Exists && info.IsLocalFile;
        }

        public Stream RetrieveFile(int? provider, WebMediaType mediatype, WebFileType filetype, string id, int offset)
        {
            try
            {
                WebFileInfo info = GetFileInfo(provider, mediatype, filetype, id, offset);
                if (!info.Exists)
                {
                    Log.Warn("Requested non-existing file mediatype={0} filetype={1} id={2} offset={3}", mediatype, filetype, id, offset);
                    return null;
                }

                return GetLibrary(provider, mediatype).GetFile(GetPathList(provider, mediatype, filetype, id).ElementAt(offset));
            }
            catch (Exception ex)
            {
                Log.Info("Failed to retrieve file for mediatype=" + mediatype + ", filetype=" + filetype + ", id=" + id + " and offset=" + offset, ex);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return null;
            }
        }
        #endregion
    }
}