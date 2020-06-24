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
using System.Text;
using MPExtended.Libraries.Service.WCF;
using Xunit;

namespace MPExtended.Tests.Libraries.Service.WCF
{
    public class CustomQueryStringConverterTests
    {
        private CustomQueryStringConverter converter;

        public CustomQueryStringConverterTests()
        {
            converter = new CustomQueryStringConverter();
        }

        [Fact]
        public void TestCustomFeatures()
        {
            Assert.True(converter.CanConvert(typeof(int?)));
            Assert.True(converter.CanConvert(typeof(string)));
            Assert.True(converter.CanConvert(typeof(DateTime)));

            Assert.Null(converter.ConvertStringToValue(null, typeof(int?)));
            Assert.Null(converter.ConvertStringToValue("", typeof(int?)));
            Assert.Null(converter.ConvertStringToValue(" \t ", typeof(int?)));

            Assert.Empty(converter.ConvertValueToString(null, typeof(int?)));
            Assert.Equal("5", converter.ConvertValueToString(5, typeof(int?)));

            Assert.Equal(new DateTime(2012, 8, 7, 15, 00, 00, DateTimeKind.Utc).ToLocalTime(), converter.ConvertStringToValue("2012-08-07T15:00:00Z", typeof(DateTime)));
            Assert.Equal(new DateTime(2012, 8, 7, 0, 0, 0, DateTimeKind.Local), converter.ConvertStringToValue("2012-08-07", typeof(DateTime)));
            // TODO: test for timezone magic here and maybe for Daylight
            Assert.Equal(new DateTime(2012, 1, 17, 14, 52, 29, DateTimeKind.Utc).ToLocalTime(), converter.ConvertStringToValue("/Date(1326815549000-1000)/", typeof(DateTime)));
        }
    }
}
