using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.UserSessionService.Interfaces
{
    [ServiceContract(Namespace = "http://mpextended.github.com")]
    public interface IPrivateUserSessionService
    {
        [OperationContract]
        WebBoolResult OpenConfigurator();

        [OperationContract]
        WebStringResult RequestAccess(string clientName, string ipAddress, List<String> user);


        [OperationContract]
        WebBoolResult CancelAccessRequest();
    }
}
