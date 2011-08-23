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
        IList<WebMusicTrackBasic> GetAllTracks();
        IList<WebMusicAlbumBasic> GetAllAlbums();
        IList<WebMusicArtistBasic> GetAllArtists();
        IList<WebMusicTrackDetailed> GetAllTracksDetailed();
        WebMusicTrackBasic GetTrackBasicById(string trackId);
        WebMusicAlbumBasic GetAlbumBasicById(string albumId);
        WebMusicArtistBasic GetArtistBasicById(string artistId);
        WebMusicTrackDetailed GetTrackDetailedById(string trackId);
        IList<WebGenre> GetAllGenres();
        DirectoryInfo GetSourceRootDirectory();
    }

    public interface IMovieLibrary
    {
        IList<WebMovieBasic> GetAllMovies();
        IList<WebMovieDetailed> GetAllMoviesDetailed();
        WebMovieBasic GetMovieBasicById(string movieId);
        WebMovieDetailed GetMovieDetailedById(string movieId);
        IList<WebGenre> GetAllGenres();
        DirectoryInfo GetSourceRootDirectory();
    }

    public interface ITVShowLibrary
    {
        IList<WebTVShowBasic> GetAllTVShowsBasic();
        IList<WebTVShowDetailed> GetAllTVShowsDetailed();
        WebTVShowDetailed GetTVShowDetailed(string seriesId);
        IList<WebTVSeasonBasic> GetAllSeasonsBasic(string seriesId);
        IList<WebTVSeasonDetailed> GetAllSeasonsDetailed(string seriesId);
        WebTVSeasonDetailed GetSeasonDetailed(string seriesId, string seasonId);
        IList<WebTVEpisodeBasic> GetAllEpisodesBasic();
        IList<WebTVEpisodeDetailed> GetAllEpisodesDetailed();

        WebTVEpisodeDetailed GetEpisodeDetailed(string episodeId);
        DirectoryInfo GetSourceRootDirectory();
        IList<WebGenre> GetAllGenres();
    }

    public interface IPictureLibrary
    {
        IList<WebPictureBasic> GetAllPicturesBasic();
        IList<WebPictureDetailed> GetAllPicturesDetailed();
        WebPictureDetailed GetPictureDetailed(string pictureId);
        IList<WebPictureCategoryBasic> GetAllPictureCategoriesBasic();  
        DirectoryInfo GetSourceRootDirectory();
    }

    public interface IFileSystemProvider
    { 
    
    }
}
