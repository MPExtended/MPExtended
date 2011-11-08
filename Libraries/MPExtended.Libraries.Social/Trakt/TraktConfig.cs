#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.General;

namespace MPExtended.Libraries.Social.Trakt
{
    internal class TraktConfig
    {
        private const string ApiKey = "abcd";

        public static string MediaCenter { get { return "Mediaportal"; } }

        public static string MediaCenterVersion 
        { 
            get 
            {
                return VersionUtil.GetCompleteMediaPortalVersion().ToString();
            } 
        }

        public static string MediaCenterDate 
        { 
            get 
            {
                return String.Empty;
            } 
        }

        public static string PluginVersion 
        { 
            get 
            {
                return String.Format("{0} ({1})", VersionUtil.GetVersionName(), VersionUtil.GetBuildVersion().ToString());
            } 
        }

        public static string UserAgent
        {
            get
            {
                return String.Format("MPExtended/{0} MediaAccessService/{1}", VersionUtil.GetVersion().ToString(), VersionUtil.GetBuildVersion().ToString());
            }
        }

        internal static class URL
        {
            public const string ScrobbleShow = @"http://api.trakt.tv/show/{0}/" + ApiKey;
            public const string ScrobbleMovie = @"http://api.trakt.tv/movie/{0}/" + ApiKey;
        }
    }
}
