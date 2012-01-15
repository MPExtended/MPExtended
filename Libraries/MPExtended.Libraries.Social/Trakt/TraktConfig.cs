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
using System.Text;
using MPExtended.Libraries.Service;

namespace MPExtended.Libraries.Social.Trakt
{
    internal class TraktConfig
    {
        private const string ApiKey = "dc2e63ec2918e23e24d8a4c2e3813fc7a4ad22d7";
        private static string buildDate = null;

        public static string MediaCenter 
        { 
            get 
            { 
                return "MPExtended"; 
            } 
        }

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
                if (buildDate == null)
                {
                    var info = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    buildDate = info.LastWriteTime.ToString("yyyy-MM-dd");
                }

                return buildDate;
            } 
        }

        public static string PluginVersion
        { 
            get 
            {
                return String.Format("{0} (build {1}, commit {2}, MP {2})", VersionUtil.GetVersionName(), VersionUtil.GetBuildVersion().ToString(), 
                    VersionUtil.GetGitVersion(), VersionUtil.GetMediaPortalBuildVersion().ToString());
            } 
        }

        public static string UserAgent
        {
            get
            {
                return String.Format("MPExtended {0}", VersionUtil.GetFullVersionString());
            }
        }

        internal static class URL
        {
            public const string ScrobbleShow = @"http://api.trakt.tv/show/{0}/" + ApiKey;
            public const string ScrobbleMovie = @"http://api.trakt.tv/movie/{0}/" + ApiKey;
            public const string TestAccount = @"http://api.trakt.tv/account/test/" + ApiKey;
        }
    }
}
