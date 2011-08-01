using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using MPExtended.Libraries.ServiceLib;

namespace MPExtended.ServiceHosts.Console.Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Log.Debug("MPExtended.ServiceHosts.Console.SingleSeat starting...");

            ServiceHost host1 = new ServiceHost(typeof(MPExtended.Services.TVAccessService.TVAccessService));
            ServiceHost host2 = new ServiceHost(typeof(MPExtended.Services.MediaAccessService.MediaAccessService));
            ServiceHost host3 = new ServiceHost(typeof(MPExtended.Services.StreamingService.StreamingService));
            Log.Debug("Opening ServiceHost...");

            host1.Open();
            host2.Open();
            host3.Open();
            Log.Debug("Host opened");

            Log.Info("MPExtended.ServiceHosts.Console.SingleSeat started...");
            NLog.LogManager.Flush();

            System.Console.WriteLine("Press ENTER to close");
            System.Console.ReadLine();

            host1.Close();
            host2.Close();
            host3.Close();

            Log.Info("MPExtended.ServiceHosts.Console.SingleSeat closed...");
        }
    }
}
