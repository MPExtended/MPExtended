using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;
using System.IO;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    // This are the interfaces used for internal communication between
    // the service and the backends which provide the data.

    public interface IMusicLibrary
    {
        IList<WebMusicTrackBasic> GetAllTracks();
        IList<WebMusicAlbumBasic> GetAllAlbums();
        IList<WebMusicArtistBasic> GetAllArtists();
        WebMusicTrackBasic GetTrackBasicById(string trackId);
        WebMusicAlbumBasic GetAlbumBasicById(string albumId);
        WebMusicArtistBasic GetArtistBasicById(string artistId);
        IList<WebMusicTrackBasic> GetTracksByAlbumId(string albumId);
        IList<String> GetAllGenres();
        DirectoryInfo GetSourceRootDirectory();
    }

    public interface IMovieLibrary
    {
        IList<WebMovieBasic> GetAllMovies();
        IList<WebMovieDetailed> GetAllMoviesDetailed();
        WebMovieBasic GetMovieBasicById(string movieId);
        WebMovieDetailed GetMovieDetailedById(string movieId);
        IList<String> GetAllGenres();
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
        IList<WebTVEpisodeBasic> GetEpisodesBasic(string seriesId);
        IList<WebTVEpisodeBasic> GetEpisodesBasicForSeason(string seriesId, string seasonId);
        IList<WebTVEpisodeDetailed> GetEpisodesDetailed(string seriesId);
        IList<WebTVEpisodeDetailed> GetEpisodesDetailedForSeason(string seriesId, string seasonId);
        WebTVEpisodeDetailed GetEpisodeDetailed(string episodeId);
        DirectoryInfo GetSourceRootDirectory();
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
