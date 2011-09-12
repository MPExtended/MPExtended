using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    // This are the interfaces used for internal communication between
    // the service and the backends which provide the data.

    public interface IMusicLibrary
    {
        IEnumerable<WebMusicTrackBasic> GetAllTracks();
        IEnumerable<WebMusicAlbumBasic> GetAllAlbums();
        IEnumerable<WebMusicArtistBasic> GetAllArtists();
        IEnumerable<WebMusicTrackDetailed> GetAllTracksDetailed();
        WebMusicTrackBasic GetTrackBasicById(string trackId);
        WebMusicAlbumBasic GetAlbumBasicById(string albumId);
        WebMusicArtistBasic GetArtistBasicById(string artistId);
        WebMusicTrackDetailed GetTrackDetailedById(string trackId);
        IEnumerable<WebGenre> GetAllGenres();
        IEnumerable<WebCategory> GetAllCategories();

    }

    public interface IMovieLibrary
    {
        IEnumerable<WebMovieBasic> GetAllMovies();
        IEnumerable<WebMovieDetailed> GetAllMoviesDetailed();
        WebMovieBasic GetMovieBasicById(string movieId);
        WebMovieDetailed GetMovieDetailedById(string movieId);
        IEnumerable<WebGenre> GetAllGenres();
        IEnumerable<WebCategory> GetAllCategories();

    }

    public interface ITVShowLibrary
    {
        IEnumerable<WebTVShowBasic> GetAllTVShowsBasic();
        IEnumerable<WebTVShowDetailed> GetAllTVShowsDetailed();
        WebTVShowDetailed GetTVShowDetailed(string seriesId);
        IEnumerable<WebTVSeasonBasic> GetAllSeasonsBasic(string seriesId);
        IEnumerable<WebTVSeasonDetailed> GetAllSeasonsDetailed(string seriesId);
        WebTVSeasonDetailed GetSeasonDetailed(string seriesId, string seasonId);
        IEnumerable<WebTVEpisodeBasic> GetAllEpisodesBasic();
        IEnumerable<WebTVEpisodeDetailed> GetAllEpisodesDetailed();
        WebTVEpisodeDetailed GetEpisodeDetailed(string episodeId);
        IEnumerable<WebGenre> GetAllGenres();
        IEnumerable<WebCategory> GetAllCategories();
    }

    public interface IPictureLibrary
    {
        IEnumerable<WebPictureBasic> GetAllPicturesBasic();
        IEnumerable<WebPictureDetailed> GetAllPicturesDetailed();
        WebPictureDetailed GetPictureDetailed(string pictureId);
        IEnumerable<WebCategory> GetAllPictureCategoriesBasic();

    }

    public interface IFileSystemProvider
    {
        IEnumerable<WebDriveBasic> GetLocalDrives();
        IEnumerable<WebFileBasic> GetFilesByPath(string id);
        IEnumerable<WebFolderBasic> GetFoldersByPath(string id);

    }

    public interface ILogger
    {
        void Trace(String _msg);
        void Trace(String _msg, Exception ex);
        void Trace(String _msg, params object[] args);
        void Debug(String _msg);
        void Debug(String _msg, Exception ex);
        void Debug(String _msg, params object[] args);
        void Info(String _msg);
        void Info(String _msg, Exception ex);
        void Info(String _msg, params object[] args);
        void Warn(String _msg);
        void Warn(String _msg, Exception ex);
        void Warn(String _msg, params object[] args);
        void Error(String _msg);
        void Error(String _msg, Exception ex);
        void Error(String _msg, params object[] arg);
        void Fatal(String _msg);
        void Fatal(String _msg, Exception ex);
        void Fatal(String _msg, params object[] args);
    }

    public interface IPluginData
    {
        Dictionary<string, string> Configuration { get; }
        ILogger Log { get; }
    }

    public interface ILazyQuery<T> : IOrderedEnumerable<T>
    {
        IOrderedEnumerable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
        IOrderedEnumerable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
        IEnumerable<T> Where(Expression<Func<T, bool>> predicate);
        int Count();
    }
}
