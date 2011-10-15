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
using System.Net.NetworkInformation;

namespace MPExtended.ServiceHosts.Hosting
{
    internal class BaseAddresses
    {
        public static Uri[] GetForService(string serviceName)
        {
            List<Uri> ret = new List<Uri>() { };

            // HTTP binding: pick the first non-local address to make sure there is also an valid IP in the SOAP messages.
            // We're restricted to just one IP by the WCF hosting, can't do anything about that. Shipping IIS is a bit too much. 
            IEnumerable<string> nonLocalAddresses = GetIpAddresses().Where(x => x != "127.0.0.1" && x != "localhost");
            string addr;
            if (nonLocalAddresses.Count() > 0)
            {
                addr = nonLocalAddresses.First();
            }
            else
            {
                addr = "127.0.0.1";
            }
            ret.Add(new Uri(String.Format("http://{0}:{1}/MPExtended/{2}", addr, ServiceHostConfig.Port, serviceName)));

            // local net.pipe binding
            ret.Add(new Uri(String.Format("net.pipe://127.0.0.1/MPExtended/{0}", serviceName)));

            return ret.ToArray();
        }

        private static string[] GetIpAddresses()
        {
            var addresses = 
                NetworkInterface.GetAllNetworkInterfaces()
                    .Where(x => x.OperationalStatus == OperationalStatus.Up)
                    .Select(x => x.GetIPProperties())
                    .Select(x => x.UnicastAddresses)
                    .SelectMany(x => x);

            bool enableIPv6 = ServiceHostConfig.IPv6Enabled;
            if (!enableIPv6)
            {
                addresses = addresses.Where(x => x.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6);
            }

            return addresses.Select(x => x.Address.ToString()).ToArray();
        }
    }
}
