using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Playlist;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    // This is the public API that is exposed by WCF to the client. This can differ
    // from the interfaces described in MediaInterfaces, which are the interfaces used
    // for internal communication, but they have to use the same known media descriptions.

    [ServiceContract(Namespace = "http://mpextended.github.com")]
    public interface IMediaAccessService
    {
        #region Global
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMediaServiceDescription GetServiceDescription();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult TestConnectionToTVService();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMediaItem GetMediaItem(int? provider, WebMediaType type, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebSearchResult> Search(string text);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebSearchResult> SearchResultsByRange(string text, int start, int end);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebDictionary<string> GetExternalMediaInfo(int? provider, WebMediaType type, string id);
        #endregion

        #region Movies
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetMovieCount(int? provider, string filter = null);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieBasic> GetMoviesBasic(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieDetailed> GetMoviesDetailed(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieBasic> GetMoviesBasicByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMovieDetailed> GetMoviesDetailedByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMovieBasic GetMovieBasicById(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMovieDetailed GetMovieDetailedById(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebGenre> GetMovieGenres(int? provider, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebGenre> GetMovieGenresByRange(int? provider, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetMovieGenresCount(int? provider);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebCategory> GetMovieCategories(int? provider, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebActor> GetMovieActors(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebActor> GetMovieActorsByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetMovieActorCount(int? provider, string filter = null);
        #endregion

        #region Music
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetMusicAlbumCount(int? provider, string filter = null);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicAlbumBasic> GetMusicAlbumsBasic(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicAlbumBasic> GetMusicAlbumsBasicByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicAlbumBasic> GetMusicAlbumsBasicForArtist(int? provider, string id, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicAlbumBasic GetMusicAlbumBasicById(int? provider, string id);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetMusicArtistCount(int? provider, string filter = null);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicArtistBasic> GetMusicArtistsBasic(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicArtistBasic> GetMusicArtistsBasicByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicArtistBasic GetMusicArtistBasicById(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicArtistDetailed> GetMusicArtistsDetailed(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicArtistDetailed> GetMusicArtistsDetailedByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicArtistDetailed GetMusicArtistDetailedById(int? provider, string id);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetMusicTrackCount(int? provider, string filter = null);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackBasic> GetMusicTracksBasic(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackDetailed> GetMusicTracksDetailed(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackBasic> GetMusicTracksBasicByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackDetailed> GetMusicTracksDetailedByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackBasic> GetMusicTracksBasicForAlbum(int? provider, string id, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackBasic> GetMusicTracksBasicForArtist(int? provider, string id, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackDetailed> GetMusicTracksDetailedForAlbum(int? provider, string id, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebMusicTrackDetailed> GetMusicTracksDetailedForArtist(int? provider, string id, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicTrackBasic GetMusicTrackBasicById(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMusicTrackDetailed GetMusicTrackDetailedById(int? provider, string id);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebGenre> GetMusicGenres(int? provider, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebGenre> GetMusicGenresByRange(int? provider, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetMusicGenresCount(int? provider);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebCategory> GetMusicCategories(int? provider);
        #endregion

        #region Pictures
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetPictureCount(int? provider);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPictureBasic> GetPicturesBasic(int? provider, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPictureDetailed> GetPicturesDetailed(int? provider, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebPictureBasic GetPictureBasicById(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebPictureDetailed GetPictureDetailedById(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebCategory> GetPictureCategories(int? provider);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebCategory> GetPictureSubCategories(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPictureBasic> GetPicturesBasicByCategory(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPictureDetailed> GetPicturesDetailedByCategory(int? provider, string id);
        #endregion

        #region TVShows
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetTVEpisodeCount(int? provider);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetTVEpisodeCountForTVShow(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetTVEpisodeCountForSeason(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetTVShowCount(int? provider, string filter = null);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetTVSeasonCountForTVShow(int? provider, string id);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowBasic> GetTVShowsBasic(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowDetailed> GetTVShowsDetailed(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowBasic> GetTVShowsBasicByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVShowDetailed> GetTVShowsDetailedByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVShowBasic GetTVShowBasicById(int? provider, string id);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVShowDetailed GetTVShowDetailedById(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVSeasonBasic> GetTVSeasonsBasicForTVShow(int? provider, string id, WebSortField? sort = WebSortField.TVSeasonNumber, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVSeasonDetailed> GetTVSeasonsDetailedForTVShow(int? provider, string id, WebSortField? sort = WebSortField.TVSeasonNumber, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVSeasonBasic GetTVSeasonBasicById(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVSeasonDetailed GetTVSeasonDetailedById(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetTVEpisodesBasic(int? provider, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeDetailed> GetTVEpisodesDetailed(int? provider, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetTVEpisodesBasicByRange(int? provider, int start, int end, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedByRange(int? provider, int start, int end, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShow(int? provider, string id, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShow(int? provider, string id, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetTVEpisodesBasicForTVShowByRange(int? provider, string id, int start, int end, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForTVShowByRange(int? provider, string id, int start, int end, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeBasic> GetTVEpisodesBasicForSeason(int? provider, string id, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVEpisodeDetailed> GetTVEpisodesDetailedForSeason(int? provider, string id, WebSortField? sort = WebSortField.TVEpisodeNumber, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVEpisodeBasic GetTVEpisodeBasicById(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVEpisodeDetailed GetTVEpisodeDetailedById(int? provider, string id);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebCategory> GetTVShowCategories(int? provider);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebGenre> GetTVShowGenres(int? provider, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebGenre> GetTVShowGenresByRange(int? provider, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetTVShowGenresCount(int? provider);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebActor> GetTVShowActors(int? provider, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebActor> GetTVShowActorsByRange(int? provider, int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetTVShowActorCount(int? provider, string filter = null);
        #endregion

        #region Filesystem
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetFileSystemDriveCount(int? provider);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebDriveBasic> GetFileSystemDrives(int? provider, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebDriveBasic> GetFileSystemDrivesByRange(int? provider, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebFolderBasic> GetFileSystemFolders(int? provider, string id, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebFolderBasic> GetFileSystemFoldersByRange(int? provider, string id, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebFileBasic> GetFileSystemFiles(int? provider, string id, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebFileBasic> GetFileSystemFilesByRange(int? provider, string id, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebFilesystemItem> GetFileSystemFilesAndFolders(int? provider, string id, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebFilesystemItem> GetFileSystemFilesAndFoldersByRange(int? provider, string id, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetFileSystemFilesAndFoldersCount(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetFileSystemFilesCount(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetFileSystemFoldersCount(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebDriveBasic GetFileSystemDriveBasicById(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebFolderBasic GetFileSystemFolderBasicById(int? provider, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebFileBasic GetFileSystemFileBasicById(int? provider, string id);
        #endregion

        #region Files
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<string> GetPathList(int? provider, WebMediaType mediatype, WebFileType filetype, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebFileInfo GetFileInfo(int? provider, WebMediaType mediatype, WebFileType filetype, string id, int offset);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult IsLocalFile(int? provider, WebMediaType mediatype, WebFileType filetype, string id, int offset);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream RetrieveFile(int? provider, WebMediaType mediatype, WebFileType filetype, string id, int offset);
        #endregion

        #region Playlist
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPlaylist> GetPlaylists(int? provider);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPlaylistItem> GetPlaylistItems(int? provider, string playlistId, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebPlaylistItem> GetPlaylistItemsByRange(int? provider, string playlistId, int start, int end, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetPlaylistItemsCount(int? provider, string playlistId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult AddPlaylistItem(int? provider, string playlistId, WebMediaType type, string id, int? position);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult AddPlaylistItems(int? provider, string playlistId, WebMediaType type, int? position, string ids);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult RemovePlaylistItem(int? provider, string playlistId, int position);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult RemovePlaylistItems(int? provider, string playlistId, string positions);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult MovePlaylistItem(int? provider, string playlistId, int oldPosition, int newPosition);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebStringResult CreatePlaylist(int? provider, string playlistName);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult DeletePlaylist(int? provider, string playlistId);
        #endregion

        #region Filters
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebIntResult GetFilterValuesCount(int? provider, WebMediaType mediaType, string filterField, string op = null, int? limit = null);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<string> GetFilterValues(int? provider, WebMediaType mediaType, string filterField, string op = null, int? limit = null, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<string> GetFilterValuesByRange(int? provider, int start, int end, WebMediaType mediaType, string filterField, string op = null, int? limit = null, WebSortOrder? order = WebSortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebStringResult CreateFilterString(string field, string op, string value, string conjunction);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebFilterOperator> GetFilterOperators();
        #endregion
    }
}
