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
using Xunit;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Config;

namespace MPExtended.Tests.Libraries.Service.Config.Tests
{
    public class Upgrading : ConfigurationTestEnvironment
    {
        public Upgrading()
            : base()
        {
            AddConfigurationFile("MediaAccess.xml", "UpgradingTests/MediaAccess.xml");
            AddConfigurationFile("Services.xml", "UpgradingTests/Services.xml");
            AddConfigurationFile("Streaming.xml", "UpgradingTests/Streaming.xml");
            AddConfigurationFile("WebMediaPortalHosting.xml", "UpgradingTests/WebMediaPortalHosting.xml");
        }

        [Fact]
        public void MediaAccess()
        {
            Assert.Equal("TVDefault", Configuration.Media.DefaultPlugins.TVShow);
            Assert.Equal("MovieDefault", Configuration.Media.DefaultPlugins.Movie);
            Assert.Equal("MusicDefault", Configuration.Media.DefaultPlugins.Music);
            Assert.Equal("PicsDefault", Configuration.Media.DefaultPlugins.Picture);
            Assert.Equal("FSDefault", Configuration.Media.DefaultPlugins.Filesystem);

            Assert.Equal(2, Configuration.Media.PluginConfiguration.Count);
            Assert.Equal(2, Configuration.Media.PluginConfiguration["PluginA"].Count);

            Assert.Equal("key", Configuration.Media.PluginConfiguration["PluginA"][0].Name);
            Assert.Equal(ConfigType.File, Configuration.Media.PluginConfiguration["PluginA"][0].Type);
            Assert.Equal("dp", Configuration.Media.PluginConfiguration["PluginA"][0].DisplayName);
            Assert.Equal("value", Configuration.Media.PluginConfiguration["PluginA"][0].Value);

            Assert.Equal("setting", Configuration.Media.PluginConfiguration["PluginA"][1].Name);
            Assert.Equal(ConfigType.Folder, Configuration.Media.PluginConfiguration["PluginA"][1].Type);
            Assert.Equal("dp2", Configuration.Media.PluginConfiguration["PluginA"][1].DisplayName);
            Assert.Equal("another value", Configuration.Media.PluginConfiguration["PluginA"][1].Value);

            Assert.Equal("key", Configuration.Media.PluginConfiguration["PluginB"][0].Name);
            Assert.Equal(ConfigType.File, Configuration.Media.PluginConfiguration["PluginB"][0].Type);
            Assert.Equal("xy", Configuration.Media.PluginConfiguration["PluginB"][0].DisplayName);
            Assert.Equal("more tests", Configuration.Media.PluginConfiguration["PluginB"][0].Value);
        }

        [Fact]
        public void Authentication()
        {
            Assert.True(Configuration.Authentication.Enabled);
            Assert.Equal(2, Configuration.Authentication.Users.Count);

            Assert.Equal("admin", Configuration.Authentication.Users[0].Username);
            Assert.True(Configuration.Authentication.Users[0].ValidatePassword("admin"));

            Assert.Equal("abc", Configuration.Authentication.Users[1].Username);
            Assert.True(Configuration.Authentication.Users[1].ValidatePassword("abc"));
        }

        [Fact]
        public void Services()
        {
            Assert.False(Configuration.Services.NetworkImpersonation.IsEnabled());
            Assert.Empty(Configuration.Services.NetworkImpersonation.Domain);
            Assert.Empty(Configuration.Services.NetworkImpersonation.Username);
            Assert.Empty(Configuration.Services.NetworkImpersonation.GetPassword());

            Assert.True(Configuration.Services.BonjourEnabled);
            Assert.Equal("PC Name", Configuration.Services.BonjourName);
            Assert.Equal(8888, Configuration.Services.Port);
            Assert.False(Configuration.Services.EnableIPv6);
        }

        [Fact]
        public void Streaming()
        {
            Assert.False(Configuration.Streaming.WatchSharing.DebugEnabled);
            Assert.False(Configuration.Streaming.WatchSharing.FollwitEnabled);
            Assert.True(Configuration.Streaming.WatchSharing.TraktEnabled);
            Assert.Equal("james", Configuration.Streaming.WatchSharing.TraktConfiguration["username"]);
            Assert.Equal("a94a8fe5ccb19ba61c4c0873d391e987982fbbd3", Configuration.Streaming.WatchSharing.TraktConfiguration["passwordHash"]);

            Assert.Equal(@"C:\Team MediaPortal\MediaPortal\thumbs\tv\logos", Configuration.Streaming.TVLogoDirectory);
            
            Assert.Equal("en", Configuration.Streaming.DefaultAudioStream);
            Assert.Equal("first", Configuration.Streaming.DefaultSubtitleStream);
        }

        [Fact]
        public void WebMediaPortalHosting()
        {
            Assert.Equal(9999, Configuration.WebMediaPortalHosting.Port);
        }
    }
}
