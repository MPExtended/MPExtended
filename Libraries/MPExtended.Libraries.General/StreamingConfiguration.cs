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
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MPExtended.Libraries.General
{
    public class TranscoderProfile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool HasVideoStream { get; set; }
        public string MIME { get; set; }
        public int MaxOutputWidth { get; set; }
        public int MaxOutputHeight { get; set; }
        public string Target { get; set; }
        public int Bandwidth { get; set; }
        public string Transport { get; set; }
        public string TranscoderImplementationClass { get; set; }
        public IDictionary<string, string> CodecParameters { get; set; }
    }

    public class StreamingConfiguration
    {
        public const string STREAM_NONE = "none";
        public const string STREAM_DEFAULT = "default";
        public const string STREAM_EXTERNAL = "external";

        public string DefaultAudioStream { get; set; }
        public string DefaultSubtitleStream { get; set; }

        public string TVLogoDirectory { get; set; }

        public string FFMpegPath { get; set; }
        public string FFMpegAPI { get; set; }

        public Dictionary<string, string> WatchSharing { get; set; }

        public List<TranscoderProfile> Transcoders { get; set; }

        public StreamingConfiguration()
        {
            XElement file = XElement.Load(Configuration.GetPath("Streaming.xml"));

            DefaultAudioStream = file.Element("defaultStreams").Element("audio").Value;
            DefaultSubtitleStream = file.Element("defaultStreams").Element("subtitle").Value;

            TVLogoDirectory = Configuration.PerformFolderSubstitution(file.Element("tvLogoDirectory").Value);

            FFMpegPath = file.Element("ffmpeg").Element("path").Value;
            FFMpegAPI = file.Element("ffmpeg").Element("api").Value;

            WatchSharing = file.Element("watchsharing").Elements().ToDictionary(x => x.Name.LocalName, x => x.Value);

            Transcoders = file.Element("transcoders").Elements("transcoder").Select(x => new TranscoderProfile()
            {
                Name = x.Element("name").Value,
                Description = x.Element("description").Value,
                Bandwidth = Int32.Parse(x.Element("bandwidth").Value),
                Target = x.Element("target").Value,
                Transport = x.Element("transport").Value,
                MaxOutputHeight = x.Element("maxOutputHeight") != null ? Int32.Parse(x.Element("maxOutputHeight").Value) : 0,
                MaxOutputWidth = x.Element("maxOutputWidth") != null ? Int32.Parse(x.Element("maxOutputWidth").Value) : 0,
                MIME = x.Element("mime").Value,
                HasVideoStream = x.Element("videoStream").Value == "true",
                CodecParameters = x.Element("transcoderConfiguration").Descendants().ToDictionary(el => el.Name.LocalName, el => el.Value),
                TranscoderImplementationClass = (string)x.Element("transcoderConfiguration").Attribute("implementation")
            }).ToList();
        }

        public TranscoderProfile GetTranscoderProfileByName(string name) 
        {
            var list = Transcoders.Where(x => x.Name == name);
            if(list.Count() == 0) 
            {
                Log.Error("Tried to load invalid transcoder profile {0}", name);
                return null;
            }

            return list.First();
        }

        public bool Save()
        {
            try
            {
                XElement file = XElement.Load(Configuration.GetPath("Streaming.xml"));

                file.Element("defaultStreams").Element("audio").Value = DefaultAudioStream;
                file.Element("defaultStreams").Element("subtitle").Value = DefaultSubtitleStream;

                file.Element("tvLogoDirectory").Value = TVLogoDirectory;

                file.Element("ffmpeg").Element("path").Value = FFMpegPath;
                file.Element("ffmpeg").Element("api").Value = FFMpegAPI;

                file.Element("watchsharing").Elements().Remove();
                foreach (KeyValuePair<string, string> kvp in WatchSharing)
                {
                    file.Element("watchsharing").Add(new XElement(kvp.Key, kvp.Value));
                }

                file.Element("transcoders").Elements("transcoder").Remove();
                foreach (TranscoderProfile profile in Transcoders)
                {
                    XElement node = new XElement("transcoder");
                    node.Add(new XElement("name", profile.Name));
                    node.Add(new XElement("description", profile.Description));
                    node.Add(new XElement("bandwidth", profile.Bandwidth));
                    node.Add(new XElement("target", profile.Target));
                    node.Add(new XElement("transport", profile.Transport));
                    node.Add(new XElement("maxOutputWidth", profile.MaxOutputWidth));
                    node.Add(new XElement("maxOutputHeight", profile.MaxOutputHeight));
                    node.Add(new XElement("mime", profile.MIME));
                    node.Add(new XElement("videoStream", profile.HasVideoStream ? "true" : "false"));
                    
                    XElement transcoderConfig = new XElement("transcoderConfiguration", new XAttribute("implementation", profile.TranscoderImplementationClass));
                    foreach(KeyValuePair<string, string> item in profile.CodecParameters) 
                    {
                        transcoderConfig.Add(new XElement(item.Key, item.Value));
                    }
                    node.Add(transcoderConfig);

                    file.Element("transcoders").Add(node);
                }

                file.Save(Configuration.GetPath("Streaming.xml"));
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to save streaming configuration", ex);
                return false;
            }
        }
    }
}
