using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;

namespace MPExtended.Services.StreamingService.Interfaces
{
    [ServiceContract(Namespace = "http://mpextended.github.com")]
    public interface IStreamingService
    {
        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetMediaItem(WebStreamMediaType type, int? provider, string itemId);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream RetrieveStream(string identifier);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream DoStream(WebStreamMediaType type, int? provider, string itemId, string clientDescription, string profileName, int startPosition);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream ExtractImage(WebStreamMediaType type, int? provider, string itemId, int position);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream ExtractImageResized(WebStreamMediaType type, int? provider, string itemId, int position, int maxWidth, int maxHeight);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetImage(WebStreamMediaType type, int? provider, string id);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetImageResized(WebStreamMediaType type, int? provider, string id, int maxWidth, int maxHeight);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream CustomTranscoderData(string identifier, string action, string parameters);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetArtwork(WebStreamMediaType mediatype, int? provider, string id, WebArtworkType artworktype, int offset);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetArtworkResized(WebStreamMediaType mediatype, int? provider, string id, WebArtworkType artworktype, int offset, int maxWidth, int maxHeight);
    }
}
