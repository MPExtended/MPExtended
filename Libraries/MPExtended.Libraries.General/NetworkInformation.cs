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
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace MPExtended.Libraries.General
{
    public static class NetworkInformation
    {
        public static IEnumerable<IPAddress> GetIPAddressList(bool enableIPv6)
        {
            return Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .Where(x => enableIPv6 || x.AddressFamily != AddressFamily.InterNetworkV6)
                .Distinct()
                .ToArray();
        }

        public static IEnumerable<IPAddress> GetIPAddressList()
        {
            return GetIPAddressList(Configuration.Services.EnableIPv6);
        }

        public static string[] GetIPAddresses(bool enableIPv6)
        {
            return GetIPAddressList(enableIPv6).Select(x => x.ToString()).ToArray();
        }

        public static string[] GetIPAddresses()
        {
            return GetIPAddresses(Configuration.Services.EnableIPv6);
        }

        public static string[] GetMACAddresses()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.OperationalStatus == OperationalStatus.Up)
                .Where(x => x.GetPhysicalAddress() != null)
                .Select(x => x.GetPhysicalAddress().ToString())
                .Where(x => x.Length == 12)
                .Distinct()
                .ToArray();
        }

        public static bool IsOnLAN(IPAddress address)
        {
            if (address.IsLocal())
            {
                return true;
            }

            var info = NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.OperationalStatus == OperationalStatus.Up && x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                .Where(x => x.IPv4Mask != null)
                .First();

            return address.IsInSameSubnet(info.Address, info.IPv4Mask);
        }

        public static IPAddress NormalizeAddress(string address)
        {
            if (address == "localhost")
            {
                address = "127.0.0.1";
            }
            IPAddress addr = IPAddress.Parse(address);

            // let's try if we can lookup the ipv4 address
            if (addr.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var list = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(x => x.OperationalStatus == OperationalStatus.Up)
                    .Select(x => x.GetIPProperties().UnicastAddresses)
                    .Where(x => x.Any(y => y.Address.IsEqual(addr)) && x.Any(y => y.Address.AddressFamily == AddressFamily.InterNetwork));
                if (list.Any())
                {
                    addr = list.First().First(x => x.Address.AddressFamily == AddressFamily.InterNetwork).Address;
                }
            }

            return addr;
        }

        public static Dictionary<string, string> GetNetworkInterfaces(bool enableIPv6)
        {
            Dictionary<string, string> ifaces = new Dictionary<string, string>();
            foreach (var iface in NetworkInterface.GetAllNetworkInterfaces().Where(x => x.OperationalStatus == OperationalStatus.Up))
            {
                foreach (IPAddressInformation unicast in iface.GetIPProperties().UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork ||
                       (unicast.Address.AddressFamily == AddressFamily.InterNetworkV6 && enableIPv6))
                    {
                        ifaces.Add(iface.Name, unicast.Address.ToString());
                    }
                }
            }

            return ifaces;
        }

        public static Dictionary<string, string> GetNetworkInterfaces()
        {
            return GetNetworkInterfaces(Configuration.Services.EnableIPv6);
        }
    }
}
