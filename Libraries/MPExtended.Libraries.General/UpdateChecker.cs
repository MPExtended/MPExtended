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
using System.Net;

namespace MPExtended.Libraries.General
{
    public class ReleasedVersion
    {
        public Version BuildNumber { get; set; }
        public string Version { get; set; }
        public DateTime ReleaseDate { get; set; }
    }

    public class UpdateChecker
    {
        private const string UPDATE_URL = "http://mpextended.github.com/lastversion.txt?version={0}";
        private static List<ReleasedVersion> versionInfoCache = null;

        public static ReleasedVersion GetLastReleasedVersion()
        {
            List<ReleasedVersion> releases = GetReleases();
            if (releases == null || releases.Count() == 0)
            {
                return null;
            }

            releases.Sort((x, y) => y.BuildNumber.CompareTo(x.BuildNumber));
            return releases.First();
        }

        public static bool IsWorking()
        {
            return GetLastReleasedVersion() != null;
        }

        public static bool IsUpdateAvailable()
        {
            Version ourBuild = VersionUtil.GetBuildVersion();
            Version lastBuild = GetLastReleasedVersion().BuildNumber;
            return ourBuild.CompareTo(lastBuild) < 0;
        }

        private static List<ReleasedVersion> GetReleases()
        {
            if (versionInfoCache != null)
            {
                return versionInfoCache;
            }

            string data;

            try
            {
                string updateUrl = String.Format(UPDATE_URL, VersionUtil.GetBuildVersion().ToString());
                Log.Debug("Downloading update information from {0}", updateUrl);
                using (WebClient client = new WebClient())
                {
                    data = client.DownloadString(updateUrl);
                }
            }
            catch (Exception ex)
            {
                Log.Info("Failed to download update information", ex);
                return null;
            }

            // get lines
            string[] lines = data.Replace("\r", "").Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0 && x.Substring(0, 1) != "#").ToArray();
            versionInfoCache = new List<ReleasedVersion>();
            foreach (string line in lines)
            {
                string[] items = line.Split(' ').Select(x => x.Trim()).ToArray();
                versionInfoCache.Add(new ReleasedVersion()
                {
                    BuildNumber = new Version(items[0]),
                    Version = items[1],
                    ReleaseDate = DateTime.Parse(items[2])
                });
            }

            return versionInfoCache;
        }
    }
}
