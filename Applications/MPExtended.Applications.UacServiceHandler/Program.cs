#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Windows.Forms;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Reflection;

namespace MPExtended.Applications.UacServiceHandler
{
    public static class Program
    {
        static String SERVICE_NAME = "MPExtended Service";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                try
                {
                    String command = args[0];
                    ServiceController sc = new ServiceController(SERVICE_NAME);
                    if (command.Equals("start"))
                    {
                        sc.Start();
                    }
                    else if (command.Equals("stop"))
                    {
                        sc.Stop();
                    }
                    else if (command.Equals("restart"))
                    {
                        RestartService(sc, 20000);
                    }
                    else if (command.Equals("changeuser"))
                    {
                        //String file =  Assembly.GetExecutingAssembly().Location ;

                        //String file = @"C:\Users\DieBagger\Documents\Projects\MediaPortal\MP-Extended\TFS\Trunk\Services\MPExtended.Services.WindowsServiceHost\bin\Debug\MPExtended.Services.WindowsServiceHost.exe";
                        //ManagedInstallerClass.InstallHelper(new string[] { "/u", file });
                        //ManagedInstallerClass.InstallHelper(new string[] { file });

                        //ManagedInstallerClass.InstallHelper(new string[] { "/u", @"C:\Program Files (x86)\MPExtended\Service\MPExtended.Services.WindowsServiceHost.exe" });
                        //ManagedInstallerClass.InstallHelper(new string[] { @"C:\Program Files (x86)\MPExtended\Service\MPExtended.Services.WindowsServiceHost.exe" });
                        /**
                            //TODO: for this to work we need to do the following:
                                * Add the WindowsServiceInstaller to the CoreService project
                         * Uninstall the service 
                         * Write all the service information (name, user, pass, description, startup type) to a place where WindowsServiceInstaller can access them
                         * Install service again, in WindowsServiceInstaller read the values and set them to the service
                        
                                                  [RunInstaller(true)]
                            public class WindowsServiceInstaller : Installer
                            {
                                public WindowsServiceInstaller()
                                {
                         
                                    ServiceInstaller si = new ServiceInstaller();
                                    si.StartType = ServiceStartMode.Automatic; // get this value from some global variable
                                    si.ServiceName = @"MPExtended Service";
                                    si.DisplayName = @"MPExtended Service";
                                    this.Installers.Add(si);

                                    ServiceProcessInstaller spi = new ServiceProcessInstaller();
                                    spi.Account = System.ServiceProcess.ServiceAccount.User;
                                    spi.Username = ".\\username";
                                    spi.Password = "Password";
                                    this.Installers.Add(spi);
                          
                         
                                }
                            } 
                             */
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private static void RestartService(ServiceController sc, int timeoutMilliseconds)
        {
            int millisec1 = Environment.TickCount;
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

            sc.Stop();
            sc.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

            // count the rest of the timeout
            int millisec2 = Environment.TickCount;
            timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running, timeout);
        }
    }
}
