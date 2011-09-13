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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Xml.Linq;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Code;
using MPExtended.Services.MediaAccessService.Code.Helper;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.Services.MediaAccessService
{
    // Here we implement all the methods, but we don't do any data retrieval, that
    // is handled by the backend library classes. We only do some filtering and
    // sorting.

    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class MediaAccessService : IMediaAccessService
    {
        #region Service
        private const int MOVIE_API = 3;
        private const int MUSIC_API = 3;
        private const int PICTURES_API = 3;
        private const int TVSHOWS_API = 3;
        private const int FILESYSTEM_API = 3;

        [ImportMany]
        private Lazy<IMovieLibrary, IDictionary<string, object>>[] MovieLibraries { get; set; }
        [ImportMany]
        private Lazy<ITVShowLibrary, IDictionary<string, object>>[] TVShowLibraries { get; set; }
        [ImportMany]
        private Lazy<IPictureLibrary, IDictionary<string, object>>[] PictureLibraries { get; set; }
        [ImportMany]
        private Lazy<IMusicLibrary, IDictionary<string, object>>[] MusicLibraries { get; set; }
        [ImportMany]
        private Lazy<IFileSystemLibrary, IDictionary<string, object>>[] FileSystemLibraries { get; set; }

        private IMovieLibrary ChosenMovieLibrary { get; set; }
        private ITVShowLibrary ChosenTVShowLibrary { get; set; }
        private IPictureLibrary ChosenPictureLibrary { get; set; }
        private IMusicLibrary ChosenMusicLibrary { get; set; }
        private IFileSystemLibrary ChosenFileSystemLibrary { get; set; }

        public MediaAccessService()
        {
            if (!Compose())
            {
                return;
            }

            try
            {
                var config = XElement.Load(Configuration.GetPath("MediaAccess.xml")).Element("plugins");
                ChosenMovieLibrary = SelectLibrary<IMovieLibrary>(config, "movie", MovieLibraries);
                ChosenMusicLibrary = SelectLibrary<IMusicLibrary>(config, "music", MusicLibraries);
                ChosenPictureLibrary = SelectLibrary<IPictureLibrary>(config, "picture", PictureLibraries);
                ChosenTVShowLibrary = SelectLibrary<ITVShowLibrary>(config, "tvShow", TVShowLibraries);
                ChosenFileSystemLibrary = SelectLibrary<IFileSystemLibrary>(config, "filesystem", FileSystemLibraries);
            }                        
            catch (Exception ex)
            {
                Log.Error("Failed to create backends", ex);
            }

        }

        private T SelectLibrary<T>(XElement config, string type, Lazy<T, IDictionary<string, object>>[] libraries)
        {
            var configured = config.Elements(type).Where(x => x.Value.Length > 0);

            if (configured.Count() == 0)
                return default(T);

            string configuredName = configured.First().Value;

            var list = libraries.Where(x => (string)x.Metadata["Database"] == configuredName);
            if (list.Count() == 0)
                return default(T);

            return list.First().Value;
        }

        private bool Compose()
        {
            try
            {
                AggregateCatalog catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
#if DEBUG
                string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string pluginRoot = Path.Combine(currentDirectory, "..", "..", "..", "..", "PlugIns");
                foreach (string pdir in Directory.GetDirectories(pluginRoot))
                {
                    string dir = Path.GetFullPath(Path.Combine(pluginRoot, pdir, "bin", "Debug"));
                    catalog.Catalogs.Add(new DirectoryCatalog(dir));
                }
#else
                string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string extensionDirectory = Path.GetFullPath(Path.Combine(currentDirectory, "Extensions"));
                catalog.Catalogs.Add(new DirectoryCatalog(extensionDirectory));
#endif

                CompositionContainer container = new CompositionContainer(catalog);
                container.ComposeExportedValue(new PluginData());
                container.ComposeParts(this);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to create MEF service", ex);
                return false;
            }
        }

        private ILibrary GetLibrary(WebMediaType type)
        {
            switch (type)
            {
                case WebMediaType.Movie:
                    return ChosenMovieLibrary;
                case WebMediaType.Music:
                    return ChosenMusicLibrary;
                case WebMediaType.Picture:
                    return ChosenPictureLibrary;
                case WebMediaType.TVShow:
                    return ChosenTVShowLibrary;
                case WebMediaType.File:
                    return ChosenFileSystemLibrary;
                default:
                    throw new ArgumentException();
            }
        }

        public WebMediaServiceDescription GetServiceDescription()
        {
            return new WebMediaServiceDescription()
            {
                AvailableMovieProvider = MovieLibraries.Select(p => (string)p.Metadata["Database"]).ToList(),
                AvailableMusicProvider = MusicLibraries.Select(p => (string)p.Metadata["Database"]).ToList(),
                AvailablePictureProvider = PictureLibraries.Select(p => (string)p.Metadata["Database"]).ToList(),
                AvailableTvShowProvider = TVShowLibraries.Select(p => (string)p.Metadata["Database"]).ToList(),

                SupportsMovies = ChosenMovieLibrary != null,
                SupportsMusic = ChosenMusicLibrary != null,
                SupportsPictures = ChosenPictureLibrary != null,
                SupportsTvShows = ChosenTVShowLibrary != null,

                MovieApiVersion = MOVIE_API,
                MusicApiVersion = MUSIC_API,
                PicturesApiVersion = PICTURES_API,
                TvShowsApiVersion = TVSHOWS_API,
                FilesystemApiVersion = FILESYSTEM_API,

                ServiceVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion
            };
        }

        public WebMediaItem GetMediaItem(WebMediaType type, string id)
        {
            switch (type)
            {
                case WebMediaType.Movie:
                    return GetMovieDetailedById(id);
                case WebMediaType.Music:
                    return GetMusicTrackDetailedById(id);
                case WebMediaType.Picture:
                    return GetPictureDetailed(id);
                case WebMediaType.TVShow:
                    return GetTVEpisodeDetailed(id);
                case WebMediaType.File:
                    return GetFileSystemFile(id);
                default:
                    throw new ArgumentException();
            }
        }
        #endregion

        #region Movies
        public IList<WebCategory> GetAllMovieCategories()
        {
            return ChosenMovieLibrary.GetAllCategories().ToList();
        }

        public WebItemCount GetMovieCount()
        {
            return new WebItemCount() { Count = ChosenMovieLibrary.GetAllMovies().Count() };
        }

        public IList<WebMovieBasic> GetAllMoviesBasic(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMovieLibrary.GetAllMovies().SortMediaItemList(sort, order).ToList();
        }

        public IList<WebMovieDetailed> GetAllMoviesDetailed(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMovieLibrary.GetAllMoviesDetailed().SortMediaItemList(sort, order).ToList();
        }

        public IList<WebMovieBasic> GetMoviesBasicForCategory(string category, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMovieLibrary.GetAllMovies().Where(x => x.UserDefinedCategories.Contains(category)).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebMovieDetailed> GetMoviesDetailedForCategory(string category, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMovieLibrary.GetAllMoviesDetailed().Where(x => x.UserDefinedCategories.Contains(category)).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebMovieBasic> GetMoviesBasicByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMovieLibrary.GetAllMovies().SortMediaItemList(sort, order).GetRange(start, end).ToList();
        }

        public IList<WebMovieDetailed> GetMoviesDetailedByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMovieLibrary.GetAllMoviesDetailed().SortMediaItemList(sort, order).GetRange(start, end).ToList();
        }

        public IList<WebMovieBasic> GetMoviesBasicByGenre(string genre, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMovieLibrary.GetAllMovies().Where(p => p.Genres.Contains(genre)).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebMovieDetailed> GetMoviesDetailedByGenre(string genre, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMovieLibrary.GetAllMoviesDetailed().Where(p => p.Genres.Contains(genre)).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebGenre> GetAllMovieGenres()
        {
            return ChosenMovieLibrary.GetAllGenres().ToList();
        }

        public WebMovieDetailed GetMovieDetailedById(string movieId)
        {
            return ChosenMovieLibrary.GetMovieDetailedById(movieId);
        }

        public Stream GetMovieCover(string id, int offset)
        {
            return ChosenMovieLibrary.GetCover(id, offset);
        }

        public Stream GetMovieBackdrop(string id, int offset)
        {
            return ChosenMovieLibrary.GetBackdrop(id, offset);
        }
        #endregion

        #region Music
        public IList<WebCategory> GetAllMusicCategories()
        {
            return ChosenMusicLibrary.GetAllCategories().ToList();
        }

        public WebItemCount GetMusicTrackCount()
        {
            return new WebItemCount() { Count = ChosenMusicLibrary.GetAllTracks().Count() };
        }

        public WebItemCount GetMusicAlbumCount()
        {
            return new WebItemCount() { Count = ChosenMusicLibrary.GetAllAlbums().Count() };
        }

        public WebItemCount GetMusicArtistCount()
        {
            return new WebItemCount() { Count = ChosenMusicLibrary.GetAllArtists().Count() };
        }

        public IList<WebMusicTrackBasic> GetAllMusicTracksBasic(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllTracks().SortMediaItemList(sort, order).ToList();
        }

        public IList<WebMusicTrackDetailed> GetAllMusicTracksDetailed(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllTracksDetailed().SortMediaItemList(sort, order).ToList();
        }

        public IList<WebMusicTrackBasic> GetTracksBasicByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllTracks().SortMediaItemList(sort, order).GetRange(start, end).ToList();
        }

        public IList<WebMusicTrackDetailed> GetMusicTracksDetailedByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllTracksDetailed().SortMediaItemList(sort, order).GetRange(start, end).ToList();
        }

        public IList<WebMusicTrackBasic> GetMusicTracksBasicByGenre(string genre, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllTracks().Where(p => p.Genres.Contains(genre)).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebMusicTrackDetailed> GetMusicTracksDetailedByGenre(string genre, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllTracksDetailed().Where(p => p.Genres.Contains(genre)).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebGenre> GetAllMusicGenres()
        {
            return ChosenMusicLibrary.GetAllGenres().ToList();
        }

        public WebMusicTrackDetailed GetMusicTrackDetailedById(string Id)
        {
            return ChosenMusicLibrary.GetAllTracksDetailed().Single(p => p.Id == Id);
        }

        public IList<WebMusicAlbumBasic> GetAllMusicAlbumsBasic(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllAlbums().SortMediaItemList(sort, order).ToList();
        }

        public IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllAlbums().SortMediaItemList(sort, order).GetRange(start, end).ToList();
        }

        public IList<WebMusicArtistBasic> GetAllMusicArtistsBasic(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllArtists().SortMediaItemList(sort, order).ToList();
        }

        public IList<WebMusicArtistBasic> GetMusicArtistsBasicByCategory(string category, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllArtists().Where(x => x.UserDefinedCategories.Contains(category)).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebMusicArtistBasic> GetMusicArtistsBasicByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllArtists().SortMediaItemList(sort, order).GetRange(start, end).ToList();
        }

        public WebMusicArtistBasic GetMusicArtistBasicById(string Id)
        {
            return ChosenMusicLibrary.GetAllArtists().Single(p => p.Id == Id);
        }

        public IList<WebMusicTrackBasic> GetMusicTracksBasicByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllTracks().SortMediaItemList(sort, order).GetRange(start, end).ToList();
        }

        public IList<WebMusicTrackBasic> GetMusicTracksBasicForAlbum(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllTracks().Where(p => p.AlbumId == id).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebMusicTrackDetailed> GetMusicTracksDetailedForAlbum(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllTracksDetailed().Where(p => p.AlbumId == id).SortMediaItemList(sort, order).ToList();
        }

        public WebMusicAlbumBasic GetMusicAlbumBasicById(string id)
        {
            return ChosenMusicLibrary.GetAllAlbums().Single(p => p.Id == id);
        }

        public IList<WebMusicAlbumBasic> GetMusicAlbumsBasicForArtist(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllAlbums().Where(p => p.ArtistId == id).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByCategory(string category, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllAlbums().Where(p => p.UserDefinedCategories.Contains(category)).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByGenre(string genre, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenMusicLibrary.GetAllAlbums().Where(p => p.Genres.Contains(genre)).SortMediaItemList(sort, order).ToList();
        }

        public Stream GetMusicCover(string id, int offset)
        {
            return ChosenMusicLibrary.GetCover(id, offset);
        }

        public Stream GetMusicBackdrop(string id, int offset)
        {
            return ChosenMusicLibrary.GetBackdrop(id, offset);
        }
        #endregion

        #region Pictures
        public IList<WebPictureBasic> GetAllPicturesBasic(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenPictureLibrary.GetAllPicturesBasic().SortMediaItemList(sort, order).ToList();
        }

        public IList<WebPictureDetailed> GetAllPicturesDetailed(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenPictureLibrary.GetAllPicturesDetailed().SortMediaItemList(sort, order).ToList();
        }

        public WebPictureDetailed GetPictureDetailed(string Id)
        {
            return ChosenPictureLibrary.GetPictureDetailed(Id);
        }

        public IList<WebCategory> GetAllPictureCategoriesBasic()
        {
            return ChosenPictureLibrary.GetAllPictureCategoriesBasic().ToList();
        }

        public WebItemCount GetPictureCount()
        {
            return new WebItemCount() { Count = ChosenPictureLibrary.GetAllPicturesBasic().Count() };
        }
        #endregion

        #region TVShows
        public IList<WebCategory> GetAllTVShowCategories()
        {
            return ChosenTVShowLibrary.GetAllCategories().ToList();
        }

        public IList<WebGenre> GetAllTVShowGenres()
        {
            return ChosenTVShowLibrary.GetAllGenres().ToList();
        }

        public IList<WebTVShowBasic> GetAllTVShowsBasic(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllTVShowsBasic().SortMediaItemList(sort, order).ToList();
        }

        public IList<WebTVShowDetailed> GetAllTVShowsDetailed(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllTVShowsDetailed().SortMediaItemList(sort, order).ToList();
        }

        public IList<WebTVShowBasic> GetTVShowsBasicByCategory(string category, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllTVShowsBasic().Where(x => x.UserDefinedCategories.Contains(category)).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebTVShowDetailed> GetTVShowsDetailedByCategory(string category, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllTVShowsDetailed().Where(x => x.UserDefinedCategories.Contains(category)).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebTVShowBasic> GetTVShowsBasicByGenre(string genre, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllTVShowsBasic().Where(x => x.Genres.Contains(genre)).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebTVShowDetailed> GetTVShowsDetailedByGenre(string genre, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllTVShowsDetailed().Where(x => x.Genres.Contains(genre)).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebTVShowBasic> GetTVShowsBasicByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllTVShowsBasic().SortMediaItemList(sort, order).GetRange(start, start - end).ToList();
        }

        public IList<WebTVShowDetailed> GetTVShowsDetailedByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllTVShowsDetailed().SortMediaItemList(sort, order).GetRange(start, start - end).ToList();
        }

        public WebTVShowDetailed GetTVShowDetailed(string id)
        {
            return ChosenTVShowLibrary.GetTVShowDetailed(id);
        }

        public Stream GetTVShowBanner(string id, int offset)
        {
            return ChosenTVShowLibrary.GetBanner(id, offset);
        }

        public Stream GetTVShowPoster(string id, int offset)
        {
            return ChosenTVShowLibrary.GetPoster(id, offset);
        }

        public Stream GetTVShowBackdrop(string id, int offset)
        {
            return ChosenTVShowLibrary.GetBackdrop(id, offset);
        }

        public IList<WebTVSeasonBasic> GetAllTVSeasonsBasic(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllSeasonsBasic(id).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebTVSeasonDetailed> GetAllTVSeasonsDetailed(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllSeasonsDetailed(id).SortMediaItemList(sort, order).ToList();
        }

        public WebTVSeasonDetailed GetTVSeasonDetailed(string showId, string seasonId, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetSeasonDetailed(showId, seasonId);
        }

        public Stream GetTVSeasonBanner(string seriesId, string seasonId, int offset)
        {
            return ChosenTVShowLibrary.GetSeasonBanner(seriesId, seasonId, offset);
        }

        public Stream GetTVSeasonPoster(string seriesId, string seasonId, int offset)
        {
            return ChosenTVShowLibrary.GetSeasonPoster(seriesId, seasonId, offset);
        }

        public Stream GetTVSeasonBackdrop(string seriesId, string seasonId, int offset)
        {
            return ChosenTVShowLibrary.GetSeasonBackdrop(seriesId, seasonId, offset);
        }

        public IList<WebTVEpisodeBasic> GetAllTVEpisodesBasicForTVShow(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllEpisodesBasic().Where(p => p.ShowId == id).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebTVEpisodeDetailed> GetAllTVEpisodesDetailedForTVShow(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllEpisodesDetailed().Where(p => p.ShowId == id).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShowByRange(string id, int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllEpisodesBasic().Where(p => p.ShowId == id).SortMediaItemList(sort, order).GetRange(start, end - start).ToList();
        }

        public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShowByRange(string id, int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllEpisodesDetailed().Where(p => p.ShowId == id).SortMediaItemList(sort, order).GetRange(start, end - start).ToList();
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesBasicForSeason(string showId, string seasonId, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {
            return ChosenTVShowLibrary.GetAllEpisodesBasic().Where(p => p.ShowId == showId && p.SeasonId == seasonId).SortMediaItemList(sort, order).ToList();
        }

        public IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForSeason(string showId, string seasonId, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc)
        {           
            return ChosenTVShowLibrary.GetAllEpisodesDetailed().Where(p => p.ShowId == showId && p.SeasonId == seasonId).SortMediaItemList(sort, order).ToList();
        }

        public WebTVEpisodeDetailed GetTVEpisodeDetailed(string id)
        {
            return ChosenTVShowLibrary.GetEpisodeDetailed(id);
        }

        public WebItemCount GetTVEpisodeCount()
        {
            return new WebItemCount() { Count = ChosenTVShowLibrary.GetAllEpisodesBasic().Count() };
        }

        public WebItemCount GetTVEpisodeCountForTVShow(string id)
        {
            return new WebItemCount() { Count = ChosenTVShowLibrary.GetAllEpisodesBasic().Where(e => e.ShowId == id).Count() };
        }

        public WebItemCount GetTVShowCount()
        {
            return new WebItemCount() { Count = ChosenTVShowLibrary.GetAllTVShowsBasic().Count() };
        }

        public WebItemCount GetTVSeasonCountForTVShow(string id)
        {
            return new WebItemCount() { Count = ChosenTVShowLibrary.GetAllSeasonsBasic(id).Count() };
        }
        #endregion

        #region Filesystem
        public IList<WebDriveBasic> GetFileSystemDrives()
        {
            return ChosenFileSystemLibrary.GetLocalDrives().ToList();
        }

        public IList<WebFolderBasic> GetFileSystemFoldersListing(string id)
        {
            return ChosenFileSystemLibrary.GetFoldersListing(id).ToList();
        }

        public IList<WebFileBasic> GetFileSystemFilesListing(string id)
        {
            return ChosenFileSystemLibrary.GetFilesListing(id).ToList();
        }

        public WebFileBasic GetFileSystemFile(string id)
        {
            return ChosenFileSystemLibrary.GetFileBasic(id);
        }

        public bool IsLocalFile(WebMediaType type, string id, int offset)
        {
            return GetLibrary(type).IsLocalFile(GetMediaItem(type, id).Path[offset]);
        }

        public Stream RetrieveFile(WebMediaType type, string id, int offset) 
        {
            return GetLibrary(type).GetFile(GetMediaItem(type, id).Path[offset]);
        }
        #endregion
    }
}