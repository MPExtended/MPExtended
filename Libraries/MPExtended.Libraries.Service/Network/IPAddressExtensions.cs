#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Web;
using System.Net;

namespace MPExtended.Libraries.Service.Network
{
    internal static class IPAddressExtensions
    {
        public static IPAddress GetNetworkAddress(this IPAddress address, byte[] subnetMaskBytes)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Subnet mask should have same size as IP address");
            byte[] broadcastAddress = new byte[ipAdressBytes.Length];

            for (int i = 0; i < broadcastAddress.Length; i++)
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & subnetMaskBytes[i]);
            return new IPAddress(broadcastAddress);
        }

        public static bool IsInSameSubnet(this IPAddress address, IPAddress check, byte[] subnetMask)
        {
            if (address.AddressFamily != check.AddressFamily)
                return false;

            IPAddress network1 = address.GetNetworkAddress(subnetMask);
            IPAddress network2 = check.GetNetworkAddress(subnetMask);
            return network1.IsEqual(network2);
        }

        public static bool IsInSameSubnet(this IPAddress address, IPAddress check, IPAddress subnetMask)
        {
            return IsInSameSubnet(address, check, subnetMask.GetAddressBytes());
        }
    }
}