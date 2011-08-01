using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Services.TVAccessService;

namespace MPExtended.ServiceHosts.TVAccessService
{
    public partial class CoreService : ServiceBase
    {
        private ServiceHost serviceHost = null;

        public CoreService()
        {
            this.ServiceName = "MPExtended TVAccessService";
        }

        protected override void OnStart(string[] args)
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
            }

            serviceHost = new ServiceHost(typeof(MPExtended.Services.TVAccessService.TVAccessService));

            foreach (ServiceEndpoint se in serviceHost.Description.Endpoints)
            {
                if (se.Name == "JsonEndpoint" || se.Name == "StreamEndpoint")
                    se.Behaviors.Add(new WebHttpWithCustomExceptionHandling());
            }

            serviceHost.Open();
        }

        protected override void OnStop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
        }
    }
}
