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
            // TODO: test for timezone magic here
            Assert.Equal(new DateTime(2012, 1, 17, 14, 52, 29, DateTimeKind.Utc).ToLocalTime(), converter.ConvertStringToValue("/Date(1326815549000+0000)/", typeof(DateTime)));
        }
    }
}
