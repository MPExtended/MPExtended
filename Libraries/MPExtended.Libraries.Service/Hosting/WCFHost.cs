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
using System.Threading.Tasks;

namespace MPExtended.Libraries.Service.Hosting
{
    internal class WCFHost
    {
        private List<ServiceHost> hosts = new List<ServiceHost>();
        private Dictionary<string, Type> types = new Dictionary<string, Type>();

        public void Start(IEnumerable<ServiceAssemblyAttribute> availableServices)
        {
            // This part is quite tricky to force execution in parallel:
            // - Do not use LINQ, it won't work in parallel
            // - Do not close over the loop variable, it'll try to load the last service 4 times:
            //   http://blogs.msdn.com/b/ericlippert/archive/2009/11/12/closing-over-the-loop-variable-considered-harmful.aspx
            // - Force the TPL to run the tasks in parallel with TaskCreationOptions.LongRunning
            var tasks = new List<Task<ServiceHost>>();
            foreach (var srv in availableServices)
            {
                tasks.Add(Task<ServiceHost>.Factory.StartNew(CreateHost, (object)srv, TaskCreationOptions.LongRunning));
            }

            // Finally open all the hosts
            foreach (var srv in tasks)
            {
                var host = srv.Result;
                try
                {
                    // open host if possible
                    if (host != null)
                    {
                        host.Open();
                        hosts.Add(host);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(String.Format("Failed to open host {0}", host.Description.ServiceType.Name), ex);
                }
            }

            Log.Debug("WCF services have been started");
        }

        private ServiceHost CreateHost(object srv)
        {
            return CreateHost((ServiceAssemblyAttribute)srv);
        }

        private ServiceHost CreateHost(ServiceAssemblyAttribute srv)
        {
            try
            {
                Log.Debug("Loading service {0}", srv.WCFType.Name);
                System.Threading.Thread.Sleep(2500);
                var host = new ServiceHost(srv.WCFType, BaseAddresses.GetForService(srv.Service));

                // configure security if needed
                if (Configuration.Services.AuthenticationEnabled)
                {
                    foreach (var endpoint in host.Description.Endpoints)
                    {
                        // do not enable auth for stream or unauthorized endpoints
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

                return host;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to load service {0}", srv.WCFType.Name), ex);
                return null;
            }
        }

        public void Stop()
        {
            foreach (var host in hosts)
            {
                try
                {
                    // we have to indicate that it should hurry up with closing because it takes 10 seconds by default...
                    host.Close(TimeSpan.FromMilliseconds(500));
                }
                catch (Exception ex)
                {
                    Log.Error(String.Format("Failed to close ServiceHost for {0}", host.Description.ServiceType.Name), ex);
                }
            }
        }
    }
}