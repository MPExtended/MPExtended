using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;

namespace MPExtended.Services.StreamingService.Interfaces
{
    [ServiceContract]
    public interface IStreamingService
    {
        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetMediaItem(WebMediaType type, string itemId);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetImage(string path);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetImageResized(string path, int maxWidth, int maxHeight);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream RetrieveStream(string identifier);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream ExtractImage(WebMediaType type, string itemId, int position);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream ExtractImageResized(WebMediaType type, string itemId, int position, int maxWidth, int maxHeight);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream HttpLiveStreaming(string identifier, string action, string parameters);
    }
}
