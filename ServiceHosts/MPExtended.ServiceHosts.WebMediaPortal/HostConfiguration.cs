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
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using MPExtended.Libraries.General; 

namespace MPExtended.ServiceHosts.WebMediaPortal
{
    internal static class HostConfiguration
    {
        public static int Port { get { return 8080; } }

        public static string[] HostAddresses
        {
            get
            {
                // If you didn't get it, I like LINQ
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Select(x => x.GetIPProperties())
                    .SelectMany(x => x.UnicastAddresses)
                    .Select(x => x.Address)
                    .Where(x => x.AddressFamily == AddressFamily.InterNetwork ||
                        (x.AddressFamily == AddressFamily.InterNetworkV6 && false)) // can't rely on Configuration.Services here due to sometimes uninstalled services
                    .SelectMany(x =>
                    {
                        try
                        {
                            var entry = Dns.GetHostEntry(x);
                            return entry.Aliases.Concat(new string[] { entry.HostName, x.ToString() });
                        }
                        catch (Exception)
                        {
                            return new string[] { x.ToString() };
                        }
                    })
                    .Concat(new string[] { "localhost" })
                    .Distinct()
                    .ToArray();
            }
        }
    }
}
