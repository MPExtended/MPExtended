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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using MPExtended.Libraries.General;

namespace MPExtended.Services.WindowsServiceHost
{
    public partial class CoreService : ServiceBase
    {
        private static List<Service> allServices = new List<Service>()
        {
            new Service("MPExtended.Services.MediaAccessService.MediaAccessService", "MPExtended.Services.MediaAccessService", Installation.IsMASInstalled),
            new Service("MPExtended.Services.TVAccessService.TVAccessService", "MPExtended.Services.TVAccessService", Installation.IsTASInstalled),
            new Service("MPExtended.Services.StreamingService.StreamingService", "MPExtended.Services.StreamingService", true),
            new Service("MPExtended.Services.UserSessionService.UserSessionProxyService", "MPExtended.Services.UserSessionService", true)
        };

        private List<ServiceHost> hosts;
        private Thread createHostsThread;

        public CoreService()
        {
            InitializeComponent();
            this.ServiceName = "MPExtended Service";
        }

        protected override void OnStart(string[] args)
        {
            // Create them in a background thread to allow debugging
            createHostsThread = new Thread(CreateHosts);
            createHostsThread.Start();
        }

        private void CreateHosts()
        {
            System.Threading.Thread.Sleep(20000); // FIXME: remove for release
            hosts = new List<ServiceHost>();
            Log.Debug("MPExtended.Services.WindowsServiceHost starting...");

            string ourDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Log.Trace("Using {0} as root directory", ourDirectory);

            foreach (Service srv in allServices.Where(x => x.IsActive))
            {
                try
                {
                    Log.Debug("Loading service {0}", srv.Name);
                    string path = Path.Combine(ourDirectory, srv.Assembly + ".dll");
                    if (File.Exists(path))
                    {
                        Assembly asm = Assembly.LoadFrom(path);
                        Type t = asm.GetType(srv.Name);
                        hosts.Add(new ServiceHost(t));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to add service", ex);
                }
            }

            foreach (var host in hosts)
            {
                try
                {
                    host.Open();
                }
                catch (Exception ex)
                {
                    Log.Error(String.Format("Failed to open host {0}", host.Description.ServiceType.Name), ex);
                }
            }

            Log.Info("MPExtended.Services.WindowsServiceHost started...");
        }

        protected override void OnStop()
        {
            foreach (var host in hosts)
            {
                host.Close();
            }
        }
    }
}
