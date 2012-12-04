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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Composition;
using MPExtended.Libraries.Service.Internal;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service.Hosting
{
    public class MPExtendedHost
    {
        private const string STARTUP_CONDITION = "MPExtendedHost";

        private WCFHost wcfHost;

        public MPExtendedHost()
        {
            Thread.CurrentThread.Name = "HostThread";
            wcfHost = new WCFHost();
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

                // set the thread locale to English; we don't output any user-facing messages from the service anyway. 
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                // TODO: Set CultureInfo.DefaultThreadCurrent{,UI}Culture when we switch to .NET4.5

                // start watching the configuration files for changes
                Configuration.Load();
                Configuration.EnableChangeWatching();

                // load and host all services
                foreach (var service in ServiceInstallation.Instance.GetServices())
                    Task.Factory.StartNew(CreateServiceHost, (object)service);
                wcfHost = new WCFHost();
                wcfHost.Start(ServiceInstallation.Instance.GetWcfServices());
				
				// ensure a service dependency on the TVEngine is set
                Task.Factory.StartNew(() => TVEDependencyInstaller.EnsureDependencyStatus(TVEDependencyInstaller.DependencyStatus.NoDependencySet));

                // log system version details
                Mediaportal.LogVersionDetails();
                Log.Debug("Running on CLR {0} on {1}", Environment.Version, Environment.OSVersion);

                // finish
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

        private void CreateServiceHost(object passedService)
        {
            var plugin = (Plugin<IService>)passedService;
            Log.Debug("Loading service {0}", plugin.Metadata["ServiceName"]);

            try
            {
                plugin.Value.Start();
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to start service {0}", plugin.Metadata["ServiceName"]), ex);
            }
        }

        public bool Close()
        {
            try
            {
                Log.Debug("Closing MPExtended ServiceHost...");
                ServiceState.TriggerStoppingEvent();
                wcfHost.Stop();
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
