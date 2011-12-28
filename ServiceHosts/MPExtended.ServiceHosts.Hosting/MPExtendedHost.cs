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
using System.Text;
using MPExtended.Libraries.General;
using MPExtended.Services.MetaService;

namespace MPExtended.ServiceHosts.Hosting
{
    public class MPExtendedHost
    {
        private WCFHost wcf;
        private IServicePublisher publisher;
        private List<Type> serviceTypes;

        public MPExtendedHost()
        {
            System.Threading.Thread.CurrentThread.Name = "HostThread";
            wcf = new WCFHost();
            publisher = new ServicePublisher();
        }

        public MPExtendedHost(List<Type> types)
            : this()
        {
            serviceTypes = types;
            wcf.SetTypes(types);
        }

        public bool Open()
        {
            try
            {
                // rotate log files if possible
                LogRotation rotation = new LogRotation();
                rotation.Rotate();

                Log.Debug("Opening MPExtended ServiceHost version {0} (build {1})", VersionUtil.GetVersionName(), VersionUtil.GetBuildVersion());

                // always log uncaught exceptions
                AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
                {
                    Log.Error("Unhandled exception", (Exception)e.ExceptionObject);
                    if (e.IsTerminating)
                    {
                        Log.Fatal("Terminating because of previous exception");
                    }
                };

                // perform the actual start
                wcf.Start(Installation.GetInstalledServices().Where(x => x.HostAsWCF).ToList());
                publisher.PublishAsync();
                Log.Debug("Opened MPExtended ServiceHost");
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
                wcf.Stop();
                ThreadManager.AbortAll();
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
