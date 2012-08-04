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
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using MPExtended.Applications.WebMediaPortal.Mvc;
using MPExtended.Applications.WebMediaPortal.Strings;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public enum StreamType
    {
        [LocalizedDescription(typeof(FormStrings), "DirectStreamingProfileDescription")]
        Direct,

        [LocalizedDescription(typeof(FormStrings), "DirectWhenPossibleDescription")]
        DirectWhenPossible,

        [LocalizedDescription(typeof(FormStrings), "ProxiedStreamingProfileDescription")]
        Proxied,
    }

    public class SettingModel
    {
        public SettingModel()
        {
        }

        public StreamType StreamType { get; set; }

        public int? DefaultGroup { get; set; }

        public string DefaultMediaProfile { get; set; }
        public string DefaultTVProfile { get; set; }
        public bool EnableDeinterlace { get; set; }
        public string DefaultAudioProfile { get; set; }
        public bool EnableAlbumPlayer { get; set; }

        public int? TVShowProvider { get; set; }
        public int? MovieProvider { get; set; }
        public int? MusicProvider { get; set; }
        public int? PicturesProvider { get; set; }
        public int? FileSystemProvider { get; set; }

        public string MASUrl { get; set; }
        public string TASUrl { get; set; }

        public string Skin { get; set; }
        public string DefaultLanguage { get; set; }
    }
}