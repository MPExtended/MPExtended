using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using MPExtended.Services.MediaAccessService.Code;

namespace MPExtended.ServiceHosts.CH.MediaAccessService
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
            Log.Debug("Opening ServiceHost...");
            host.Open();
            Log.Debug("Host opened");

            Log.Info("GmaWebService ConsoleHost started....");
            NLog.LogManager.Flush();

            Console.WriteLine("Press ENTER to close");
            Console.ReadLine();
            host.Close();

            Log.Debug("GmaWebService ConsoleHost closed...");
      
        }
    }
}
