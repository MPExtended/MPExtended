#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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

namespace MPExtended.Libraries.Service.Network
{
    public static class NetworkInformation
    {
        private static string[] _localhostNames = new string[] { "127.0.0.1", "::1", "localhost" };
        private static IPAddress _ipAddress;

        public static string GetIPAddressForUri()
        {
            if (_ipAddress == null)
                _ipAddress = LoadIPAddress();
            return _ipAddress.AddressFamily == AddressFamily.InterNetworkV6 ? String.Format("[{0}]", _ipAddress) : _ipAddress.ToString();
        }

        private static IPAddress LoadIPAddress()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                                             .Where(x => x.OperationalStatus == OperationalStatus.Up);

            Func<IEnumerable<NetworkInterface>, IEnumerable<IPAddress>> getAddresses = interfaceList =>
                interfaceList
                    .SelectMany(x => x.GetIPProperties().UnicastAddresses.Select(a => a.Address))
                    .Where(x => x.AddressFamily == AddressFamily.InterNetwork || x.AddressFamily == AddressFamily.InterNetworkV6)
                    .Where(x => !x.IsIPv6LinkLocal && !x.IsIPv6Multicast)
                    .OrderBy(x => x.AddressFamily != AddressFamily.InterNetwork); // Prefer IPv4 until we've found a reliable way to determine working IPv6
            
            // Even though the docs say that NetworkIntereface.Type only returns a subset of these types, I've seen some others
            // (for example Tunnel) in the wild, so let's just list all acceptable types.
            var preferedTypes = new NetworkInterfaceType[] {
                NetworkInterfaceType.Ethernet,
                NetworkInterfaceType.FastEthernetFx,
                NetworkInterfaceType.FastEthernetT,
                NetworkInterfaceType.GigabitEthernet,
            };
            var lanAddresses = getAddresses(interfaces.Where(x => preferedTypes.Contains(x.NetworkInterfaceType)));
            if (lanAddresses.Any())
                return lanAddresses.First();

            var addresses = getAddresses(interfaces);
            if (addresses.Any(x => !_localhostNames.Contains(x.ToString())))
                return addresses.First(x => !_localhostNames.Contains(x.ToString()));
            if (addresses.Any())
                return addresses.First();

            return IPAddress.Parse("127.0.0.1");
        }

        private static IEnumerable<IPAddress> GetIPAddressList()
        {
            return Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .Where(x => x.AddressFamily == AddressFamily.InterNetwork || x.AddressFamily == AddressFamily.InterNetworkV6)
                .Where(x => !x.IsIPv6LinkLocal && !x.IsIPv6Multicast)
                .Distinct()
                .ToArray();
        }

        public static IEnumerable<string> GetIPAddresses()
        {
            return GetIPAddressList().Select(x => x.ToString()).ToList();
        }

        public static IEnumerable<string> GetMACAddresses()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.OperationalStatus == OperationalStatus.Up)
                .Where(x => x.GetPhysicalAddress() != null)
                .Select(x => x.GetPhysicalAddress().ToString())
                .Where(x => x.Length == 12)
                .Distinct()
                .ToList();
        }

        public static bool IsOnLAN(IPAddress address)
        {
            if (IsLocalAddress(address))
                return true;

            var systemAddresses = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(x => x.OperationalStatus == OperationalStatus.Up && x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .SelectMany(x => x.GetIPProperties().UnicastAddresses);

            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                return systemAddresses
                    .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Where(x => x.IPv4Mask != null)
                    .Any(x => address.IsInSameSubnet(x.Address, x.IPv4Mask.GetAddressBytes()));
            }
            else
            {
                // TODO: Get the current IPv6 subnet mask from the .NET framework instead of hardcoding it to the first 64-bits
                byte[] subnetMask = new byte[16] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                return systemAddresses
                    .Where(x => x.Address.AddressFamily == AddressFamily.InterNetworkV6)
                    .Any(x => address.IsInSameSubnet(x.Address, subnetMask));
            }
        }

        public static bool IsOnLAN(string address)
        {
            if (IsLocalAddress(address))
                return true;

            return IsOnLAN(IPAddress.Parse(address));
        }

        public static bool IsLocalAddress(IPAddress address)
        {
            return _localhostNames.Contains(address.ToString()) || GetIPAddressList().Contains(address);
        }

        public static bool IsLocalAddress(string address)
        {
            if (_localhostNames.Contains(address))
                return true;

            try
            {
                bool isLocal = Dns.GetHostAddresses(address)
                    .Where(x => x.AddressFamily == AddressFamily.InterNetwork || x.AddressFamily == AddressFamily.InterNetworkV6)
                    .Any(x => IsLocalAddress(x));
                if (isLocal)
                    return true;
            }
            catch (SocketException)
            {
                // This can happen for unknown DNS names and things like that, assume they're not local
                return false;
            }

            IPAddress ipAddr;
            if(IPAddress.TryParse(address, out ipAddr))
                return IsLocalAddress(ipAddr);
            return false;
        }

        public static Dictionary<string, string> GetNetworkInterfaces()
        {
            Dictionary<string, string> ifaces = new Dictionary<string, string>();
            foreach (var iface in NetworkInterface.GetAllNetworkInterfaces().Where(x => x.OperationalStatus == OperationalStatus.Up))
            {
                foreach (IPAddressInformation unicast in iface.GetIPProperties().UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork || unicast.Address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        ifaces.Add(iface.Name, unicast.Address.ToString());
                    }
                }
            }

            return ifaces;
        }

        public static bool IsValid(IPAddress address)
        {
            return address.AddressFamily == AddressFamily.InterNetwork || address.AddressFamily == AddressFamily.InterNetworkV6;
        }
    }
}
