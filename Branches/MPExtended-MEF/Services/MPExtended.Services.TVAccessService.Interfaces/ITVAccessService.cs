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
    [ServiceContract(Namespace = "http://mpextended.codeplex.com")]
    public interface ITVAccessService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        bool TestConnectionToTVService();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        void AddSchedule(int channelId, string title, DateTime startTime, DateTime endTime, int scheduleType);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        void AddScheduleDetailed(int channelId, string title, DateTime startTime, DateTime endTime, int scheduleType, int preRecordInterval, int postRecordInterval, string directory, int priority);

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
        Dictionary<int, WebChannelState> GetAllChannelStatesForGroup(int groupId, string userName);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebChannelState GetChannelState(int channelId, string userName);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        bool CancelCurrentTimeShifting(string userName);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebChannelDetailed> GetChannelsDetailed(int groupId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        Stream GetChannelLogo(int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebChannelDetailed> GetChannelsDetailedByRange(int groupId, int startIndex, int count);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebChannelBasic> GetChannelsBasic(int groupId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebChannelBasic> GetChannelsBasicByRange(int groupId, int startIndex, int count);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        int GetChannelCount(int groupId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebRecordingBasic> GetRecordings();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebScheduleBasic> GetSchedules();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebScheduleBasic GetScheduleById(int scheduleId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebChannelDetailed GetChannelDetailedById(int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebChannelBasic GetChannelBasicById(int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebProgramDetailed GetProgramDetailedById(int programId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebProgramBasic GetProgramBasicById(int programId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebProgramBasic> GetNowNextWebProgramBasicForChannel(int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebProgramDetailed> GetNowNextWebProgramDetailedForChannel(int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebProgramDetailed> GetProgramsDetailedForChannel(int channelId, DateTime startTime, DateTime endTime);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebProgramBasic> GetProgramsBasicForChannel(int channelId, DateTime startTime, DateTime endTime);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        bool GetProgramIsScheduledOnChannel(int channelId, int programId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        bool GetProgramIsScheduled(int programId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebProgramDetailed> SearchProgramsDetailed(string searchTerm);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebProgramBasic> SearchProgramsBasic(string searchTerm);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebChannelGroup> GetGroups();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebChannelGroup GetGroupById(int groupId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        void CancelSchedule(int programId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        void DeleteSchedule(int scheduleId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebCard> GetCards();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebVirtualCard> GetActiveCards();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebRtspClient> GetStreamingClients();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebUser> GetActiveUsers();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebProgramDetailed GetCurrentProgramOnChannel(int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebProgramDetailed GetNextProgramOnChannel(int channelId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        string ReadSettingFromDatabase(string tagName);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        void WriteSettingToDatabase(string tagName, string value);
    }
}