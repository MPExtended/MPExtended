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
    [DataContract(Name = "StreamType", Namespace = "http://mpextended.github.com/schema/config/1/WebMediaPortal")]
    public enum StreamType
    {
        [EnumMember]
        Direct,
        [EnumMember]
        DirectWhenPossible,
        [EnumMember]
        Proxied,
    }

    [DataContract(Name = "WebMediaPortal", Namespace = "http://mpextended.github.com/schema/config/1/WebMediaPortal")]
    public class WebMediaPortal
    {
        [DataMember]
        public StreamType StreamType { get; set; }

        [DataMember]
        public int? DefaultGroup { get; set; }

        [DataMember]
        public string DefaultMediaProfile { get; set; }
        [DataMember]
        public string DefaultTVProfile { get; set; }
        [DataMember]
        public bool EnableDeinterlace { get; set; }
        [DataMember]
        public string DefaultAudioProfile { get; set; }
        [DataMember]
        public bool EnableAlbumPlayer { get; set; }

        [DataMember]
        public int? TVShowProvider { get; set; }
        [DataMember]
        public int? MovieProvider { get; set; }
        [DataMember]
        public int? MusicProvider { get; set; }
        [DataMember]
        public int? PicturesProvider { get; set; }
        [DataMember]
        public int? FileSystemProvider { get; set; }

        [DataMember]
        public string MASUrl { get; set; }
        [DataMember]
        public string TASUrl { get; set; }

        [DataMember]
        public string Skin { get; set; }
        [DataMember]
        public string DefaultLanguage { get; set; }

        public WebMediaPortal()
        {
            StreamType = StreamType.DirectWhenPossible;
            MASUrl = "auto://127.0.0.1:4322/";
            TASUrl = "auto://127.0.0.1:4322/";
            Skin = "default";
        }
    }
}
