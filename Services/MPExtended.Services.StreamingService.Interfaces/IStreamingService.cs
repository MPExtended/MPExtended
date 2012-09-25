using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.StreamingService.Interfaces
{
    [ServiceContract(Namespace = "http://mpextended.github.com")]
    public interface IStreamingService
    {
        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetMediaItem(string clientDescription, WebMediaType type, int? provider, string itemId, long? startPosition);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream RetrieveStream(string identifier);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream DoStream(WebMediaType type, int? provider, string itemId, string clientDescription, string profileName, long startPosition, int? idleTimeout);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream CustomTranscoderData(string identifier, string action, string parameters);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream ExtractImage(WebMediaType type, int? provider, string itemId, long position, string format = null);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream ExtractImageResized(WebMediaType type, int? provider, string itemId, long position, int maxWidth, int maxHeight, string borders = null, string format = null);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetImage(WebMediaType type, int? provider, string id, string format = null);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetImageResized(WebMediaType type, int? provider, string id, int maxWidth, int maxHeight, string borders = null, string format = null);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetArtwork(WebMediaType mediatype, int? provider, string id, WebFileType artworktype, int offset, string format = null);

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetArtworkResized(WebMediaType mediatype, int? provider, string id, WebFileType artworktype, int offset, int maxWidth, int maxHeight, string borders = null, string format = null);
    }
}
