using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;
using System.IO;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

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
        DirectoryInfo GetSourceRootDirectory();
    }

    public interface IMovieLibrary
    {
        IEnumerable<WebMovieBasic> GetAllMovies();
        IEnumerable<WebMovieDetailed> GetAllMoviesDetailed();
        WebMovieBasic GetMovieBasicById(string movieId);
        WebMovieDetailed GetMovieDetailedById(string movieId);
        IEnumerable<WebGenre> GetAllGenres();
        IEnumerable<WebCategory> GetAllCategories();
        DirectoryInfo GetSourceRootDirectory();
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
        DirectoryInfo GetSourceRootDirectory();
    }

    public interface IPictureLibrary
    {
        IEnumerable<WebPictureBasic> GetAllPicturesBasic();
        IEnumerable<WebPictureDetailed> GetAllPicturesDetailed();
        WebPictureDetailed GetPictureDetailed(string pictureId);
        IEnumerable<WebCategory> GetAllPictureCategoriesBasic();
        DirectoryInfo GetSourceRootDirectory();
    }

    public interface IFileSystemProvider
    {

    }
}
