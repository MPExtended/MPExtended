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
using System.Linq.Expressions;
using System.Text;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;
using MPExtended.Services.MediaAccessService.Interfaces.Playlist;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    // This are the interfaces used for internal communication between
    // the service and the backends which provide the data.

    public interface ILibrary
    {
        bool Supported { get; }
        WebFileInfo GetFileInfo(string path);
        Stream GetFile(string path);
        IEnumerable<WebSearchResult> Search(string text);
        WebDictionary<string> GetExternalMediaInfo(WebMediaType type, string id);
    }

    public interface IPlaylistLibrary : ILibrary
    {
        IEnumerable<WebPlaylist> GetPlaylists();
        IEnumerable<WebPlaylistItem> GetPlaylistItems(String playlistId);
        bool SavePlaylist(String playlistId, IEnumerable<WebPlaylistItem> playlistItems);
        String CreatePlaylist(String playlistName);
        bool DeletePlaylist(String playlistId);
    }

    public interface IMusicLibrary : ILibrary
    {
        IEnumerable<WebMusicTrackBasic> GetAllTracks();
        IEnumerable<WebMusicTrackDetailed> GetAllTracksDetailed();
        IEnumerable<WebMusicAlbumBasic> GetAllAlbums();
        IEnumerable<WebMusicArtistBasic> GetAllArtists();
        IEnumerable<WebMusicArtistDetailed> GetAllArtistsDetailed();
        WebMusicTrackBasic GetTrackBasicById(string trackId);
        WebMusicTrackDetailed GetTrackDetailedById(string trackId);
        WebMusicAlbumBasic GetAlbumBasicById(string albumId);
        WebMusicArtistBasic GetArtistBasicById(string artistId);
        WebMusicArtistDetailed GetArtistDetailedById(string artistId);
        IEnumerable<WebGenre> GetAllGenres();
        IEnumerable<WebCategory> GetAllCategories();
    }

    public interface IMovieLibrary : ILibrary
    {
        IEnumerable<WebMovieBasic> GetAllMovies();
        IEnumerable<WebMovieDetailed> GetAllMoviesDetailed();
        WebMovieBasic GetMovieBasicById(string movieId);
        WebMovieDetailed GetMovieDetailedById(string movieId);
        IEnumerable<WebGenre> GetAllGenres();
        IEnumerable<WebCategory> GetAllCategories();
        WebCollection GetCollectionById(string title);
        IEnumerable<WebCollection> GetAllCollections();
        WebBoolResult SetMovieStoptime(string id, int stopTime, Boolean isWatched, int watchedPercent);
        WebBoolResult SetWathcedStatus(string id, Boolean isWatched);
    }

    public interface ITVShowLibrary : ILibrary
    {
        IEnumerable<WebTVShowBasic> GetAllTVShowsBasic();
        IEnumerable<WebTVShowDetailed> GetAllTVShowsDetailed();
        WebTVShowBasic GetTVShowBasic(string seriesId);
        WebTVShowDetailed GetTVShowDetailed(string seriesId);

        IEnumerable<WebTVSeasonBasic> GetAllSeasonsBasic();
        IEnumerable<WebTVSeasonDetailed> GetAllSeasonsDetailed();
        WebTVSeasonBasic GetSeasonBasic(string seasonId);
        WebTVSeasonDetailed GetSeasonDetailed(string seasonId);

        IEnumerable<WebTVEpisodeBasic> GetAllEpisodesBasic();
        IEnumerable<WebTVEpisodeDetailed> GetAllEpisodesDetailed();
        WebTVEpisodeBasic GetEpisodeBasic(string episodeId);
        WebTVEpisodeDetailed GetEpisodeDetailed(string episodeId);

        IEnumerable<WebGenre> GetAllGenres();
        IEnumerable<WebCategory> GetAllCategories();
    }

    public interface IPictureLibrary : ILibrary
    {
        IEnumerable<WebCategory> GetAllPictureCategories();
        IEnumerable<WebPictureBasic> GetAllPicturesBasic();
        IEnumerable<WebPictureDetailed> GetAllPicturesDetailed();
        IEnumerable<WebMobileVideoBasic> GetAllMobileVideosBasic();
        IEnumerable<WebPictureFolder> GetAllPictureFolders();
        IEnumerable<WebPictureBasic> GetPicturesBasicByCategory(string id);
        IEnumerable<WebPictureDetailed> GetPicturesDetailedByCategory(string id);
        WebPictureBasic GetPictureBasic(string pictureId);
        WebPictureDetailed GetPictureDetailed(string pictureId);
        IEnumerable<WebCategory> GetSubCategoriesById(string categoryId);
        WebPictureFolder GetPictureFolderById(string folderId);
        IEnumerable<WebPictureFolder> GetSubFoldersById(string folderId);
        WebMobileVideoBasic GetMobileVideoBasic(string id);
        IEnumerable<WebMobileVideoBasic> GetMobileVideosBasicByCategory(string categoryId);
    }

    public interface IFileSystemLibrary : ILibrary
    {
        IEnumerable<WebDriveBasic> GetDriveListing();
        IEnumerable<WebFileBasic> GetFilesListing(string id);
        IEnumerable<WebFolderBasic> GetFoldersListing(string id);
        WebFileBasic GetFileBasic(string id);
        WebFolderBasic GetFolderBasic(string id);
        WebDriveBasic GetDriveBasic(string id);
        WebBoolResult DeleteFile(string id);
    }

    public interface IPluginData
    {
        Dictionary<string, string> GetConfiguration(string pluginname);
    }
}
