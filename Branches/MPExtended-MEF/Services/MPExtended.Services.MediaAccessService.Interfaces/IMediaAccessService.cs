using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    [DataContract]
    public enum OrderBy
    {
        [EnumMember]
        Asc = 0,
        [EnumMember]
        Desc = 1
    }

    [DataContract]
    public enum SortBy
    {
        [EnumMember]
        Name = 0,
        [EnumMember]
        Date = 1,
        [EnumMember]
        TrackNumber = 2,
        [EnumMember]
        Year = 3,
        [EnumMember]
        Genre = 4,
        [EnumMember]
        Composer = 5,
        [EnumMember]
        EpisodeNumber = 6,
        [EnumMember]
        Rating = 7,
        [EnumMember]
        SeasonNumber_EpisodeNumber = 8,
        [EnumMember]
        DateAdded = 9
    }

    [ServiceContract(Namespace = "http://mpextended.codeplex.com")]
    public interface IMediaAccessService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebServiceDescription GetServiceDescription();

        #region Filesystem
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<String> GetDirectoryListByPath(string path);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebFileInfo> GetFilesFromDirectory(string filepath);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebFileInfo GetFileInfo(MediaItemType type, string itemId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        string GetPath(MediaItemType type, string itemId);
        #endregion

        #region Music
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebShare> GetAllMusicShares();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicTrack GetMusicTrack(int trackId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMusicTrack> GetAllMusicTracks(SortBy sort = SortBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMusicTrack> GetMusicTracks(int startIndex, int endIndex, SortBy sort = SortBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetMusicTracksCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMusicAlbum> GetAllAlbums(SortBy sort = SortBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicAlbum GetAlbum(string albumArtistName, string albumName);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMusicAlbum> GetAlbums(int startIndex, int endIndex, SortBy sort = SortBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetAlbumsCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMusicArtist> GetAllArtists(OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMusicArtist> GetArtists(int startIndex, int endIndex, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetArtistsCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMusicAlbum> GetAlbumsByArtist(String artistName, SortBy sort = SortBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMusicTrack> GetSongsOfAlbum(String albumName, String albumArtistName, SortBy sort = SortBy.TrackNumber, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMusicTrack> FindMusicTracks(string album, string artist, string title, SortBy sort = SortBy.Name, OrderBy order = OrderBy.Asc);
        #endregion

        #region Videos
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebShare> GetVideoShares();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetVideosCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMovie> GetAllVideos(SortBy sort = SortBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMovie> GetVideos(int startIndex, int endIndex, SortBy sort = SortBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMovieFull GetFullVideo(int videoId);
        #endregion

        #region TvSeries
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetSeriesCount();
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebSeries> GetAllSeries(SortBy sort = SortBy.Name, OrderBy order = OrderBy.Asc);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebSeries> GetSeries(int startIndex, int endIndex, SortBy sort = SortBy.Name, OrderBy order = OrderBy.Asc);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebSeriesFull GetFullSeries(int seriesId);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebSeason> GetAllSeasons(int seriesId, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebSeason GetSeason(int seriesId, int seasonNumber);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebEpisode> GetAllEpisodes(int seriesId, SortBy sort = SortBy.SeasonNumber_EpisodeNumber, OrderBy order = OrderBy.Asc);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebEpisode> GetAllEpisodesForSeason(int seriesId, int seasonNumber, SortBy sort = SortBy.EpisodeNumber, OrderBy order = OrderBy.Asc);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebEpisode> GetEpisodes(int seriesId, int startIndex, int endIndex, SortBy sort = SortBy.SeasonNumber_EpisodeNumber, OrderBy order = OrderBy.Asc);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebEpisode> GetEpisodesForSeason(int seriesId, int seasonId, int startIndex, int endIndex, SortBy sort = SortBy.EpisodeNumber, OrderBy order = OrderBy.Asc);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetEpisodesCount(int seriesId);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetEpisodesCountForSeason(int seriesId, int season);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebEpisodeFull GetFullEpisode(int episodeId);
        #endregion

        #region Movies
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMovieFull GetFullMovie(int movieId);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetMovieCount();
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMovie> GetAllMovies(SortBy sort = SortBy.Name, OrderBy order = OrderBy.Asc);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMovie> GetMovies(int startIndex, int endIndex, SortBy sort = SortBy.Name, OrderBy order = OrderBy.Asc);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMovieFull> GetAllMoviesDetailed(SortBy sort = SortBy.Name, OrderBy order = OrderBy.Asc);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMovieFull> GetMoviesDetailed(int startIndex, int endIndex, SortBy sort = SortBy.Name, OrderBy order = OrderBy.Asc);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebMovie> SearchForMovie(String searchString);
        #endregion

        #region Pictures
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebShare> GetPictureShares();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebPictureDirectory GetPictureDirectory(string path);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebPicture GetPicture(string path);
        #endregion
    }
}