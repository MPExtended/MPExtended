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
using System.IO;
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service;
using Xunit;

namespace MPExtended.Tests.Libraries.Service.Config.Tests
{
    public class Features : ConfigurationTestEnvironment
    {
        public Features()
            : base()
        {
            AddDefaultConfigurationFile("Authentication.xml", "ParsingTests/Authentication.xml");
        }

        [Fact]
        public void DefaultPort()
        {
            Assert.Equal(4322, Configuration.DEFAULT_PORT);
        }

        [Fact]
        public void UseDefaultFiles()
        {
            Assert.False(File.Exists(Path.Combine(Installation.Properties.ConfigurationDirectory, "Authentication.xml")));
            Assert.True(File.Exists(Path.Combine(Installation.Properties.DefaultConfigurationDirectory, "Authentication.xml")));

            // Load the configuration in a way that won't be optimized away by the compiler
            Configuration.Authentication.GetHashCode();

            Assert.True(File.Exists(Path.Combine(Installation.Properties.ConfigurationDirectory, "Authentication.xml")));
        }
    }
}
