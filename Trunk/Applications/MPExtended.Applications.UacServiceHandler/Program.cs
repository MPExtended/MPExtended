using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ServiceProcess;

namespace MPExtended.Applications.UacServiceHandler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ServiceController sc = new ServiceController("MPExtended Service");
            if (args[0].Equals("start"))
            {
                sc.Start();
            }
            else
            {
                sc.Stop();
            }
        }
    }
}
