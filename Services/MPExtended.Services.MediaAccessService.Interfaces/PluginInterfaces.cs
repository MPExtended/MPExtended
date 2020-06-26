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
        IEnumerable<WebPictureBasic> GetAllPicturesBasic();
        IEnumerable<WebPictureDetailed> GetAllPicturesDetailed();
        IEnumerable<WebPictureBasic> GetPicturesBasicByCategory(string id);
        IEnumerable<WebPictureDetailed> GetPicturesDetailedByCategory(string id);
        WebPictureBasic GetPictureBasic(string pictureId);
        WebPictureDetailed GetPictureDetailed(string pictureId);
        IEnumerable<WebCategory> GetAllPictureCategories();
        IEnumerable<WebCategory> GetSubCategoriesById(string categoryId);
    }

    public interface IFileSystemLibrary : ILibrary
    {
        IEnumerable<WebDriveBasic> GetDriveListing();
        IEnumerable<WebFileBasic> GetFilesListing(string id);
        IEnumerable<WebFolderBasic> GetFoldersListing(string id);
        WebFileBasic GetFileBasic(string id);
        WebFolderBasic GetFolderBasic(string id);
        WebDriveBasic GetDriveBasic(string id);
    }

    public interface IPluginData
    {
        Dictionary<string, string> GetConfiguration(string pluginname);
    }
}
