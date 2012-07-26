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

namespace MPExtended.Libraries.Service.Config
{
    public class TranscoderProfile
    {
        public TranscoderProfile()
        {
        }

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

    public class WatchSharingConfiguration
    {
        public WatchSharingConfiguration()
        {
            DebugEnabled = false;
            TraktEnabled = false;
            TraktConfiguration = new Dictionary<string, string>();
            FollwitEnabled = false;
            FollwitConfiguration = new Dictionary<string, string>();
        }

        public bool DebugEnabled { get; set; }
        public bool TraktEnabled { get; set; }
        public Dictionary<string, string> TraktConfiguration { get; set; }
        public bool FollwitEnabled { get; set; }
        public Dictionary<string, string> FollwitConfiguration { get; set; }
    }

    public class Streaming
    {
        public const string STREAM_NONE = "none";
        public const string STREAM_DEFAULT = "default";
        public const string STREAM_EXTERNAL = "external";

        public Streaming()
        {
            WatchSharing = new WatchSharingConfiguration();
            Transcoders = new List<TranscoderProfile>();
        }

        public string DefaultAudioStream { get; set; }
        public string DefaultSubtitleStream { get; set; }

        public string TVLogoDirectory { get; set; }

        public string FFMpegPath { get; set; }
        public string FFMpegAPI { get; set; }

        public WatchSharingConfiguration WatchSharing { get; set; }

        public List<TranscoderProfile> Transcoders { get; set; }

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
