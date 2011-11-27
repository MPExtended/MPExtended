#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using MPExtended.Libraries.General;

namespace MPExtended.ServiceHosts.WebMediaPortal
{
    internal class IISExpressHost
    {
        private Process hostProcess;
        private Thread logThread;
        private string tempConfigFile;

        public void Start()
        {
            Log.Debug("MPExtended.ServiceHosts.WebMediaPortal starting...");

            try
            {
                // generate config file
                var generator = new IISConfigGenerator();
#if DEBUG
                generator.PhysicalSitePath = Path.Combine(Installation.GetSourceRootDirectory(), "Applications", "MPExtended.Applications.WebMediaPortal");
#else
                generator.PhysicalSitePath = Path.Combine(Installation.GetInstallDirectory(MPExtendedProduct.WebMediaPortal), "www");
#endif
                generator.HostAddresses = HostConfiguration.HostAddresses;
                generator.Port = HostConfiguration.Port;
                generator.TemplatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "IISExpressTemplate.config");
                tempConfigFile = Path.GetTempFileName();
                generator.GenerateConfigFile(tempConfigFile);
                Log.Debug("Saved IIS Express configuration file to {0}", tempConfigFile);

                // log configuration
                foreach (var addr in generator.HostAddresses)
                {
                    Log.Debug("- Listening on {0}:{1}", addr, generator.Port);
                }

                // start IIS Express
                string iisExpress = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "IIS Express", "iisexpress.exe");
                string arguments = String.Format("/systray:0 /config:{0} /site:WebMediaPortal", tempConfigFile);
                string logPath = Path.Combine(Installation.GetLogDirectory(), String.Format("WebMediaPortalIIS-{0:yyyy_MM_dd}.log", DateTime.Now));

                hostProcess = new Process();
                hostProcess.StartInfo = new ProcessStartInfo()
                {
                    FileName = iisExpress,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                hostProcess.Start();
                Log.Info("Started IIS Express!");

                // read output from IIS Express
                logThread = new Thread(delegate(object param)
                {
                    using (StreamReader reader = (StreamReader)param)
                    {
                        using (StreamWriter writer = new StreamWriter(logPath, true, Encoding.UTF8, 16 * 1024))
                        {
                            writer.WriteLine("<process started at {0:yyyy-MM-dd HH:mm:ss} with arguments {1}>", DateTime.Now, arguments);
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                writer.WriteLine("[{0:yyyy-MM-dd HH:mm:ss}] {1}", DateTime.Now, line);
                            }
                            writer.WriteLine("<process exited at {0:yyyy-MM-dd HH:mm:ss}>", DateTime.Now);
                        }
                    }

                });
                logThread.Start(hostProcess.StandardOutput);
            }
            catch (Exception ex)
            {
                Log.Fatal("Failed to start IIS Express", ex);
            }
        }

        public void Stop()
        {
            try
            {
                if(!hostProcess.HasExited)
                    hostProcess.Kill();
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to kill IIS Express", ex);
            }

            try
            {
                logThread.Join(TimeSpan.FromSeconds(5));
                if (logThread.IsAlive)
                    logThread.Abort();
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to abort log thread", ex);
            }

            try
            {
                File.Delete(tempConfigFile);
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to delete temporary config file", ex);
            }

            Log.Info("Stopped IIS Express");
        }
    }
}
