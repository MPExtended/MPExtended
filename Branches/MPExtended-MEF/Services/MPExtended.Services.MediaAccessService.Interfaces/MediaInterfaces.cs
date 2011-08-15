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
    // each interface represents the structure of MEF plugins
    // A new MEF plugin has to implement one of these interfaces in order to be used by the service.

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
        IList<WebTVShowBasic> GetAllTVShows();
        WebTVShowDetailed GetTVShowDetailed(string seriesId);
        IList<WebTVSeasonBasic> GetSeasons(string seriesId);
        IList<WebTVEpisodeBasic> GetEpisodes(string seriesId);
        IList<WebTVEpisodeBasic> GetEpisodesForSeason(string seriesId, string seasonId);
        WebTVEpisodeDetailed GetEpisodeDetailed(string episodeId);
        DirectoryInfo GetSourceRootDirectory();
  
    }
    public interface IPictureLibrary
    {
        IList<WebPictureBasic> GetAllPicturesBasic();
        IList<WebPictureDetailed> GetAllPicturesDetailed();
        WebPictureDetailed GetPictureDetailed(string pictureId);  
      


        DirectoryInfo GetSourceRootDirectory(); 
    }
    public interface IFileSystemProvider
    { 
    
    }
}
