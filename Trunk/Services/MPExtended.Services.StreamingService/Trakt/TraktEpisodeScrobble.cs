﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.StreamingService.Trakt
{
    [DataContract]
    internal class TraktEpisodeScrobble
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "year")]
        public string Year { get; set; }

        [DataMember(Name = "season")]
        public string Season { get; set; }

        [DataMember(Name = "episode")]
        public string Episode { get; set; }

        [DataMember(Name = "tvdb_id")]
        public string TVDBID { get; set; }

        [DataMember(Name = "imdb_id")]
        public string IMDBID { get; set; }

        [DataMember(Name = "progress")]
        public string Progress { get; set; }

        [DataMember(Name = "plugin_version")]
        public string PluginVersion { get; set; }

        [DataMember(Name = "media_center")]
        public string MediaCenter { get; set; }

        [DataMember(Name = "media_center_version")]
        public string MediaCenterVersion { get; set; }

        [DataMember(Name = "media_center_date")]
        public string MediaCenterBuildDate { get; set; }

        [DataMember(Name = "duration")]
        public string Duration { get; set; }

        [DataMember(Name = "username")]
        public string UserName { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }
    }
}
