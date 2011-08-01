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
namespace MPExtended.ServiceHosts.Client
{
    public partial class CoreService : ServiceBase
    {
        ServiceHost mediaHost = null;
        ServiceHost streamingHost = null;

        public CoreService()
        {
            InitializeComponent();
            this.ServiceName = "MPExtended Client Services";
        }

        protected override void OnStart(string[] args)
        {
            if (mediaHost == null)
            {
                mediaHost = new ServiceHost(typeof(MediaAccessService));
            }
            if (streamingHost == null)
            {
                streamingHost = new ServiceHost(typeof(StreamingService));
            }
            mediaHost.Open();
            streamingHost.Open();
    
        }

        protected override void OnStop()
        {
            if (mediaHost != null)
            {
                mediaHost.Close();
            }
            if (streamingHost != null)
            {
                streamingHost.Close();
            }
        }
    }
}
