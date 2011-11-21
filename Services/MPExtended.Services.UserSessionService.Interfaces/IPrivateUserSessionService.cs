using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace MPExtended.Services.UserSessionService.Interfaces
{
    [ServiceContract(Namespace = "http://mpextended.github.com")]
    public interface IPrivateUserSessionService
    {
        [OperationContract]
        WebResult OpenConfigurator();
    }
}
