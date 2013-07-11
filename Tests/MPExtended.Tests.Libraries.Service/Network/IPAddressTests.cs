#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://mpextended.github.com/
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

using MPExtended.Libraries.Service.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Xunit;

namespace MPExtended.Tests.Libraries.Service.Network
{
    public class IPAddressTests
    {
        private byte[] CreateV6Mask(int rightMaskedBitCount)
        {
            byte[] mask = new byte[16] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            for(int i = 0; i < rightMaskedBitCount; i++)
                mask[(127 - i) / 8] ^= (byte)Math.Pow(2, i % 8);

            return mask;
        }

        [Fact]
        public void ExtensionMethods()
        {
            var addressA =  IPAddress.Parse("10.11.12.13");
            var addressB =  IPAddress.Parse("10.11.11.11");
            var addressC =  IPAddress.Parse("192.168.0.1");
            var address6A = IPAddress.Parse("2001:0DB8:AC10:FE01:0123:0123:0123:0123");
            var address6B = IPAddress.Parse("2001:0DB8:AC10:FE01:0123:0123:1234:5678");
            var address6C = IPAddress.Parse("1730:0BF8:AC10:CDFA:BEEF:BEEF:1234:5678");

            Assert.Equal(addressA.GetNetworkAddress(new byte[] { 255, 255, 255, 0 }), new IPAddress(new byte[] { 10, 11, 12, 0 }));
            Assert.Equal(addressB.GetNetworkAddress(new byte[] { 255, 255, 254, 0 }), new IPAddress(new byte[] { 10, 11, 10, 0 }));
            Assert.Equal(addressB.GetNetworkAddress(new byte[] { 255, 128, 0, 0 }),   new IPAddress(new byte[] { 10, 0, 0, 0 }));
            Assert.Equal(address6A.GetNetworkAddress(CreateV6Mask(64)), IPAddress.Parse("2001:0DB8:AC10:FE01::"));
            Assert.Equal(address6A.GetNetworkAddress(CreateV6Mask(0)), IPAddress.Parse("2001:0DB8:AC10:FE01:0123:0123:0123:0123"));
            Assert.Throws<ArgumentException>(() => addressA.GetNetworkAddress(CreateV6Mask(64)));

            Assert.True(addressA.IsInSameSubnet(addressB, new byte[] { 255, 255, 0, 0 }));
            Assert.False(addressA.IsInSameSubnet(addressB, new byte[] { 255, 255, 255, 0 }));
            Assert.False(addressA.IsInSameSubnet(addressC, new byte[] { 255, 0, 0, 0 }));
            Assert.True(addressA.IsInSameSubnet(addressB, IPAddress.Parse("255.255.0.0")));
            Assert.True(addressA.IsInSameSubnet(addressB, IPAddress.Parse("255.255.248.0")));
            Assert.False(addressA.IsInSameSubnet(addressC, IPAddress.Parse("255.255.248.0")));
            Assert.False(addressC.IsInSameSubnet(addressB, IPAddress.Parse("255.0.0.0")));
            Assert.True(address6A.IsInSameSubnet(address6B, CreateV6Mask(64)));
            Assert.True(address6A.IsInSameSubnet(address6B, CreateV6Mask(32)));
            Assert.False(address6A.IsInSameSubnet(address6B, CreateV6Mask(16)));
            Assert.False(address6B.IsInSameSubnet(address6C, CreateV6Mask(64)));
            Assert.False(address6A.IsInSameSubnet(addressA, new byte[] { 255, 255, 255, 0 }));
            Assert.Throws<ArgumentException>(() => addressA.IsInSameSubnet(addressB, CreateV6Mask(64)));
            Assert.Throws<ArgumentException>(() => address6A.IsInSameSubnet(address6B, new byte[] { 255, 255, 0, 0 }));
        }
    }
}
