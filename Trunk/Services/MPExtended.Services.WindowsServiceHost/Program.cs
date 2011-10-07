#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace MPExtended.Services.WindowsServiceHost
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            bool runAsService = true;
            if (args.Length > 0 && args[0] == "/noservice")
                runAsService = false;

            if (runAsService)
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
			    { 
    				new CoreService() 
			    };
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                System.Threading.Thread.Sleep(15000);

                Console.WriteLine("Starting WCF host");
                WCFHost host = new WCFHost();
                host.Start();

                while (true)
                {
                    System.Threading.Thread.Sleep(60000);
                }

                Console.WriteLine("Stopping WCF host");
                host.Stop();
            }
        }
    }
}
