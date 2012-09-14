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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xml.Serialization.GeneratedAssembly;

namespace MPExtended.Libraries.Service.Config.Upgrade
{
    internal class StreamingUpgrader : AttemptConfigUpgrader<Streaming>
    {
        protected override Streaming DoUpgrade()
        {
            var file = XElement.Load(OldPath);
            var model = new Streaming();

            model.DefaultAudioStream = file.Element("defaultStreams").Element("audio").Value;
            model.DefaultSubtitleStream = file.Element("defaultStreams").Element("subtitle").Value;

            model.TVLogoDirectory = TransformationCallbacks.FolderSubstitution(file.Element("tvLogoDirectory").Value);

            if (file.Element("watchsharing").Element("type").Value == "trakt")
            {
                model.WatchSharing.TraktEnabled = true;
                model.WatchSharing.TraktConfiguration = ParseWatchSharingConfig(file);
            }
            else if (file.Element("watchsharing").Element("type").Value == "follwit")
            {
                model.WatchSharing.FollwitEnabled = true;
                model.WatchSharing.FollwitConfiguration = ParseWatchSharingConfig(file);
            }

            return model;
        }

        private ConfigDictionary ParseWatchSharingConfig(XElement file)
        {
            return new ConfigDictionary()
                {
                    { "username", file.Element("watchsharing").Element("username").Value },
                    { "passwordHash", file.Element("watchsharing").Element("passwordHash").Value },
                };
        }
    }
}
