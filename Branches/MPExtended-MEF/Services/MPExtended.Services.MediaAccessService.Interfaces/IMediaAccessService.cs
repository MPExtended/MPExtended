using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    // IMediaAccessService is the "real" api which is exposed by WCF and
    // can basically  differ from the interfaces described
    // in MediaInterfaces except they have to use the same known media descriptions

    [ServiceContract(Namespace = "http://mpextended.codeplex.com")]
    public interface IMediaAccessService
    {
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
            SeasonNumber_EpisodeNumber = 6
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
            DateTaken
        }
        #endregion

        #region GlobalOperations
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebServiceDescription GetServiceDescription();
        #endregion

        #region Movies

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
        IList<WebMovieBasic> GetMoviesBasicByGenre(string genre,SortMoviesBy sort = SortMoviesBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieDetailed> GetMoviesDetailedByGenre(string genre, SortMoviesBy sort = SortMoviesBy.Name, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<string> GetAllGenre();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMovieDetailed GetMovieDetailedById(string movieId);


        #endregion

        #region Music
        #endregion

        #region Pictures
        #endregion

        #region TVShows
        #endregion



    }
}