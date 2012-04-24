using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    [ServiceContract(Namespace = "http://mpextended.github.com")]
    public interface ITVAccessService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTVServiceDescription GetServiceDescription();

        #region TV Server
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult TestConnectionToTVService();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        string ReadSettingFromDatabase(string tagName);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult WriteSettingToDatabase(string tagName, string value);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVSearchResult> Search(string text, WebTVSearchResultType? type = null);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        SerializableDictionary<string> GetExternalMediaInfo(WebTvMediaType? type, string id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebTVSearchResult> SearchResultsByRange(string text, int start, int end, WebTVSearchResultType? type = null);
        #endregion

        #region Cards
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebCard> GetCards();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebVirtualCard> GetActiveCards();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebUser> GetActiveUsers();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebRtspClient> GetStreamingClients();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebDiskSpaceInformation> GetAllRecordingDiskInformation();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebDiskSpaceInformation GetRecordingDiskInformationForCard(int id);
        #endregion

        #region Schedules
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult StartRecordingManual(string userName, int channelId, string title);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult AddSchedule(int channelId, string title, DateTime startTime, DateTime endTime, WebScheduleType scheduleType);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult AddScheduleDetailed(int channelId, string title, DateTime startTime, DateTime endTime, WebScheduleType scheduleType, int preRecordInterval, int postRecordInterval, string directory, int priority);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebCount GetScheduleCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebScheduleBasic> GetSchedules(SortField? sort = SortField.Name, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebScheduleBasic> GetSchedulesByRange(int start, int end, SortField? sort = SortField.Name, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebScheduleBasic GetScheduleById(int scheduleId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult CancelSchedule(int programId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult DeleteSchedule(int scheduleId);
        #endregion

        #region Recordings
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebCount GetRecordingCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebRecordingBasic> GetRecordings(SortField? sort = SortField.Name, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebRecordingBasic> GetRecordingsByRange(int start, int end, SortField? sort = SortField.Name, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebRecordingBasic GetRecordingById(int id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebRecordingFileInfo GetRecordingFileInfo(int id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        Stream ReadRecordingFile(int id);
        #endregion

        #region Channels
        #region TV
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebCount GetGroupCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelGroup> GetGroups(SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelGroup> GetGroupsByRange(int start, int end, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebChannelGroup GetGroupById(int groupId);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebCount GetChannelCount(int groupId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelBasic> GetAllChannelsBasic(SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelBasic> GetChannelsBasic(int groupId, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelBasic> GetChannelsBasicByRange(int groupId, int start, int end, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelDetailed> GetAllChannelsDetailed(SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelDetailed> GetChannelsDetailed(int groupId, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelDetailed> GetChannelsDetailedByRange(int groupId, int start, int end, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelState> GetAllChannelStatesForGroup(int groupId, string userName);
        #endregion

        #region Radio specific
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebCount GetRadioGroupCount();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelGroup> GetRadioGroups(SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelGroup> GetRadioGroupsByRange(int start, int end, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebChannelGroup GetRadioGroupById(int groupId);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebCount GetRadioChannelCount(int groupId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelBasic> GetAllRadioChannelsBasic(SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelBasic> GetRadioChannelsBasic(int groupId, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelBasic> GetRadioChannelsBasicByRange(int groupId, int start, int end, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelDetailed> GetAllRadioChannelsDetailed(SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelDetailed> GetRadioChannelsDetailed(int groupId, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelDetailed> GetRadioChannelsDetailedByRange(int groupId, int start, int end, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelState> GetAllRadioChannelStatesForGroup(int groupId, string userName);
        #endregion


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebChannelBasic GetChannelBasicById(int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebChannelDetailed GetChannelDetailedById(int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebChannelState GetChannelState(int channelId, string userName);
        #endregion

        #region Timeshifting
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebVirtualCard SwitchTVServerToChannelAndGetVirtualCard(string userName, int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        string SwitchTVServerToChannelAndGetStreamingUrl(string userName, int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        string SwitchTVServerToChannelAndGetTimeshiftFilename(string userName, int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult SendHeartbeat(string userName);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult CancelCurrentTimeShifting(string userName);
        #endregion

        #region EPG
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebProgramBasic> GetProgramsBasicForChannel(int channelId, DateTime startTime, DateTime endTime);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebProgramDetailed> GetProgramsDetailedForChannel(int channelId, DateTime startTime, DateTime endTime);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelPrograms<WebProgramBasic>> GetProgramsBasicForGroup(int channelGroup, DateTime startTime, DateTime endTime);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelPrograms<WebProgramDetailed>> GetProgramsDetailedForGroup(int channelGroup, DateTime startTime, DateTime endTime);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebProgramDetailed GetCurrentProgramOnChannel(int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebProgramDetailed GetNextProgramOnChannel(int channelId);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebCount SearchProgramsCount(string searchTerm);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebProgramBasic> SearchProgramsBasic(string searchTerm);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebProgramBasic> SearchProgramsBasicByRange(string searchTerm, int start, int end);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebProgramDetailed> SearchProgramsDetailed(string searchTerm);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebProgramDetailed> SearchProgramsDetailedByRange(string searchTerm, int start, int end);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebProgramBasic> GetNowNextWebProgramBasicForChannel(int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebProgramDetailed> GetNowNextWebProgramDetailedForChannel(int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebProgramBasic GetProgramBasicById(int programId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebProgramDetailed GetProgramDetailedById(int programId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult GetProgramIsScheduledOnChannel(int channelId, int programId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult GetProgramIsScheduled(int programId);
        #endregion
    }
}
