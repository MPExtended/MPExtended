using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    // each interface represents the structure of MEF plugins
    // A new MEF plugin has to implement one of these interfaces in order to be used by the service.

    public interface IMusicLibrary
    {
        List<WebMusicTrackBasic> GetAllTracks();
        List<WebMusicAlbumBasic> GetAllAlbums();
        List<WebMusicArtistBasic> GetAllArtists();
        WebMusicTrackBasic GetTrackBasicById(string trackId);
        WebMusicAlbumBasic GetAlbumBasicById(string albumId);
        WebMusicArtistBasic GetArtistBasicById(string artistId);
        List<WebMusicTrackBasic> GetTracksByAlbumId(string albumId);
        WebMusicArtistBasic GetArtistByAlbumId(string albumId);
        List<String> GetExistingGenre();
        List<WebMusicAlbumBasic> GetAlbumsByGenre(string genre);
    }
    public interface IMovieLibrary
    {
        List<WebMovieBasic> GetAllMovies();
        List<WebMovieDetailed> GetAllMoviesDetailed();
        WebMovieBasic GetMovieBasicById(string movieId);
        WebMovieDetailed GetMovieDetailedById(string movieId);
        List<WebMovieDetailed> GetMoviesDetailedByGenre(string genre);
        List<String> GetExistingGenre();

    }
    public interface ITVShowLibrary
    {
        public IList<WebTVShowBasic> GetAllSeries();
        public WebTVShowDetailed GetTVShowDetailed(string seriesId);
        public IList<WebTVSeason> GetSeasons(string seriesId);
        public IList<WebTVEpisodeBasic> GetEpisodes(string seriesId);
        public IList<WebTVEpisodeBasic> GetEpisodesForSeason(string seriesId, string seasonId);
        public WebTVEpisodeDetailed GetEpisodeDetailed(string episodeId);
    }
    public interface IPictureLibrary
    {

    }
}
