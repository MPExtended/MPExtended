using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.ServiceModel;
using MPExtended.Services.MediaAccessService;
using MPExtended.Services.MediaAccessService.Code;

namespace MPExtended.ServiceHosts.CH.SingleSeat
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Log.Debug("GmaWebService ConsoleHost starting....");

            ServiceHost host = new ServiceHost(typeof(MPExtended.Services.MediaAccessService.MediaAccessService));
            ServiceHost host2 = new ServiceHost(typeof(MPExtended.Services.StreamingService.StreamingService));
            ServiceHost host3 = new ServiceHost(typeof(MPExtended.Services.TVAccessService.TVAccessService));
            Log.Debug("Opening ServiceHost...");
            host.Open();
            host2.Open();
            host3.Open();
            Log.Debug("Host opened");

            Log.Info("GmaWebService ConsoleHost started....");
            NLog.LogManager.Flush();

            Console.WriteLine("Press ENTER to close");
            Console.ReadLine();
            host.Close();
            host2.Close();
            host3.Close();
            Log.Debug("GmaWebService ConsoleHost closed...");
           
        }
    }
}
