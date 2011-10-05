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

    [ServiceContract(Namespace = "http://mpextended.codeplex.com")]
    public interface IMediaAccessService
    {

        #region Global
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMediaServiceDescription GetServiceDescription();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]        
        ConcreteWebMediaItem GetMediaItem(WebMediaType type, string id);
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
        WebMovieBasic GetMovieBasicById(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMovieDetailed GetMovieDetailedById(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebGenre> GetAllMovieGenres();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebCategory> GetAllMovieCategories();
        #endregion

        #region Music
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebItemCount GetMusicAlbumCount();

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
        WebMusicAlbumBasic GetMusicAlbumBasicById(string id);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebItemCount GetMusicArtistCount();

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
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebItemCount GetMusicTrackCount();

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
        WebMusicTrackBasic GetMusicTrackBasicById(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicTrackDetailed GetMusicTrackDetailedById(string id);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebGenre> GetAllMusicGenres();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebCategory> GetAllMusicCategories();
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
        WebPictureBasic GetPictureBasicById(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebPictureDetailed GetPictureDetailedById(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebCategory> GetAllPictureCategoriesBasic();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPictureBasic> GetPicturesBasicByCategory(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPictureDetailed> GetPicturesDetailedByCategory(string id);
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
        WebItemCount GetTVEpisodeCountForSeason(string id);

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
        WebTVShowBasic GetTVShowBasicById(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVShowDetailed GetTVShowDetailedById(string id);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVSeasonBasic> GetTVSeasonsBasicForTVShow(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVSeasonDetailed> GetTVSeasonsDetailedForTVShow(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVSeasonBasic GetTVSeasonBasicById(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVSeasonDetailed GetTVSeasonDetailedById(string id);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShow(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShow(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShowByRange(string id, int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShowByRange(string id, int start, int end, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetTVEpisodesBasicForSeason(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForSeason(string id, SortBy sort = SortBy.Title, OrderBy order = OrderBy.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVEpisodeBasic GetTVEpisodeBasicById(string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVEpisodeDetailed GetTVEpisodeDetailedById(string id);


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
        WebFileBasic GetFileSystemFileBasicById(string id);
        #endregion

        #region Files
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<string> GetPathList(WebMediaType mediatype, WebFileType filetype, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebFileInfo GetFileInfo(WebMediaType mediatype, WebFileType filetype, string id, int offset);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        bool IsLocalFile(WebMediaType mediatype, WebFileType filetype, string id, int offset);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream RetrieveFile(WebMediaType mediatype, WebFileType filetype, string id, int offset);
        #endregion
    }
}
