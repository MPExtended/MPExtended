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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Internal;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service.Hosting
{
    public class MPExtendedHost
    {
        private const string STARTUP_CONDITION = "MPExtendedHost";

        private WCFHost wcf;
        private Zeroconf zeroconf;

        public MPExtendedHost()
        {
            Thread.CurrentThread.Name = "HostThread";
            wcf = new WCFHost();
            zeroconf = new Zeroconf();
        }

        public bool Open()
        {
            try
            {
                ServiceState.RegisterStartupCondition(STARTUP_CONDITION);
                Log.Debug("Opening MPExtended ServiceHost version {0}", VersionUtil.GetFullVersionString());

                // always log uncaught exceptions that cause the program to exit
                AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
                {
                    Log.Error("Unhandled exception", (Exception)e.ExceptionObject);
                    if (e.IsTerminating)
                    {
                        Log.Fatal("Terminating because of previous exception");
                    }
                };

                // start watching the configuration files for changes
                Configuration.Load();
                Configuration.EnableChangeWatching();

                // start the WCF services
                wcf.Start(Installation.GetAvailableServices().Where(x => x.WCFType != null));

                // start the background threads
                var services = Installation.GetAvailableServices().Where(x => x.InitClass != null && x.InitMethod != null);
                foreach (var service in services)
                {
                    string name = service.Service.ToString() + "Background";
                    BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod;
                    service.InitClass.InvokeMember(service.InitMethod, flags, null, null, null);
                }

                // ensure a service dependency on the TVEngine is set
                Task.Factory.StartNew(TVEDependencyInstaller.EnsureDependencyIsInstalled);

                // do the zeroconf publish
                Task.Factory.StartNew(delegate()
                {
                    try
                    {
                        zeroconf.PublishServices(Installation.GetInstalledServices());
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Zeroconf publish failed", ex);
                    }
                });

                // log MP version details
                Mediaportal.LogVersionDetails();

                // finally finish the startup
                ServiceState.StartupConditionCompleted(STARTUP_CONDITION);
                Log.Trace("Opened MPExtended ServiceHost");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to open MPExtended ServiceHost", ex);
                return false;
            }
        }

        public bool Close()
        {
            try
            {
                Log.Debug("Closing MPExtended ServiceHost...");
                ServiceState.TriggerStoppingEvent();
                wcf.Stop();
                Log.Flush();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to close MPExtended ServiceHost", ex);
                return false;
            }
        }
    }
}
