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
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Code;
using MPExtended.Services.MediaAccessService.Code.Helper;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.Services.MediaAccessService
{

    // each method described by IMediaAccessService has to be implemented here, but instead of doing it itself it just references to the MediaInterfaces
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class MediaAccessService : IMediaAccessService
    {
        [ImportMany]
        private Lazy<IMovieLibrary, IDictionary<string, object>>[] MovieLibraries { get; set; }
        [ImportMany]
        private Lazy<ITVShowLibrary, IDictionary<string, object>>[] TVShowLibraries { get; set; }
        [ImportMany]
        private Lazy<IPictureLibrary, IDictionary<string, object>>[] PictureLibraries { get; set; }
        [ImportMany]
        private Lazy<IMusicLibrary, IDictionary<string, object>>[] MusicLibraries { get; set; }

        private IMovieLibrary ChosenMovieLibrary { get; set; }
        private ITVShowLibrary ChosenTVShowLibrary { get; set; }
        private IPictureLibrary ChosenPictureLibrary { get; set; }
        private IMusicLibrary ChosenMusicLibrary { get; set; }

        public MediaAccessService()
        {
            Compose();
            ChosenMovieLibrary = MovieLibraries.FirstOrDefault().Value;
            ChosenMusicLibrary = MusicLibraries.FirstOrDefault().Value;
            ChosenPictureLibrary = PictureLibraries.FirstOrDefault().Value;
            ChosenTVShowLibrary = TVShowLibraries.FirstOrDefault().Value;
        }
        private void Compose()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"MPExtended\Extensions\"));

            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }


        public WebServiceDescription GetServiceDescription()
        {
            var description = new WebServiceDescription();
            description.AvailableMovieProvider = MovieLibraries.Select(p => (string)p.Metadata["Database"]).ToList();
            description.AvailableMusicProvider = MusicLibraries.Select(p => (string)p.Metadata["Database"]).ToList();
            description.AvailablePictureProvider = PictureLibraries.Select(p => (string)p.Metadata["Database"]).ToList();
            description.AvailableTvShowProvider = TVShowLibraries.Select(p => (string)p.Metadata["Database"]).ToList();

            return description;
        }

        #region Movies
        public int GetMovieCount()
        {
            return ChosenMovieLibrary.GetAllMovies().Count;
        }

        public IList<WebMovieBasic> GetAllMoviesBasic(SortMoviesBy sort = SortMoviesBy.Name, OrderBy order = OrderBy.Asc)
        {
            return SortMoviesBasic(ChosenMovieLibrary.GetAllMovies(), sort, order);
        }

        public IList<WebMovieDetailed> GetAllMoviesDetailed(SortMoviesBy sort = SortMoviesBy.Name, OrderBy order = OrderBy.Asc)
        {
            return SortMoviesDetailed(ChosenMovieLibrary.GetAllMoviesDetailed(), sort, order);
        }

        public IList<WebMovieBasic> GetMoviesBasicByRange(int start, int end, SortMoviesBy sort = SortMoviesBy.Name, OrderBy order = OrderBy.Asc)
        {
            return SortMoviesBasic(ChosenMovieLibrary.GetAllMovies(), sort, order).Skip(start).Take(end - start).ToList();
        }

        public IList<WebMovieDetailed> GetMoviesDetailedByRange(int start, int end, SortMoviesBy sort = SortMoviesBy.Name, OrderBy order = OrderBy.Asc)
        {
            return SortMoviesDetailed(ChosenMovieLibrary.GetAllMoviesDetailed(), sort, order).Skip(start).Take(end - start).ToList();
        }

        public IList<WebMovieBasic> GetMoviesBasicByGenre(string genre, SortMoviesBy sort = SortMoviesBy.Name, OrderBy order = OrderBy.Asc)
        {
            return SortMoviesBasic(ChosenMovieLibrary.GetAllMovies().Where(p => p.Genre == genre), sort, order);
        }

        public IList<WebMovieDetailed> GetMoviesDetailedByGenre(string genre, SortMoviesBy sort = SortMoviesBy.Name, OrderBy order = OrderBy.Asc)
        {
            return SortMoviesDetailed(ChosenMovieLibrary.GetAllMoviesDetailed().Where(p => p.Genre == genre), sort, order);
        }

        public IList<string> GetAllMovieGenres()
        {
            return ChosenMovieLibrary.GetAllGenres();
        }

        public WebMovieDetailed GetMovieDetailedById(string movieId)
        {
            return ChosenMovieLibrary.GetMovieDetailedById(movieId);
        }

        #endregion

        public int GetMusicTrackCount()
        {
            throw new NotImplementedException();
        }

        public int GetMusicAlbumCount()
        {
            throw new NotImplementedException();
        }

        public int GetMusicArtistCount()
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicTrackBasic> GetAllMusicTracksBasic(SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicTrackDetailed> GetAllMusicTracksDetailed(SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicTrackBasic> GetTracksBasicByRange(int start, int end, SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicTrackDetailed> GetMusicTracksDetailedByRange(int start, int end, SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicTrackBasic> GetMusicTracksBasicByGenre(string genre, SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicTrackDetailed> GetMusicTracksDetailedByGenre(string genre, SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<string> GetAllMusicGenres()
        {
            throw new NotImplementedException();
        }

        public WebMusicTrackDetailed GetMusicTracksDetailedById(string trackId)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicAlbumBasic> GetAllMusicAlbumsBasic(SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByRange(int start, int end, SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicArtistBasic> GetAllMusicArtistsBasic(SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicArtistBasic> GetMusicArtistsBasicByRange(int start, int end, SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public WebMusicArtistBasic GetMusicArtistBasicById(string artistId)
        {
            throw new NotImplementedException();
        }

        public IList<WebPictureBasic> GetAllPicturesBasic(SortPicturesBy sort = SortPicturesBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebPictureDetailed> GetAllPicturesDetailed(SortPicturesBy sort = SortPicturesBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public WebPictureDetailed GetPictureDetailed(string pictureId)
        {
            throw new NotImplementedException();
        }

        public IList<WebPictureCategoryBasic> GetAllPictureCategoriesBasic()
        {
            throw new NotImplementedException();
        }

        public IList<WebTVShowBasic> GetAllTVShows(SortTVShowsBy sort = SortTVShowsBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public WebTVShowDetailed GetTVShowDetailed(string seriesId)
        {
            throw new NotImplementedException();
        }

        public IList<WebTVSeasonBasic> GetTVSeasons(string seriesId, SortTVShowsBy sort = SortTVShowsBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodes(string seriesId, SortTVShowsBy sort = SortTVShowsBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesForSeason(string seriesId, string seasonId, SortTVShowsBy sort = SortTVShowsBy.Name, OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public WebTVEpisodeDetailed GetTVEpisodeDetailed(string episodeId)
        {
            throw new NotImplementedException();
        }


        private IList<WebMovieBasic> SortMoviesBasic(IEnumerable<WebMovieBasic> list, SortMoviesBy sort, OrderBy order)
        {

            return list.ToList();
        }

        private IList<WebMovieDetailed> SortMoviesDetailed(IEnumerable<WebMovieDetailed> list, SortMoviesBy sort, OrderBy order)
        {

            return list.ToList();
        }

    }
}
