using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.ServiceModel;
using MPExtended.Services.TVAccessService;
using MPExtended.Services.StreamingService;

namespace MPExtended.ServiceHosts.Server
{
    public partial class CoreService : ServiceBase
    {
        ServiceHost tvHost = null;
        ServiceHost streamingHost = null;

        public CoreService()
        {
            InitializeComponent();
            this.ServiceName = "MPExtended Server Services";
        }

        protected override void OnStart(string[] args)
        {
            if (tvHost == null)
            {
                tvHost = new ServiceHost(typeof(TVAccessService));
            }
            if (streamingHost == null)
            {
                streamingHost = new ServiceHost(typeof(StreamingService));
            }
            tvHost.Open();
            streamingHost.Open();

        }

        protected override void OnStop()
        {
            if (tvHost != null)
            {
                tvHost.Close();
            }
            if (streamingHost != null)
            {
                streamingHost.Close();
            }
        }
    }
}
