using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace MPExtended.Services.MetaService.Interfaces
{
    [ServiceContract(Namespace = "http://mpextended.github.com")]
    public interface IMetaService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebService> GetInstalledServices();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebServiceSet> GetActiveServiceSets();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBool HasUI();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebServiceVersion GetVersion();
    }
}
