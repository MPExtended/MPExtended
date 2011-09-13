using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;

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
    public enum SortBy
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
        UserDefinedCategories = 5,
        [EnumMember]
        MusicTrackNumber = 6,
        [EnumMember]
        MusicComposer = 7,
        [EnumMember]
        TVEpisodeNumber = 8,
        [EnumMember]
        TVSeasonNumber = 9,
        [EnumMember]
        PictureDateTaken = 10
    }
    #endregion

    [ServiceContract(Namespace = "http://mpextended.codeplex.com")]
    public interface IMediaAccessService
    {

        #region Global
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMediaServiceDescription GetServiceDescription();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]        
        WebMediaItem GetMediaItem(WebMediaType type, string id);
        #endregion

        #region Movies
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebItemCount GetMovieCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieBasic> GetAllMoviesBasic(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieDetailed> GetAllMoviesDetailed(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieBasic> GetMoviesBasicByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieDetailed> GetMoviesDetailedByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieBasic> GetMoviesBasicForCategory(string category, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieDetailed> GetMoviesDetailedForCategory(string category, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieBasic> GetMoviesBasicByGenre(string genre, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieDetailed> GetMoviesDetailedByGenre(string genre, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebGenre> GetAllMovieGenres();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebCategory> GetAllMovieCategories();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMovieDetailed GetMovieDetailedById(string id);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetMovieCover(string id, int offset);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetMovieBackdrop(string id, int offset);
        #endregion

        #region Music
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebItemCount GetMusicTrackCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebItemCount GetMusicAlbumCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebItemCount GetMusicArtistCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackBasic> GetAllMusicTracksBasic(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackDetailed> GetAllMusicTracksDetailed(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackBasic> GetMusicTracksBasicByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackDetailed> GetMusicTracksDetailedByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackBasic> GetMusicTracksBasicByGenre(string genre, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackDetailed> GetMusicTracksDetailedByGenre(string genre, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackBasic> GetMusicTracksBasicForAlbum(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)] 
        IList<WebMusicTrackDetailed> GetMusicTracksDetailedForAlbum(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebGenre> GetAllMusicGenres();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebCategory> GetAllMusicCategories();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicTrackDetailed GetMusicTrackDetailedById(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicAlbumBasic GetMusicAlbumBasicById(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicAlbumBasic> GetAllMusicAlbumsBasic(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByCategory(string category, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByGenre(string genre, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicAlbumBasic> GetMusicAlbumsBasicForArtist(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicArtistBasic> GetAllMusicArtistsBasic(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicArtistBasic> GetMusicArtistsBasicByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicArtistBasic> GetMusicArtistsBasicByCategory(string category, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicArtistBasic GetMusicArtistBasicById(string id);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetMusicCover(string id, int offset);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetMusicBackdrop(string id, int offset);
        #endregion

        #region Pictures
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebItemCount GetPictureCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPictureBasic> GetAllPicturesBasic(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPictureDetailed> GetAllPicturesDetailed(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebPictureDetailed GetPictureDetailed(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebCategory> GetAllPictureCategoriesBasic();
        #endregion

        #region TVShows
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebItemCount GetTVEpisodeCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebItemCount GetTVEpisodeCountForTVShow(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebItemCount GetTVShowCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebItemCount GetTVSeasonCountForTVShow(string id);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowBasic> GetAllTVShowsBasic(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowDetailed> GetAllTVShowsDetailed(SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowBasic> GetTVShowsBasicByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowDetailed> GetTVShowsDetailedByRange(int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowBasic> GetTVShowsBasicByCategory(string category, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowDetailed> GetTVShowsDetailedByCategory(string category, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowBasic> GetTVShowsBasicByGenre(string genre, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowDetailed> GetTVShowsDetailedByGenre(string genre, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVShowDetailed GetTVShowDetailed(string id);


        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetTVShowBanner(string id, int offset);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetTVShowPoster(string id, int offset);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetTVShowBackdrop(string id, int offset);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVSeasonBasic> GetAllTVSeasonsBasic(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVSeasonDetailed> GetAllTVSeasonsDetailed(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVSeasonDetailed GetTVSeasonDetailed(string showId, string seasonId, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);


        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetTVSeasonBanner(string showId, string seasonId, int offset);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetTVSeasonPoster(string showId, string seasonId, int offset);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetTVSeasonBackdrop(string showId, string seasonId, int offset);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetAllTVEpisodesBasicForTVShow(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeDetailed> GetAllTVEpisodesDetailedForTVShow(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShowByRange(string id, int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShowByRange(string id, int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetTVEpisodesBasicForSeason(string showId, string seasonId, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForSeason(string showId, string seasonId, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVEpisodeDetailed GetTVEpisodeDetailed(string id);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebCategory> GetAllTVShowCategories();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebGenre> GetAllTVShowGenres();
        #endregion

        #region Filesystem
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebDriveBasic> GetFileSystemDrives();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebFolderBasic> GetFileSystemFoldersListing(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebFileBasic> GetFileSystemFilesListing(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebFileBasic GetFileSystemFile(string id);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        bool IsLocalFile(WebMediaType type, string id, int offset);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream RetrieveFile(WebMediaType type, string id, int offset);
        #endregion
    }
}
