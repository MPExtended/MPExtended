using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace MPExtended.Services.StreamingService.Interfaces
{
    [ServiceContract]
    public interface IWebStreamingService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebStreamServiceDescription GetServiceDescription();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebTranscoderProfile> GetTranscoderProfiles();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebTranscoderProfile> GetTranscoderProfilesForTarget(String target);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTranscoderProfile GetTranscoderProfileByName(string name);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebMediaInfo GetMediaInfo(WebStreamMediaType type, string itemId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebTranscodingInfo GetTranscodingInfo(string identifier);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        bool InitStream(WebStreamMediaType type, string itemId, string clientDescription, string identifier);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        string StartStream(string identifier, string profileName, int startPosition);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        string StartStreamWithStreamSelection(string identifier, string profileName, int startPosition, int audioId, int subtitleId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        bool FinishStream(string identifier);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebStreamingSession> GetStreamingSessions();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebResolution GetStreamSize(WebStreamMediaType type, string itemId, string profile);
    }
}
