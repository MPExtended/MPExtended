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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using MPExtended.Libraries.General;

namespace MPExtended.ServiceHosts.WebMediaPortal
{
    public partial class HostService : ServiceBase
    {
        private Process hostProcess;
        private Thread logThread;

        public HostService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
#if DEBUG
            string path = Path.Combine(Installation.GetSourceRootDirectory(), "Applications", "MPExtended.Applications.WebMediaPortal");
#else
            string path = Installation.GetInstallDirectory(MPExtendedProduct.WebMediaPortal);
#endif

            string iisExpress = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "IIS Express", "iisexpress.exe");
            string arguments = String.Format("/path:{0} /port:{1}", path, 8080);
            string logPath = Path.Combine(Installation.GetLogDirectory(), String.Format("WebMediaPortalIIS-{0:yyyy_MM_dd}.log", DateTime.Now));

            hostProcess = new Process();
            hostProcess.StartInfo = new ProcessStartInfo()
            {
                FileName = iisExpress,
                Arguments = "",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            hostProcess.Start();

            logThread = new Thread(delegate(object param)
            {
                Stream data = (Stream)param;
                using (StreamReader reader = new StreamReader(data))
                {
                    using (StreamWriter writer = new StreamWriter(logPath, true))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }

            });
            logThread.Start(hostProcess.StandardOutput);
        }

        protected override void OnStop()
        {
            hostProcess.Kill();
            logThread.Join();
        }
    }
}
