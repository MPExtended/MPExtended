using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

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
        bool TestConnectionToTVService();

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
        string ReadSettingFromDatabase(string tagName);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        void WriteSettingToDatabase(string tagName, string value);
        #endregion

        #region Schedules
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        void StartRecordingManual(string userName, int channelId, string title);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        void AddSchedule(int channelId, string title, DateTime startTime, DateTime endTime, int scheduleType);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        void AddScheduleDetailed(int channelId, string title, DateTime startTime, DateTime endTime, int scheduleType, int preRecordInterval, int postRecordInterval, string directory, int priority);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebScheduleBasic> GetSchedules();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebScheduleBasic GetScheduleById(int scheduleId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        void CancelSchedule(int programId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        void DeleteSchedule(int scheduleId);
        #endregion

        #region Recordings
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebRecordingBasic> GetRecordings();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebRecordingBasic GetRecordingById(int id);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebFileInfo GetFileInfo(int recordingId);
        #endregion

        #region Channels
        #region Tv specific
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelGroup> GetGroups();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebChannelGroup GetGroupById(int groupId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetChannelCount(int groupId);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelBasic> GetChannelsBasic(int groupId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelBasic> GetChannelsBasicByRange(int groupId, int startIndex, int count);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelDetailed> GetChannelsDetailed(int groupId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelDetailed> GetChannelsDetailedByRange(int groupId, int startIndex, int count);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelState> GetAllChannelStatesForGroup(int groupId, string userName);
        #endregion

        #region Radio specific
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelGroup> GetRadioGroups();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebChannelGroup GetRadioGroupById(int groupId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetRadioChannelCount(int groupId);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelBasic> GetRadioChannelsBasic(int groupId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelBasic> GetRadioChannelsBasicByRange(int groupId, int startIndex, int count);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelDetailed> GetRadioChannelsDetailed(int groupId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebChannelDetailed> GetRadioChannelsDetailedByRange(int groupId, int startIndex, int count);

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
        void SendHeartbeat(string userName);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        bool CancelCurrentTimeShifting(string userName);
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
        WebProgramDetailed GetCurrentProgramOnChannel(int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebProgramDetailed GetNextProgramOnChannel(int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebProgramBasic> SearchProgramsBasic(string searchTerm);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebProgramDetailed> SearchProgramsDetailed(string searchTerm);

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
        bool GetProgramIsScheduledOnChannel(int channelId, int programId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        bool GetProgramIsScheduled(int programId);
        #endregion
    }
}
