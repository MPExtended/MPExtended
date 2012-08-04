using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.MetaService.Interfaces
{
    [ServiceContract(Namespace = "http://mpextended.github.com")]
    public interface IMetaService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult TestConnection();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebService> GetInstalledServices();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebService> GetActiveServices();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebServiceSet> GetActiveServiceSets();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult HasUI();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebServiceVersion GetVersion();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebAccessRequestResponse CreateAccessRequest(string clientName);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebAccessRequestResponse GetAccessRequestStatus(string token);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebAccessRequestResponse GetAccessRequestStatusBlocking(string token, int timeout);
    }
}
