﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace MPExtended.Services.MetaService.Interfaces
{
    [ServiceContract(Namespace = "http://mpextended.github.io")]
    public interface IProtectedMetaService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        DateTime GetLastClientActivity();
    }
}
