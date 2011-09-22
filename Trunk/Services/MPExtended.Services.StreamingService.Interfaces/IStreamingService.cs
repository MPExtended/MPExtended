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
        Stream GetMediaItem(WebStreamMediaType type, string itemId);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream RetrieveStream(string identifier);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream ExtractImage(WebStreamMediaType type, string itemId, int position);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream ExtractImageResized(WebStreamMediaType type, string itemId, int position, int maxWidth, int maxHeight);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetImage(WebStreamMediaType type, string id);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetImageResized(WebStreamMediaType type, string id, int maxWidth, int maxHeight);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream CustomTranscoderData(string identifier, string action, string parameters);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetArtwork(WebArtworkType artworktype, WebArtworkSource mediatype, string id, int offset, int maxWidth, int maxHeight);
    }
}
