#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Xml.Serialization;

namespace MPExtended.Libraries.Service.Config
{
    [XmlType(Namespace = "http://mpextended.github.com/schema/config/StreamingProfiles/1")]
    public class TranscoderProfile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool HasVideoStream { get; set; }
        public string MIME { get; set; }
        public int MaxOutputWidth { get; set; }
        public int MaxOutputHeight { get; set; }

        [XmlArrayItem(ElementName="Target")]
        public List<string> Targets { get; set; }
        public int Bandwidth { get; set; }
        public string Transport { get; set; }
        public string Transcoder { get; set; }
        public TranscoderConfigDictionary TranscoderParameters { get; set; }

        public TranscoderProfile()
        {
            TranscoderParameters = new TranscoderConfigDictionary();
        }
    }

    [XmlRoot(Namespace = "http://mpextended.github.com/schema/config/StreamingProfiles/1")]
    public class StreamingProfiles
    {
        [XmlIgnore]
        private string _ffmpegPath;

        public List<TranscoderProfile> Transcoders { get; set; }
        
        public string FFMpegPath
        {
            get
            {
                return _ffmpegPath;
            }
            set
            {
                _ffmpegPath = Transformations.FolderNames(value);
            }
        }

        public StreamingProfiles()
        {
            Transcoders = new List<TranscoderProfile>();
        }

        public TranscoderProfile GetTranscoderProfileByName(string name) 
        {
            var list = Transcoders.Where(x => x.Name == name);
            if(list.Count() == 0) 
            {
                return null;
            }

            return list.First();
        }
    }
}
