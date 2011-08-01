using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.ServiceHosts.CH.TVAccessService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Console.WriteLine("TV4Home.Server.CoreService.ConsoleHost starting....");
            Console.WriteLine();

            Console.WriteLine("Opening service host...");
            ServiceHost serviceHost = new ServiceHost(typeof(MPExtended.Services.StreamingService.StreamingService));
            ServiceHost serviceHost2 = new ServiceHost(typeof(MPExtended.Services.TVAccessService.TVAccessService));


            serviceHost.Open();
            serviceHost2.Open();
            Console.WriteLine();
            Console.WriteLine("Finished. Press any key to exit.");
            Console.ReadLine();
            serviceHost.Close();
            serviceHost2.Close();
        }
    }
}
