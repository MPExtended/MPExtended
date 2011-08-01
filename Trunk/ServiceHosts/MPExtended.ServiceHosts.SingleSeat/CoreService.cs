using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.ServiceModel;
using MPExtended.Services.MediaAccessService;
using MPExtended.Services.StreamingService;
using MPExtended.Services.TVAccessService;

namespace MPExtended.ServiceHosts.SingleSeat
{
    public partial class CoreService : ServiceBase
    {
        ServiceHost tvHost = null;
        ServiceHost streamingHost = null;
        ServiceHost mediaHost = null;

        public CoreService()
        {
            InitializeComponent();
            this.ServiceName = "MPExtended SingleSeat Services";
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
            if (mediaHost == null)
            {
                mediaHost = new ServiceHost(typeof(MediaAccessService));
            }

            tvHost.Open();
            streamingHost.Open();
            mediaHost.Open();
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
            if (mediaHost != null)
            {
                mediaHost.Close();
            }
        }
    }
}
