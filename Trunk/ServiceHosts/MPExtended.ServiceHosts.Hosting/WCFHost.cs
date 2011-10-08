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
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.IO;
using System.Threading;
using System.Reflection;
using MPExtended.Libraries.General;

namespace MPExtended.ServiceHosts.Hosting
{
    internal class WCFHost
    {
        private List<ServiceHost> hosts = new List<ServiceHost>();
        private Dictionary<string, Type> types = new Dictionary<string, Type>();

        public void SetTypes(List<Type> passedList) 
        {
            foreach (Type t in passedList)
            {
                types[t.FullName] = t;
            }
        }

        public void Start(List<Service> availableServices)
        {
            foreach (Service srv in availableServices)
            {
                try
                {
                    Log.Debug("Loading service {0}", srv.Name);
                    if (types.ContainsKey(srv.Name))
                    {
                        hosts.Add(new ServiceHost(types[srv.Name]));
                    }
                    else if (File.Exists(srv.AssemblyPath))
                    {
                        Assembly asm = Assembly.LoadFrom(srv.AssemblyPath);
                        Type t = asm.GetType(srv.Name);
                        hosts.Add(new ServiceHost(t));
                    }
                    else
                    {
                        Log.Debug("Service not installed");
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
        }

        public void Stop()
        {
            foreach (var host in hosts)
            {
                host.Close();
            }
        }
    }
}