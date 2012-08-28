#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
    public class Parsing : ConfigurationTestEnvironment
    {
        public Parsing()
            : base()
        {
            AddConfigurationFile("Authentication.xml", "ParsingTests/Authentication.xml");
            AddConfigurationFile("MediaAccess.xml", "ParsingTests/MediaAccess.xml");
            AddConfigurationFile("Services.xml", "ParsingTests/Services.xml");
            AddConfigurationFile("Streaming.xml", "ParsingTests/Streaming.xml");
        }

        [Fact]
        public void Authentication()
        {
            Assert.True(Configuration.Authentication.Enabled);
            Assert.Single(Configuration.Authentication.Users);

            var user = Configuration.Authentication.Users.First();
            Assert.Equal("admin", user.Username);
            Assert.True(user.ValidatePassword("admin"));
            Assert.False(user.ValidatePassword("something else"));
        }

        [Fact]
        public void MediaAccess()
        {
            Assert.Equal("MP Shares", Configuration.Media.DefaultPlugins.Filesystem);
            Assert.Null(Configuration.Media.DefaultPlugins.Movie);

            Assert.Equal(3, Configuration.Media.PluginConfiguration.Count);

            var testPlugin = Configuration.Media.PluginConfiguration["TestPlugin"];
            Assert.NotNull(testPlugin);
            Assert.Equal(2, testPlugin.Count);

            var nr = testPlugin.Single(x => x.Name == "number");
            Assert.Equal("5", nr.Value);
            Assert.Equal(ConfigType.Number, nr.Type);
            Assert.Equal("Some value", nr.DisplayName);

            var path = testPlugin.Single(x => x.Name == "path");
            Assert.Equal(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "path"), path.Value);
        }

        [Fact]
        public void Services()
        {
            Assert.Equal(true, Configuration.Services.AccessRequestEnabled);

            Assert.Equal("DOMAIN", Configuration.Services.NetworkImpersonation.Domain);
            Assert.Equal("USERNAME", Configuration.Services.NetworkImpersonation.Username);
            Assert.Equal("admin", Configuration.Services.NetworkImpersonation.GetPassword());

            Assert.Equal(true, Configuration.Services.BonjourEnabled);
            Assert.Equal("BONJOUR", Configuration.Services.BonjourName);

            Assert.Equal(true, Configuration.Services.ExternalAddress.Autodetect);
            Assert.True(String.IsNullOrEmpty(Configuration.Services.ExternalAddress.Custom));

            Assert.Equal(4322, Configuration.Services.Port);
        }

        [Fact]
        public void Streaming()
        {
            Assert.True(Configuration.Streaming.WatchSharing.DebugEnabled);
            Assert.False(Configuration.Streaming.WatchSharing.FollwitEnabled);
            Assert.True(String.IsNullOrEmpty(Configuration.Streaming.WatchSharing.FollwitConfiguration["username"]));
            Assert.True(String.IsNullOrEmpty(Configuration.Streaming.WatchSharing.FollwitConfiguration["passwordHash"]));
            Assert.True(Configuration.Streaming.WatchSharing.TraktEnabled);
            Assert.Equal("user", Configuration.Streaming.WatchSharing.TraktConfiguration["username"]);
            Assert.Equal("abcd", Configuration.Streaming.WatchSharing.TraktConfiguration["passwordHash"]);

            Assert.Equal(Path.Combine(Installation.Properties.SourceRoot, "Libraries", "Streaming", "ffmpeg.exe"), Configuration.Streaming.FFMpegPath);

            var profiles = Configuration.Streaming.Transcoders;
            Assert.Equal(2, profiles.Count);
            
            var firstProfile = profiles.Single(x => x.Name == "TEST P1");
            Assert.Equal("DESC", firstProfile.Description);
            Assert.Equal(1400, firstProfile.Bandwidth);
            Assert.Equal(2, firstProfile.Targets.Count);
            Assert.True(firstProfile.Targets.Contains("target1"));
            Assert.True(firstProfile.Targets.Contains("target2"));
            Assert.Equal("http", firstProfile.Transport);
            Assert.Equal(800, firstProfile.MaxOutputWidth);
            Assert.Equal(600, firstProfile.MaxOutputHeight);
            Assert.Equal("video/MP2T", firstProfile.MIME);
            Assert.Equal("MPExtended.Services.StreamingService.Transcoders.FFMpeg", firstProfile.Transcoder);
            Assert.Empty(firstProfile.TranscoderParameters);
            Assert.True(firstProfile.HasVideoStream);

            var secondProfile = profiles.Single(x => x.Name == "TEST P2");
            Assert.Empty(secondProfile.Targets);
            Assert.False(secondProfile.HasVideoStream);
            Assert.Equal(2, secondProfile.TranscoderParameters.Count);
            Assert.Equal("CODECPARAMETERS", secondProfile.TranscoderParameters["codecParameters"]);
            Assert.Equal("VALUE", secondProfile.TranscoderParameters["otherKey"]);
        }
    }
}
