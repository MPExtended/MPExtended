#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using System.Xml;
using System.Xml.Linq;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Transcoders;

namespace MPExtended.Services.StreamingService.Code
{
    internal class TranscoderProfile : WebTranscoderProfile
    {
        public IDictionary<string, string> CodecParameters { get; set; }
        public string TranscoderImplementationClass { get; set; }

        public ITranscoder GetTranscoder()
        {
            // For now only support files in current assembly
            ITranscoder inst = (ITranscoder)Activator.CreateInstance(Type.GetType(TranscoderImplementationClass));
            inst.Profile = this;
            return inst;
        }
    }

    internal static class Config
    {
        public static TranscoderProfile GetTranscoderProfileByName(string name)
        {
            return GetTranscoderProfiles().Where(s => s.Name == name).FirstOrDefault();
        }

        public static ICollection<TranscoderProfile> GetTranscoderProfiles()
        {
            return XElement.Load(Configuration.GetPath("Streaming.xml"))
                .Descendants("transcoder").Select(
                    x => new TranscoderProfile()
                    {
                        Name = x.Element("name").Value,
                        Description = x.Element("description").Value,
                        Bandwidth = Int32.Parse(x.Element("bandwidth").Value),
                        Target = x.Element("target").Value,
                        MaxOutputHeight = x.Element("maxOutputHeight") != null ? Int32.Parse(x.Element("maxOutputHeight").Value) : 0,
                        MaxOutputWidth = x.Element("maxOutputWidth") != null ? Int32.Parse(x.Element("maxOutputWidth").Value) : 0,
                        MIME = x.Element("mime").Value,
                        HasVideoStream = x.Element("videoStream").Value == "true",

                        TranscoderImplementationClass = (string)x.Element("transcoderConfiguration").Attribute("implementation"),
                        CodecParameters = x.Element("transcoderConfiguration").Descendants().ToDictionary(el => el.Name.LocalName, el => el.Value)
                    }).ToList();

        }

        public static string GetFFMpegPath()
        {
            return XElement.Load(Configuration.GetPath("Streaming.xml")).Element("ffmpeg").Element("path").Value;
        }
    }
}
