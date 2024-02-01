using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MetaService.Interfaces;

namespace MPExtended.Services.UserSessionService.Interfaces
{
    [ServiceContract(Namespace = "http://mpextended.github.io")]
    public interface IPrivateUserSessionService
    {
        [OperationContract]
        WebBoolResult OpenConfigurator();

        [OperationContract]
        WebBoolResult RequestAccess(string token, string clientName, string ipAddress, List<String> user);

        [OperationContract]
        WebAccessRequestResponse GetAccessRequestStatus(string token);

        [OperationContract]
        WebBoolResult CancelAccessRequest(string token);
    }
}
