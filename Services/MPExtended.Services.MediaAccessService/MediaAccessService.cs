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

        public SerializableDictionary<string> GetExternalMediaInfo(int? provider, WebMediaType type, string id)
        {
            return GetLibrary(provider, type).GetExternalMediaInfo(type, id);
        }
        #endregion

        #region Movies
        public IList<WebCategory> GetAllMovieCategories(int? provider, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MovieLibraries[provider].GetAllCategories().SortMediaItemList(sort, order).Finalize(provider, ProviderType.Movie);
        }

        public WebItemCount GetMovieCount(int? provider, string genre, string category, string actor = null)
        {
            return new WebItemCount() { Count = MovieLibraries[provider].GetAllMovies().CommonFilter(genre, category, actor).Count() };
        }

        public IList<WebMovieBasic> GetAllMoviesBasic(int? provider, string genre = null, string category = null, string actor = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MovieLibraries[provider].GetAllMovies().CommonFilter(genre, category, actor).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Movie);
        }

        public IList<WebMovieDetailed> GetAllMoviesDetailed(int? provider, string genre = null, string category = null, string actor = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MovieLibraries[provider].GetAllMoviesDetailed().CommonFilter(genre, category, actor).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Movie);
        }

        public IList<WebMovieBasic> GetMoviesBasicByRange(int? provider, int start, int end, string genre = null, string category = null, string actor = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MovieLibraries[provider].GetAllMovies().CommonFilter(genre, category, actor).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Movie);
        }

        public IList<WebMovieDetailed> GetMoviesDetailedByRange(int? provider, int start, int end, string genre = null, string category = null, string actor = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MovieLibraries[provider].GetAllMoviesDetailed().CommonFilter(genre, category, actor).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Movie);
        }

        public IList<WebGenre> GetAllMovieGenres(int? provider, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MovieLibraries[provider].GetAllGenres().SortMediaItemList(sort, order).Finalize(provider, ProviderType.Movie).ToList();
        }

        public IList<WebGenre> GetMovieGenresByRange(int? provider, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MovieLibraries[provider].GetAllGenres().SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Movie);
        }

        public WebItemCount GetMovieGenresCount(int? provider)
        {
            return new WebItemCount() { Count = MovieLibraries[provider].GetAllGenres().Count() };
        }

        public WebMovieBasic GetMovieBasicById(int? provider, string id)
        {
            return MovieLibraries[provider].GetMovieBasicById(id).Finalize(provider, ProviderType.Movie);
        }

        public WebMovieDetailed GetMovieDetailedById(int? provider, string id)
        {
            return MovieLibraries[provider].GetMovieDetailedById(id).Finalize(provider, ProviderType.Movie);
        }

        public IList<WebActor> GetAllMovieActors(int? provider, SortBy? sort = SortBy.Name, OrderBy? order = OrderBy.Asc)
        {
            sort = sort.HasValue ? sort.Value : SortBy.Name;
            return MovieLibraries[provider].GetAllMovies().SelectMany(x => x.Actors).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebActor> GetMovieActorsByRange(int? provider, int start, int end, SortBy? sort = SortBy.Name, OrderBy? order = OrderBy.Asc)
        {
            return GetAllMovieActors(provider, sort, order).TakeRange(start, end).ToList();
        }

        public WebItemCount GetMovieActorCount(int? provider)
        {
            return new WebItemCount() { Count = GetAllMovieActors(provider).Count };
        }
        #endregion

        #region Music
        public IList<WebCategory> GetAllMusicCategories(int? provider)
        {
            return MusicLibraries[provider].GetAllCategories().Finalize(provider, ProviderType.Music);
        }

        public WebItemCount GetMusicTrackCount(int? provider, string genre = null)
        {
            return new WebItemCount() { Count = MusicLibraries[provider].GetAllTracks().FilterGenre(genre).Count() };
        }

        public WebItemCount GetMusicAlbumCount(int? provider, string genre = null, string category = null)
        {
            return new WebItemCount() { Count = MusicLibraries[provider].GetAllAlbums().CommonFilter(genre, category).Count() };
        }

        public WebItemCount GetMusicArtistCount(int? provider, string category = null)
        {
            return new WebItemCount() { Count = MusicLibraries[provider].GetAllArtists().FilterCategory(category).Count() };
        }

        public IList<WebMusicTrackBasic> GetAllMusicTracksBasic(int? provider, string genre = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllTracks().FilterGenre(genre).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicTrackDetailed> GetAllMusicTracksDetailed(int? provider, string genre = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllTracksDetailed().FilterGenre(genre).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicTrackBasic> GetMusicTracksBasicByRange(int? provider, int start, int end, string genre = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllTracks().FilterGenre(genre).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicTrackDetailed> GetMusicTracksDetailedByRange(int? provider, int start, int end, string genre = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllTracksDetailed().FilterGenre(genre).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
        }

        public WebMusicTrackBasic GetMusicTrackBasicById(int? provider, string id)
        {
            return MusicLibraries[provider].GetTrackBasicById(id).Finalize(provider, ProviderType.Music);
        }

        public IList<WebGenre> GetAllMusicGenres(int? provider, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllGenres().SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public IList<WebGenre> GetMusicGenresByRange(int? provider, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllGenres().SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
        }

        public WebItemCount GetMusicGenresCount(int? provider)
        {
            return new WebItemCount() { Count = MusicLibraries[provider].GetAllGenres().Count() };
        }

        public WebMusicTrackDetailed GetMusicTrackDetailedById(int? provider, string id)
        {
            return MusicLibraries[provider].GetTrackDetailedById(id).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicAlbumBasic> GetAllMusicAlbumsBasic(int? provider, string genre = null, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllAlbums().CommonFilter(genre, category).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByRange(int? provider, int start, int end, string genre = null, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllAlbums().CommonFilter(genre, category).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicArtistBasic> GetAllMusicArtistsBasic(int? provider, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllArtists().FilterCategory(category).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicArtistBasic> GetMusicArtistsBasicByRange(int? provider, int start, int end, string category = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllArtists().FilterCategory(category).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Music);
        }

        public WebMusicArtistBasic GetMusicArtistBasicById(int? provider, string id)
        {
            return MusicLibraries[provider].GetArtistBasicById(id).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicTrackBasic> GetMusicTracksBasicForAlbum(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllTracks().Where(p => p.AlbumId == id).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicTrackDetailed> GetMusicTracksDetailedForAlbum(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllTracksDetailed().Where(p => p.AlbumId == id).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }

        public WebMusicAlbumBasic GetMusicAlbumBasicById(int? provider, string id)
        {
            return MusicLibraries[provider].GetAlbumBasicById(id).Finalize(provider, ProviderType.Music);
        }

        public IList<WebMusicAlbumBasic> GetMusicAlbumsBasicForArtist(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return MusicLibraries[provider].GetAllAlbums().Where(p => p.AlbumArtistId == id).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Music);
        }
        #endregion

        #region Pictures
        public WebItemCount GetPictureCount(int? provider)
        {
            return new WebItemCount() { Count = PictureLibraries[provider].GetAllPicturesBasic().Count() };
        }

        public IList<WebPictureBasic> GetAllPicturesBasic(int? provider, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return PictureLibraries[provider].GetAllPicturesBasic().SortMediaItemList(sort, order).Finalize(provider, ProviderType.Picture);
        }

        public IList<WebPictureDetailed> GetAllPicturesDetailed(int? provider, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return PictureLibraries[provider].GetAllPicturesDetailed().SortMediaItemList(sort, order).Finalize(provider, ProviderType.Picture);
        }

        public IList<WebCategory> GetAllPictureCategories(int? provider)
        {
            return PictureLibraries[provider].GetAllPictureCategories().Finalize(provider, ProviderType.Picture);
        }

        public IList<WebCategory> GetPictureSubCategories(int? provider, string id)
        {
            return PictureLibraries[provider].GetSubCategoriesById(id).Finalize(provider, ProviderType.Picture);
        }

        public IList<WebPictureBasic> GetPicturesBasicByCategory(int? provider, string id)
        {
            return PictureLibraries[provider].GetPicturesBasicByCategory(id).Finalize(provider, ProviderType.Picture);
        }

        public IList<WebPictureDetailed> GetPicturesDetailedByCategory(int? provider, string id)
        {
            return PictureLibraries[provider].GetPicturesDetailedByCategory(id).Finalize(provider, ProviderType.Picture);
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
            return TVShowLibraries[provider].GetAllCategories().Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebGenre> GetAllTVShowGenres(int? provider, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllGenres().SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebGenre> GetTVShowGenresByRange(int? provider, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllGenres().SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
        }

        public WebItemCount GetTVShowGenresCount(int? provider)
        {
            return new WebItemCount() { Count = TVShowLibraries[provider].GetAllGenres().Count() };
        }

        public IList<WebTVShowBasic> GetAllTVShowsBasic(int? provider, string genre = null, string category = null, string actor = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsBasic().CommonFilter(genre, category, actor).SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVShowDetailed> GetAllTVShowsDetailed(int? provider, string genre = null, string category = null, string actor = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsDetailed().CommonFilter(genre, category, actor).SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVShowBasic> GetTVShowsBasicByRange(int? provider, int start, int end, string genre = null, string category = null, string actor = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsBasic().CommonFilter(genre, category, actor).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVShowDetailed> GetTVShowsDetailedByRange(int? provider, int start, int end, string genre = null, string category = null, string actor = null, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllTVShowsDetailed().CommonFilter(genre, category, actor).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
        }

        public WebTVShowDetailed GetTVShowDetailedById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetTVShowDetailed(id).Finalize(provider, ProviderType.TVShow);
        }

        public WebTVShowBasic GetTVShowBasicById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetTVShowBasic(id).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVSeasonBasic> GetTVSeasonsBasicForTVShow(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllSeasonsBasic().Where(x => x.ShowId == id).SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVSeasonDetailed> GetTVSeasonsDetailedForTVShow(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllSeasonsDetailed().Where(x => x.ShowId == id).SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
        }

        public WebTVSeasonDetailed GetTVSeasonDetailedById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetSeasonDetailed(id).Finalize(provider, ProviderType.TVShow);
        }

        public WebTVSeasonBasic GetTVSeasonBasicById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetSeasonBasic(id).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesBasicByRange(int? provider, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedByRange(int? provider, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesDetailed().SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShow(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().Where(p => p.ShowId == id).SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShow(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesDetailed().Where(p => p.ShowId == id).SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShowByRange(int? provider, string id, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().Where(p => p.ShowId == id).SortMediaItemList(sort, order).TakeRange(start, end - start).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShowByRange(int? provider, string id, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesDetailed().Where(p => p.ShowId == id).SortMediaItemList(sort, order).TakeRange(start, end - start).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesBasicForSeason(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesBasic().Where(p => p.SeasonId == id).SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
        }

        public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForSeason(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return TVShowLibraries[provider].GetAllEpisodesDetailed().Where(p => p.SeasonId == id).SortMediaItemList(sort, order).Finalize(provider, ProviderType.TVShow);
        }

        public WebTVEpisodeBasic GetTVEpisodeBasicById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetEpisodeBasic(id).Finalize(provider, ProviderType.TVShow);
        }

        public WebTVEpisodeDetailed GetTVEpisodeDetailedById(int? provider, string id)
        {
            return TVShowLibraries[provider].GetEpisodeDetailed(id).Finalize(provider, ProviderType.TVShow);
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

        public WebItemCount GetTVShowCount(int? provider, string genre = null, string category = null, string actor = null)
        {
            return new WebItemCount() { Count = TVShowLibraries[provider].GetAllTVShowsBasic().CommonFilter(genre, category, actor).Count() };
        }

        public WebItemCount GetTVSeasonCountForTVShow(int? provider, string id)
        {
            return new WebItemCount() { Count = TVShowLibraries[provider].GetAllSeasonsBasic().Where(x => x.ShowId == id).Count() };
        }

        public IList<WebActor> GetAllTVShowActors(int? provider, SortBy? sort = SortBy.Name, OrderBy? order = OrderBy.Asc)
        {
            sort = sort.HasValue ? sort.Value : SortBy.Name;
            return TVShowLibraries[provider].GetAllTVShowsBasic().SelectMany(x => x.Actors).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebActor> GetTVShowActorsByRange(int? provider, int start, int end, SortBy? sort = SortBy.Name, OrderBy? order = OrderBy.Asc)
        {
            return GetAllTVShowActors(provider, sort, order).TakeRange(start, end).ToList();
        }

        public WebItemCount GetTVShowActorCount(int? provider)
        {
            return new WebItemCount() { Count = GetAllTVShowActors(provider).Count };
        }
        #endregion

        #region Filesystem
        public WebItemCount GetFileSystemDriveCount(int? provider)
        {
            return new WebItemCount() { Count = FileSystemLibraries[provider].GetDriveListing().Count() };
        }

        public IList<WebDriveBasic> GetAllFileSystemDrives(int? provider, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return FileSystemLibraries[provider].GetDriveListing().SortMediaItemList(sort, order).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public IList<WebDriveBasic> GetFileSystemDrivesByRange(int? provider, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return FileSystemLibraries[provider].GetDriveListing().SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public IList<WebFolderBasic> GetAllFileSystemFolders(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return FileSystemLibraries[provider].GetFoldersListing(id).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public IList<WebFolderBasic> GetFileSystemFoldersByRange(int? provider, string id, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return FileSystemLibraries[provider].GetFoldersListing(id).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public WebItemCount GetFileSystemFoldersCount(int? provider, string id)
        {
            var listA = FileSystemLibraries[provider].GetFoldersListing(id);
            int count = listA.Count();
            return new WebItemCount() { Count = count };
        }

        public IList<WebFileBasic> GetAllFileSystemFiles(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return FileSystemLibraries[provider].GetFilesListing(id).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public IList<WebFileBasic> GetFileSystemFilesByRange(int? provider, string id, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            return FileSystemLibraries[provider].GetFilesListing(id).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public WebItemCount GetFileSystemFilesCount(int? provider, string id)
        {
            var listA = FileSystemLibraries[provider].GetFilesListing(id);
            int count = listA.Count();
            return new WebItemCount() { Count = count };
        }

        public IList<WebFilesystemItem> GetAllFileSystemFilesAndFolders(int? provider, string id, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            var listA = FileSystemLibraries[provider].GetFilesListing(id).Select(x => x.ToWebFilesystemItem());
            var listB = FileSystemLibraries[provider].GetFoldersListing(id).Select(x => x.ToWebFilesystemItem());
            return listA.Concat(listB).SortMediaItemList(sort, order).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public IList<WebFilesystemItem> GetFileSystemFilesAndFoldersByRange(int? provider, string id, int start, int end, SortBy? sort = SortBy.Title, OrderBy? order = OrderBy.Asc)
        {
            var listA = FileSystemLibraries[provider].GetFilesListing(id).Select(x => x.ToWebFilesystemItem());
            var listB = FileSystemLibraries[provider].GetFoldersListing(id).Select(x => x.ToWebFilesystemItem());
            return listA.Concat(listB).SortMediaItemList(sort, order).TakeRange(start, end).Finalize(provider, ProviderType.Filesystem).ToList();
        }

        public WebItemCount GetFileSystemFilesAndFoldersCount(int? provider, string id)
        {
            var listA = FileSystemLibraries[provider].GetFilesListing(id).Select(x => x.ToWebFilesystemItem());
            var listB = FileSystemLibraries[provider].GetFoldersListing(id).Select(x => x.ToWebFilesystemItem());
            int count = listA.Count() + listB.Count();
            return new WebItemCount() { Count = count };
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
            else if (mediatype == WebMediaType.MusicAlbum && filetype == WebFileType.Cover)
                return MusicLibraries[provider].GetAlbumBasicById(id).Artwork.Where(x => x.Type == WebFileType.Cover).Select(x => ((WebArtworkDetailed)x).Path).ToList();
            else if (mediatype == WebMediaType.MusicTrack && filetype == WebFileType.Content)
                return MusicLibraries[provider].GetTrackBasicById(id).Path;

            Log.Warn("Invalid combination of filetype {0} and mediatype {1} requested", filetype, mediatype);
            return null;
        }

        public WebFileInfo GetFileInfo(int? provider, WebMediaType mediatype, WebFileType filetype, string id, int offset)
        {
            string path = GetPathList(provider, mediatype, filetype, id).ElementAt(offset);

            try
            {
                try
                {
                    // first try it the usual way
                    return GetLibrary(provider, mediatype).GetFileInfo(path).Finalize(provider, mediatype);
                }
                catch (UnauthorizedAccessException)
                {
                    // access denied, try impersonation
                    if (new Uri(path).IsUnc)
                    {
                        using (NetworkShareImpersonator impersonation = new NetworkShareImpersonator())
                        {
                            var ret = new WebFileInfo(path);
                            ret.IsLocalFile = false;
                            ret.OnNetworkDrive = true;
                            return ret;
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Log.Info("Failed to load fileinfo for non-existing file mediatype={0}, filetype={1}, id={2}, offset={3} (resulting in path={4})", mediatype, filetype, id, offset, path);
            }
            catch (Exception ex)
            {
                Log.Info(String.Format("Failed to load fileinfo for mediatype={0}, filetype={1}, id={2}, offset={3} (resulting in path={4})", mediatype, filetype, id, offset, path), ex);
            }

            return new WebFileInfo()
            {
                Exists = false
            };
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
                string path = GetPathList(provider, mediatype, filetype, id).ElementAt(offset);
                WebFileInfo info = GetFileInfo(provider, mediatype, filetype, id, offset);

                // first try to read the file
                if (info.IsLocalFile)
                {
                    return new FileStream(path, FileMode.Open, FileAccess.Read);
                }

                // maybe the plugin has some magic
                if (info.Exists && !info.OnNetworkDrive)
                {
                    return GetLibrary(provider, mediatype).GetFile(path);
                }

                // try to load it from a network drive
                if(info.OnNetworkDrive && info.Exists)
                {
                    using (NetworkShareImpersonator impersonation = new NetworkShareImpersonator())
                    {
                        return new FileStream(path, FileMode.Open, FileAccess.Read);
                    }
                }

                // fail
                Log.Warn("Requested non-existing or non-accessible file mediatype={0} filetype={1} id={2} offset={3}", mediatype, filetype, id, offset);
                return null;
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