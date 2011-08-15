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

        [Import]
        public IMovieLibrary MovieLibrary { get; set; }


        [ImportMany]
        public List<IMovieLibrary> MovieLibraries { get; set; }
        [ImportMany]
        public List<ITVShowLibrary> TVShowLibraries { get; set; }
        [ImportMany]
        public List<IPictureLibrary> PictureLibraries { get; set; }
        [ImportMany]
        public List<IMusicLibrary> MusicLibraries { get; set; }

        public MediaAccessService()
        {

            AggregateCatalog agrCatalog = new AggregateCatalog(new DirectoryCatalog(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"MPExtended\Extensions\"));
            CompositionContainer objContainer = new CompositionContainer(agrCatalog);
            TypeCatalog moduleCatalog1 = new TypeCatalog(typeof(IMovieLibrary));
            //TypeCatalog moduleCatalog2 = new TypeCatalog(typeof(IMusicLibrary));
            agrCatalog.Catalogs.Add(moduleCatalog1);
            //agrCatalog.Catalogs.Add(moduleCatalog2);
            objContainer.ComposeParts(this);
        }



        public WebServiceDescription GetServiceDescription()
        {
            throw new NotImplementedException();
        }

        public IList<WebMovieBasic> GetAllMoviesBasic(IMediaAccessService.SortMoviesBy sort = SortMoviesBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMovieDetailed> GetAllMoviesDetailed(IMediaAccessService.SortMoviesBy sort = SortMoviesBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMovieBasic> GetMoviesBasicByRange(int start, int end, IMediaAccessService.SortMoviesBy sort = SortMoviesBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMovieDetailed> GetMoviesDetailedByRange(int start, int end, IMediaAccessService.SortMoviesBy sort = SortMoviesBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMovieBasic> GetMoviesBasicByGenre(string genre, IMediaAccessService.SortMoviesBy sort = SortMoviesBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMovieDetailed> GetMoviesDetailedByGenre(string genre, IMediaAccessService.SortMoviesBy sort = SortMoviesBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<string> GetAllMovieGenres()
        {
            throw new NotImplementedException();
        }

        public WebMovieDetailed GetMovieDetailedById(string movieId)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicTrackBasic> GetAllMusicTracksBasic(IMediaAccessService.SortMusicBy sort = SortMusicBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicTrackDetailed> GetAllMusicTracksDetailed(IMediaAccessService.SortMusicBy sort = SortMusicBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicTrackBasic> GetTracksBasicByRange(int start, int end, IMediaAccessService.SortMusicBy sort = SortMusicBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicTrackDetailed> GetMusicTracksDetailedByRange(int start, int end, IMediaAccessService.SortMusicBy sort = SortMusicBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicTrackBasic> GetMusicTracksBasicByGenre(string genre, IMediaAccessService.SortMusicBy sort = SortMusicBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicTrackDetailed> GetMusicTracksDetailedByGenre(string genre, IMediaAccessService.SortMusicBy sort = SortMusicBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
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

        public IList<WebMusicAlbumBasic> GetAllMusicAlbumsBasic(IMediaAccessService.SortMusicBy sort = SortMusicBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByRange(int start, int end, IMediaAccessService.SortMusicBy sort = SortMusicBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicArtistBasic> GetAllMusicArtistsBasic(IMediaAccessService.SortMusicBy sort = SortMusicBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicArtistBasic> GetMusicArtistsBasicByRange(int start, int end, IMediaAccessService.SortMusicBy sort = SortMusicBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public WebMusicArtistBasic GetMusicArtistBasicById(string artistId)
        {
            throw new NotImplementedException();
        }

        public IList<WebPictureBasic> GetAllPicturesBasic(IMediaAccessService.SortPicturesBy sort = SortPicturesBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebPictureDetailed> GetAllPicturesDetailed(IMediaAccessService.SortPicturesBy sort = SortPicturesBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
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

        public IList<WebTVShowBasic> GetAllTVShows(IMediaAccessService.SortTVShowsBy sort = SortTVShowsBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public WebTVShowDetailed GetTVShowDetailed(string seriesId)
        {
            throw new NotImplementedException();
        }

        public IList<WebTVSeasonBasic> GetTVSeasons(string seriesId, IMediaAccessService.SortTVShowsBy sort = SortTVShowsBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodes(string seriesId, IMediaAccessService.SortTVShowsBy sort = SortTVShowsBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public IList<WebTVEpisodeBasic> GetTVEpisodesForSeason(string seriesId, string seasonId, IMediaAccessService.SortTVShowsBy sort = SortTVShowsBy.Name, IMediaAccessService.OrderBy order = OrderBy.Asc)
        {
            throw new NotImplementedException();
        }

        public WebTVEpisodeDetailed GetTVEpisodeDetailed(string episodeId)
        {
            throw new NotImplementedException();
        }
    }   
}
