using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    // This is the public API that is exposed by WCF to the client. This can differ
    // from the interfaces described in MediaInterfaces, which are the interfaces used
    // for internal communication, but they have to use the same known media descriptions.

    #region Enums
    [DataContract]
    public enum WebMediaType
    {
        [EnumMember]
        Movie = 0,
        [EnumMember]
        Music = 1,
        [EnumMember]
        Picture = 2,
        [EnumMember]
        TVShow = 3,
        [EnumMember]
        File = 4,
        [EnumMember]
        Folder = 5
    }

    [DataContract]
    public enum OrderBy
    {
        [EnumMember]
        Asc = 0,
        [EnumMember]
        Desc = 1
    }

    [DataContract]
    public enum SortMusicBy
    {
        [EnumMember]
        Title = 0,
        [EnumMember]
        DateAdded = 1,
        [EnumMember]
        Year = 2,
        [EnumMember]
        Genre = 3,
        [EnumMember]
        Rating = 4,
        [EnumMember]
        TrackNumber = 5,
        [EnumMember]
        Composer = 6
    }

    [DataContract]
    public enum SortTVShowsBy
    {
        [EnumMember]
        Title = 0,
        [EnumMember]
        DateAdded = 1,
        [EnumMember]
        Year = 2,
        [EnumMember]
        Genre = 3,
        [EnumMember]
        Rating = 4,
        [EnumMember]
        EpisodeNumber = 5,
        [EnumMember]
        SeasonNumberEpisodeNumber = 6
    }

    [DataContract]
    public enum SortMoviesBy
    {
        [EnumMember]
        Title = 0,
        [EnumMember]
        DateAdded = 1,
        [EnumMember]
        Year = 2,
        [EnumMember]
        Genre = 3,
        [EnumMember]
        Rating = 4
    }

    [DataContract]
    public enum SortPicturesBy
    {
        [EnumMember]
        Title = 0,
        [EnumMember]
        DateAdded = 1,
        [EnumMember]
        Year = 2,
        [EnumMember]
        Genre = 3,
        [EnumMember]
        Rating = 4,
        [EnumMember]
        DateTaken = 5
    }
    #endregion

    [ServiceContract(Namespace = "http://mpextended.codeplex.com")]
    public interface IMediaAccessService
    {

        #region Global
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebServiceDescription GetServiceDescription();
        #endregion

        #region Movies
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetMovieCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieBasic> GetAllMoviesBasic(SortMoviesBy sort = SortMoviesBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieDetailed> GetAllMoviesDetailed(SortMoviesBy sort = SortMoviesBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieBasic> GetMoviesBasicByRange(int start, int end, SortMoviesBy sort = SortMoviesBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieDetailed> GetMoviesDetailedByRange(int start, int end, SortMoviesBy sort = SortMoviesBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieBasic> GetMoviesBasicByGenre(string genre, SortMoviesBy sort = SortMoviesBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieDetailed> GetMoviesDetailedByGenre(string genre, SortMoviesBy sort = SortMoviesBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<string> GetAllMovieGenres();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMovieDetailed GetMovieDetailedById(string id);
        #endregion

        #region Music
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetMusicTrackCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetMusicAlbumCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetMusicArtistCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackBasic> GetAllMusicTracksBasic(SortMusicBy sort = SortMusicBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackDetailed> GetAllMusicTracksDetailed(SortMusicBy sort = SortMusicBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackBasic> GetMusicTracksBasicByRange(int start, int end, SortMusicBy sort = SortMusicBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackDetailed> GetMusicTracksDetailedByRange(int start, int end, SortMusicBy sort = SortMusicBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackBasic> GetMusicTracksBasicByGenre(string genre, SortMusicBy sort = SortMusicBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackDetailed> GetMusicTracksDetailedByGenre(string genre, SortMusicBy sort = SortMusicBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackBasic> GetMusicTracksBasicForAlbum(string id, SortMusicBy sort = SortMusicBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)] 
        IList<WebMusicTrackDetailed> GetMusicTracksDetailedForAlbum(string id, SortMusicBy sort = SortMusicBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<string> GetAllMusicGenres();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicTrackDetailed GetMusicTrackDetailedById(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicAlbumBasic GetMusicAlbumBasicById(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicAlbumBasic> GetAllMusicAlbumsBasic(SortMusicBy sort = SortMusicBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByRange(int start, int end, SortMusicBy sort = SortMusicBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicAlbumBasic> GetMusicAlbumsBasicForArtist(string id, SortMusicBy sort = SortMusicBy.Title, OrderBy order = OrderBy.Asc);

    
        
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicArtistBasic> GetAllMusicArtistsBasic(SortMusicBy sort = SortMusicBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicArtistBasic> GetMusicArtistsBasicByRange(int start, int end, SortMusicBy sort = SortMusicBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicArtistBasic GetMusicArtistBasicById(string id);

        #endregion

        #region Pictures

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetPictureCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPictureBasic> GetAllPicturesBasic(SortPicturesBy sort = SortPicturesBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPictureDetailed> GetAllPicturesDetailed(SortPicturesBy sort = SortPicturesBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebPictureDetailed GetPictureDetailed(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPictureCategoryBasic> GetAllPictureCategoriesBasic();
        #endregion

        #region TVShows

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetTVEpisodeCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetTVEpisodeCountForTVShow(int id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetTVShowCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetTVSeasonCountForTVShow(int id);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowBasic> GetAllTVShowsBasic(SortTVShowsBy sort = SortTVShowsBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowDetailed> GetAllTVShowsDetailed(SortTVShowsBy sort = SortTVShowsBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowBasic> GetTVShowsBasicByRange(int start, int end, SortTVShowsBy sort = SortTVShowsBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowDetailed> GetTVShowsDetailedByRange(int start, int end, SortTVShowsBy sort = SortTVShowsBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVShowDetailed GetTVShowDetailed(string id);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVSeasonBasic> GetAllTVSeasonsBasic(string id, SortTVShowsBy sort = SortTVShowsBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVSeasonDetailed> GetAllTVSeasonsDetailed(string id, SortTVShowsBy sort = SortTVShowsBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVSeasonDetailed GetTVSeasonDetailed(string showId, string seasonId, SortTVShowsBy sort = SortTVShowsBy.Title, OrderBy order = OrderBy.Asc);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetAllTVEpisodesBasicForTVShow(string id, SortTVShowsBy sort = SortTVShowsBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeDetailed> GetAllTVEpisodesDetailedForTVShow(string id, SortTVShowsBy sort = SortTVShowsBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShowByRange(string id, int start, int end, SortTVShowsBy sort = SortTVShowsBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShowByRange(string id, int start, int end, SortTVShowsBy sort = SortTVShowsBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetTVEpisodesBasicForSeason(string showId, string seasonId, SortTVShowsBy sort = SortTVShowsBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForSeason(string showId, string seasonId, SortTVShowsBy sort = SortTVShowsBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVEpisodeDetailed GetTVEpisodeDetailed(string episodeId);
        #endregion

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        string GetPath(WebMediaType type, string id);

    }
}
