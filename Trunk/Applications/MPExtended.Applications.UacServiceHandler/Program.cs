using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Reflection;

namespace MPExtended.Applications.UacServiceHandler
{
    static class Program
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
