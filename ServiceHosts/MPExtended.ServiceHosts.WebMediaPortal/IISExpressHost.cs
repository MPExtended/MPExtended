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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;

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
                // generate IIS Express config file
                var generator = new IISConfigGenerator();
                
                generator.PhysicalSitePath = Installation.GetFileLayoutType() == FileLayoutType.Source ?
                    Path.Combine(Installation.GetSourceRootDirectory(), "Applications", "MPExtended.Applications.WebMediaPortal") :
                    Path.Combine(Installation.GetInstallDirectory(MPExtendedProduct.WebMediaPortal), "www");

                generator.TemplatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "IISExpressTemplate.config");
                tempConfigFile = Path.GetTempFileName();
                generator.GenerateConfigFile(tempConfigFile);
                Log.Debug("Saved IIS Express configuration file to {0}", tempConfigFile);

                // lookup IIS Express location
                string iisExpress = null;
                object iisExpressLocation = RegistryReader.ReadKeyAllViews(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\IISExpress\7.5", "InstallPath");
                string iisExpressDefault = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "IIS Express", "iisexpress.exe");
                if (iisExpressLocation != null && File.Exists(Path.Combine(iisExpressLocation.ToString(), "iisexpress.exe")))
                {
                    iisExpress = Path.Combine(iisExpressLocation.ToString(), "iisexpress.exe");
                    Log.Debug("Using IIS Express installed at {0}", iisExpress);
                }
                else if (File.Exists(iisExpressDefault))
                {
                    Log.Debug("Using IIS Express at default location {0}", iisExpressDefault);
                } 
                else
                {
                    Log.Fatal("IIS Express not found");
                    return;
                }

                // rotate IIS Express logfile if it's too big
                string logPath = Path.Combine(Installation.GetLogDirectory(), String.Format("WebMediaPortalIIS.log", DateTime.Now));
                if (File.Exists(logPath) && new FileInfo(logPath).Length > 1024 * 1024)
                {
                    string backup = Path.ChangeExtension(logPath, ".bak");
                    if (File.Exists(backup))
                    {
                        File.Delete(backup);
                    }
                    File.Move(logPath, backup);
                }

                // start IIS Express
                string arguments = String.Format("/systray:0 /config:{0} /site:WebMediaPortal", tempConfigFile);

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
                        Stream file = File.Open(logPath, FileMode.Append, FileAccess.Write, FileShare.Read);
                        using (StreamWriter writer = new StreamWriter(file, Encoding.UTF8, 16 * 1024))
                        {
                            writer.WriteLine("<process started at {0:yyyy-MM-dd HH:mm:ss} with arguments {1}>", DateTime.Now, arguments);
                            string line;
                            long i = 0;
                            while ((line = reader.ReadLine()) != null)
                            {
                                writer.WriteLine("[{0:yyyy-MM-dd HH:mm:ss}] {1}", DateTime.Now, line);
                                if (i++ % 10 == 0)
                                    writer.Flush();
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
