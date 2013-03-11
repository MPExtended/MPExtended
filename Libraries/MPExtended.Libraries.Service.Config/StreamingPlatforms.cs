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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MPExtended.Libraries.Service.Config
{
    public enum StreamingProfileType
    {
        Audio,
        Video,
        Tv
    }

    [XmlType(Namespace = "http://mpextended.github.com/schema/config/StreamingPlatforms/1")]
    public class StreamingPlatform
    {
        public string Name { get; set; }
        public string DefaultAudioProfile { get; set; }
        public string DefaultVideoProfile { get; set; }
        public string DefaultTvProfile { get; set; }
        [XmlArrayItem("Target")]
        public List<string> ValidTargets { get; set; }

        private Regex regexPattern;
        private string userAgentPattern;

        public string UserAgentPattern
        {
            get
            {
                return userAgentPattern;
            }
            set
            {
                userAgentPattern = value;
                regexPattern = new Regex(value, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
        }

        public bool MatchesUserAgent(string userAgent)
        {
            return regexPattern.IsMatch(userAgent);
        }
    }

    [XmlRoot(ElementName = "StreamingPlatforms", Namespace = "http://mpextended.github.com/schema/config/StreamingPlatforms/1")]
    public class StreamingPlatforms : List<StreamingPlatform>
    {
        public StreamingPlatforms()
        {
        }

        public List<String> GetPlatforms()
        {
            return this.Select(x => x.Name).Distinct().ToList();
        }

        public IEnumerable<string> GetValidTargetsForUserAgent(string userAgent)
        {
            var platform = this.FirstOrDefault(x => x.MatchesUserAgent(userAgent));
            return platform != null ? platform.ValidTargets : new List<string>();
        }

        public IEnumerable<string> GetValidTargetsForPlatform(string platformName)
        {
            var platform = this.FirstOrDefault(x => x.Name == platformName);
            return platform != null ? platform.ValidTargets : new List<string>();
        }

        public string GetDefaultProfileForUserAgent(StreamingProfileType type, string userAgent)
        {
            return GetDefaultProfile(type, this.FirstOrDefault(x => x.MatchesUserAgent(userAgent)));
        }

        public string GetDefaultProfileForPlatform(StreamingProfileType type, string platform)
        {
            return GetDefaultProfile(type, this.FirstOrDefault(x => x.Name == platform));
        }

        private string GetDefaultProfile(StreamingProfileType type, StreamingPlatform platform)
        {
            if (platform == null)
                return null;

            var methods = new Dictionary<StreamingProfileType, Func<StreamingPlatform, string>>()
            {
                { StreamingProfileType.Audio, x => x.DefaultAudioProfile },
                { StreamingProfileType.Video, x => x.DefaultVideoProfile },
                { StreamingProfileType.Tv, x => x.DefaultTvProfile },
            };

            return methods[type].Invoke(platform);
        }

        public void SetDefaultProfileForPlatform(StreamingProfileType type, string platform, string profile)
        {
            SetDefaultProfile(type, this.FirstOrDefault(x => x.Name == platform), profile);
        }

        private void SetDefaultProfile(StreamingProfileType type, StreamingPlatform platform, string profile)
        {
            if (platform == null)
                return;

            var methods = new Dictionary<StreamingProfileType, Action<StreamingPlatform, string>>()
            {
                { StreamingProfileType.Audio, (x, y) => x.DefaultAudioProfile = y },
                { StreamingProfileType.Video, (x, y) => x.DefaultVideoProfile = y },
                { StreamingProfileType.Tv, (x, y) => x.DefaultTvProfile = y },
            };

            methods[type].Invoke(platform, profile);
        }
    }
}
