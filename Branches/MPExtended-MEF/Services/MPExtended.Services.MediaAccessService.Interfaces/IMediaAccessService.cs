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
    // IMediaAccessService is the "real" api which is exposed by WCF and
    // can basically  differ from the interfaces described
    // in MediaInterfaces except they have to use the same known media descriptions
    #region Enum
    [DataContract]
    public enum OrderBy
    {
        [EnumMember]
        Asc = 0,
        [EnumMember]
        Desc = 1

    }
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
    public enum SortMusicBy
    {
        [EnumMember]
        Name = 0,
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
        Name = 0,
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
        Name = 0,
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
        Name = 0,
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

        #region GlobalOperations
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
        IList<WebMovieBasic> GetAllMoviesBasic(SortMoviesBy sort = SortMoviesBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieDetailed> GetAllMoviesDetailed(SortMoviesBy sort = SortMoviesBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieBasic> GetMoviesBasicByRange(int start, int end, SortMoviesBy sort = SortMoviesBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieDetailed> GetMoviesDetailedByRange(int start, int end, SortMoviesBy sort = SortMoviesBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieBasic> GetMoviesBasicByGenre(string genre, SortMoviesBy sort = SortMoviesBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieDetailed> GetMoviesDetailedByGenre(string genre, SortMoviesBy sort = SortMoviesBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<string> GetAllMovieGenres();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMovieDetailed GetMovieDetailedById(string Id);


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
        IList<WebMusicTrackBasic> GetAllMusicTracksBasic(SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackDetailed> GetAllMusicTracksDetailed(SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackBasic> GetMusicTracksBasicByRange(int start, int end, SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackDetailed> GetMusicTracksDetailedByRange(int start, int end, SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackBasic> GetMusicTracksBasicByGenre(string genre, SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackDetailed> GetMusicTracksDetailedByGenre(string genre, SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<string> GetAllMusicGenres();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicTrackDetailed GetMusicTracksDetailedById(string Id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicAlbumBasic> GetAllMusicAlbumsBasic(SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByRange(int start, int end, SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicArtistBasic> GetAllMusicArtistsBasic(SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicArtistBasic> GetMusicArtistsBasicByRange(int start, int end, SortMusicBy sort = SortMusicBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicArtistBasic GetMusicArtistBasicById(string Id);

        #endregion

        #region Pictures

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPictureBasic> GetAllPicturesBasic(SortPicturesBy sort = SortPicturesBy.Name, OrderBy order = OrderBy.Asc);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPictureDetailed> GetAllPicturesDetailed(SortPicturesBy sort = SortPicturesBy.Name, OrderBy order = OrderBy.Asc);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebPictureDetailed GetPictureDetailed(string Id);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPictureCategoryBasic> GetAllPictureCategoriesBasic();

        #endregion

        #region TVShows

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowBasic> GetAllTVShows(SortTVShowsBy sort = SortTVShowsBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVShowDetailed GetTVShowDetailed(string Id);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVSeasonBasic> GetTVSeasons(string Id, SortTVShowsBy sort = SortTVShowsBy.Name, OrderBy order = OrderBy.Asc);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetTVEpisodes(string Id, SortTVShowsBy sort = SortTVShowsBy.Name, OrderBy order = OrderBy.Asc);
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetTVEpisodesForSeason(string Id, string seasonId, SortTVShowsBy sort = SortTVShowsBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVEpisodeDetailed GetTVEpisodeDetailed(string Id);
        #endregion

        #region FileSystem

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        string GetPath(WebMediaType type, string id);
        #endregion



    }
}