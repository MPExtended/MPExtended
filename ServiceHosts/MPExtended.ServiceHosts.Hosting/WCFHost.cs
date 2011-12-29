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
                types[t.Name] = t;
            }
        }

        public void Start(List<Service> availableServices)
        {
            foreach (Service srv in availableServices)
            {
                try
                {
                    Log.Debug("Loading service {0}", srv.ImplementationName);
                    if (types.ContainsKey(srv.ImplementationName))
                    {
                        hosts.Add(new ServiceHost(types[srv.ImplementationName], BaseAddresses.GetForService(srv.ServiceName)));
                    }
                    else if (File.Exists(srv.AssemblyPath))
                    {
                        Log.Trace("Assembly path: {0}", srv.AssemblyPath);
                        Assembly asm = Assembly.LoadFrom(srv.AssemblyPath);
                        Type t = asm.GetType(srv.FullTypeName);
                        hosts.Add(new ServiceHost(t, BaseAddresses.GetForService(srv.ServiceName)));
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
                    // configure security if needed
                    if (Configuration.Services.AuthenticationEnabled)
                    {
                        foreach (var endpoint in host.Description.Endpoints)
                        {
                            // do not enable auth for stream endpoint
                            if (endpoint.Name == "StreamEndpoint" || endpoint.Name.StartsWith("Unauthorized"))
                            {
                                continue;
                            }

                            if (endpoint.Binding is BasicHttpBinding)
                            {
                                ((BasicHttpBinding)endpoint.Binding).Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                                ((BasicHttpBinding)endpoint.Binding).Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
                            }
                            else if (endpoint.Binding is WebHttpBinding)
                            {
                                ((WebHttpBinding)endpoint.Binding).Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
                                ((WebHttpBinding)endpoint.Binding).Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
                            }
                        }
                    }

                    // and open host
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
                // we have to indicate that it should hurry up with closing because it takes 10 seconds by default...
                host.Close(new TimeSpan(0, 0, 0, 0, 250));
            }
        }
    }
}