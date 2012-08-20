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
            model.FFMpegPath = file.Element("ffmpeg").Element("path").Value;

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

            model.Transcoders = ParseAndMergeTranscoders(file);

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

        private List<TranscoderProfile> ParseAndMergeTranscoders(XElement oldFile)
        {
            // load all the transcoders from the old file
            var list = new List<TranscoderProfile>();
            foreach (var node in oldFile.Elements("transcoders").Elements("transcoder"))
            {
                var profile = new TranscoderProfile()
                {
                    Name = node.Element("name").Value,
                    Description = node.Element("description").Value,
                    Bandwidth = Int32.Parse(node.Element("bandwidth").Value),
                    Targets = new List<string>() { node.Element("target").Value },
                    Transport = node.Element("transport").Value,
                    MaxOutputHeight = node.Element("maxOutputHeight") != null ? Int32.Parse(node.Element("maxOutputHeight").Value) : 0,
                    MaxOutputWidth = node.Element("maxOutputWidth") != null ? Int32.Parse(node.Element("maxOutputWidth").Value) : 0,
                    MIME = node.Element("mime").Value,
                    HasVideoStream = node.Element("videoStream").Value == "true",
                    Transcoder = (string)node.Element("transcoderConfiguration").Attribute("implementation")
                };

                foreach (var configNode in node.Elements("transcoderConfiguration").Descendants())
                {
                    profile.TranscoderParameters[configNode.Name.LocalName] = configNode.Value;
                }

                list.Add(profile);
            }

            // parse the new default file
            var defaultSerializer = new StreamingSerializer();
            Streaming defaultModel;
            using (var file = File.OpenRead(DefaultPath))
            {
                defaultModel = (Streaming)defaultSerializer.Deserialize(file);
            }

            // merge the lists
            var finalList = defaultModel.Transcoders;
            foreach (var oldProfile in list)
            {
                if (!finalList.Any(x => x.Name == oldProfile.Name))
                    finalList.Add(oldProfile);
            }

            return finalList;
        }
    }
}
