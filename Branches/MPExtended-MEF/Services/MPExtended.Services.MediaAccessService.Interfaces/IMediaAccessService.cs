using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    // IMediaAccessService is the "real" api which is exposed by WCF and
    // can basically  differ from the interfaces described
    // in MediaInterfaces except they have to use the same known media descriptions

    [ServiceContract(Namespace = "http://mpextended.codeplex.com")]
    public interface IMediaAccessService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebServiceDescription GetServiceDescription();

    }
}