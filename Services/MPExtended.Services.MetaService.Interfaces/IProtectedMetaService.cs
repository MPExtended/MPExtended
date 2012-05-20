using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace MPExtended.Services.MetaService.Interfaces
{
    [ServiceContract(Namespace = "http://mpextended.github.com")]
    public interface IProtectedMetaService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        void DummyMethod();
    }
}
