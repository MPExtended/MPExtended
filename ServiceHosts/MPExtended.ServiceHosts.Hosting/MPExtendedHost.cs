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
using System.Linq;
using System.Text;
using MPExtended.Libraries.General;

namespace MPExtended.ServiceHosts.Hosting
{
    public class MPExtendedHost
    {
        private WCFHost wcf;
        private Zeroconf zeroconf;
        private List<Type> serviceTypes;

        public MPExtendedHost()
        {
            System.Threading.Thread.CurrentThread.Name = "HostThread";
            wcf = new WCFHost();
            zeroconf = new Zeroconf();
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
                Log.Debug("Opening MPExtended ServiceHost version {0} (build {1})", VersionUtil.GetVersionName(), VersionUtil.GetBuildVersion());
                wcf.Start(Installation.GetInstalledServices().Where(x => x.HostAsWCF).ToList());
                ThreadManager.Start("Zeroconf", delegate()
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
