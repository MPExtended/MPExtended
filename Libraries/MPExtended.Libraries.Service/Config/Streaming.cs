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
using System.Runtime.Serialization;
using System.Text;

namespace MPExtended.Libraries.Service.Config
{
    [DataContract(Name = "TranscoderProfile", Namespace = "http://mpextended.github.com/schema/config/Streaming/1")]
    public class TranscoderProfile
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool HasVideoStream { get; set; }
        [DataMember]
        public string MIME { get; set; }
        [DataMember]
        public int MaxOutputWidth { get; set; }
        [DataMember]
        public int MaxOutputHeight { get; set; }
        [DataMember]
        public string Target { get; set; }
        [DataMember]
        public int Bandwidth { get; set; }
        [DataMember]
        public string Transport { get; set; }
        [DataMember]
        public string Transcoder { get; set; }
        [DataMember]
        public ConfigDictionary TranscoderParameters { get; set; }

        public TranscoderProfile()
        {
            TranscoderParameters = new ConfigDictionary();
        }
    }

    [DataContract(Name = "WatchSharingConfiguration", Namespace = "http://mpextended.github.com/schema/config/Streaming/1")]
    public class WatchSharingConfiguration
    {
        [DataMember]
        public bool DebugEnabled { get; set; }
        [DataMember]
        public bool TraktEnabled { get; set; }
        [DataMember]
        public ConfigDictionary TraktConfiguration { get; set; }
        [DataMember]
        public bool FollwitEnabled { get; set; }
        [DataMember]
        public ConfigDictionary FollwitConfiguration { get; set; }

        public WatchSharingConfiguration()
        {
            DebugEnabled = false;
            TraktEnabled = false;
            TraktConfiguration = new ConfigDictionary();
            FollwitEnabled = false;
            FollwitConfiguration = new ConfigDictionary();
        }
    }

    [DataContract(Name = "Streaming", Namespace = "http://mpextended.github.com/schema/config/Streaming/1")]
    public class Streaming
    {
        public const string STREAM_NONE = "none";
        public const string STREAM_DEFAULT = "default";
        public const string STREAM_EXTERNAL = "external";

        private string _ffmpegPath;

        [DataMember]
        public string DefaultAudioStream { get; set; }
        [DataMember]
        public string DefaultSubtitleStream { get; set; }

        [DataMember]
        public string TVLogoDirectory { get; set; }

        [DataMember]
        public WatchSharingConfiguration WatchSharing { get; set; }

        [DataMember]
        public List<TranscoderProfile> Transcoders { get; set; }

        [DataMember]
        public string FFMpegPath
        {
            get
            {
                return _ffmpegPath;
            }
            set
            {
                _ffmpegPath = Configuration.PerformFolderSubstitution(value);
            }
        }

        public Streaming()
        {
            WatchSharing = new WatchSharingConfiguration();
            Transcoders = new List<TranscoderProfile>();
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
    }
}
