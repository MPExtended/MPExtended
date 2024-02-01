using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.UserSessionService.Interfaces
{
    [ServiceContract(Namespace = "http://mpextended.github.io")]
    public interface IUserSessionService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult TestConnection();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebUserServiceDescription GetServiceDescription();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult IsMediaPortalRunning();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult StartMediaPortal();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult StartMediaPortalBlocking();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult SetMediaPortalForeground();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult SetPowerMode(WebPowerMode powerMode);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult CloseMediaPortal();
    }
}