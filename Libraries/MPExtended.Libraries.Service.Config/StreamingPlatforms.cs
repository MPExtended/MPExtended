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

    public class StreamingPlatform
    {
        public string Name { get; set; }
        public string UserAgentPattern { get { return userAgentPattern; } set { SetUserAgentPattern(value); } }
        public string DefaultAudioProfile { get; set; }
        public string DefaultVideoProfile { get; set; }
        public string DefaultTvProfile { get; set; }
        [XmlArrayItem("Target")]
        public List<string> ValidTargets { get; set; }

        private Regex regexPattern;
        private String userAgentPattern;

        private void SetUserAgentPattern(string pattern)
        {
            userAgentPattern = pattern;
            regexPattern = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public bool MatchesUserAgent(string userAgent)
        {
            return regexPattern.IsMatch(userAgent);
        }

        public StreamingPlatform()
        {
        }
    }

    [XmlRoot(ElementName = "StreamingPlatforms", Namespace = "http://mpextended.github.com/schema/config/StreamingPlatforms/1")]
    public class StreamingPlatforms : List<StreamingPlatform>
    {
        public List<String> GetPlatforms()
        {
            return this.Select(x => x.Name).Distinct().ToList();
        }

        public IEnumerable<string> GetValidTargetsForUserAgent(string userAgent)
        {
            return this.FirstOrDefault(x => x.MatchesUserAgent(userAgent)).ValidTargets;
        }

        public string GetDefaultProfileForUserAgent(StreamingProfileType type, string userAgent)
        {
            switch (type)
            {
                case StreamingProfileType.Audio:
                    return GetDefaultAudioProfileForUserAgent(userAgent);
                case StreamingProfileType.Tv:
                    return GetDefaultTvProfileForUserAgent(userAgent);
                default:
                    return GetDefaultVideoProfileForUserAgent(userAgent);
            }
        }

        public string GetDefaultAudioProfileForUserAgent(string userAgent)
        {
            return this.FirstOrDefault(x => x.MatchesUserAgent(userAgent)).DefaultAudioProfile;
        }

        public string GetDefaultAudioProfileForPlatform(string platform)
        {
            return this.FirstOrDefault(x => x.Name == platform).DefaultAudioProfile;
        }

        public void SetDefaultAudioProfileForPlatform(string platform, string profile)
        {
            this.FirstOrDefault(x => x.Name == platform).DefaultAudioProfile = profile;
        }

        public string GetDefaultVideoProfileForUserAgent(string userAgent)
        {
            return this.FirstOrDefault(x => x.MatchesUserAgent(userAgent)).DefaultVideoProfile;
        }

        public string GetDefaultVideoProfileForPlatform(string platform)
        {
            return this.FirstOrDefault(x => x.Name == platform).DefaultVideoProfile;
        }

        public void SetDefaultVideoProfileForPlatform(string platform, string profile)
        {
            this.FirstOrDefault(x => x.Name == platform).DefaultVideoProfile = profile;
        }

        public string GetDefaultTvProfileForUserAgent(string userAgent)
        {
            return this.FirstOrDefault(x => x.MatchesUserAgent(userAgent)).DefaultTvProfile;
        }

        public string GetDefaultTvProfileForPlatform(string platform)
        {
            return this.FirstOrDefault(x => x.Name == platform).DefaultTvProfile;
        }

        public void SetDefaultTvProfileForPlatform(string platform, string profile)
        {
            this.FirstOrDefault(x => x.Name == platform).DefaultTvProfile = profile;
        }

        public StreamingPlatforms()
        {
        }
    }
}
