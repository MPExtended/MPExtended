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
using System.Linq;
using System.Reflection;
using System.Text;
using MPExtended.Libraries.Service.Internal;

namespace MPExtended.Libraries.Service
{
    /// <summary>
    /// Utility to parse our own version information.
    /// 
    /// We differentiate between 4 different versions for MPExtended:
    /// - The API version, defined by AssemblyVersion in GlobalVersion.cs. This is retrieved with GetVersion() and changes only
    ///   with new feature releases (minor/major). Only the first two numbers are relevant here.
    /// - The version name, defined by AssemblyInformationalVersion in GlobalVersion.cs. This is retrieved with GetVersioName()
    ///   and is different for each release. It isn't of a fixed format, and should only be displayed to the user and/or printed
    ///   to logs. Has no technical meaning.
    /// - The build version, defined by AssemblyFileVersion in GlobalVersion.cs. This is retrieved with GetBuildVersion() and
    ///   changes with each release. As opposed to AssemblyInformationalVersion, this number always increments and has a meaning.
    ///   It is the number that is used for checking for updates. 
    /// - The git commit version, defined by AssemblyGitVersion in GitVersion.cs, only on this assembly. This is retrieved with
    ///   GetGitVersion() and changes with each build. 
    /// A full version string for logs can be retrieved with GetFullVersionString(). 
    /// </summary>
    public class VersionUtil
    {
        public static Version GetVersion()
        {
            return GetVersion(Assembly.GetExecutingAssembly());
        }

        public static Version GetVersion(Assembly asm)
        {
            return asm.GetName().Version;
        }

        public static string GetVersionName()
        {
            return GetVersionName(Assembly.GetExecutingAssembly());
        }

        public static string GetVersionName(Assembly asm)
        {
            return System.Diagnostics.FileVersionInfo.GetVersionInfo(asm.Location).ProductVersion;
        }

        public static Version GetBuildVersion()
        {
            return GetBuildVersion(Assembly.GetExecutingAssembly());
        }

        public static Version GetBuildVersion(Assembly asm)
        {
            string ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(asm.Location).FileVersion;
            return new Version(ver);
        }

        public static string GetGitVersion()
        {
            var attributes = (AssemblyGitVersionAttribute[])Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyGitVersionAttribute), true);
            if (attributes.Length == 0)
            {
                return null;
            }
            else
            {
                string fullHash = attributes.First().Commit;
                return fullHash.Substring(0, Math.Min(fullHash.Length, 7));
            }
        }

        private static string GetCommitOrBuildVersion()
        {
            string gitVersion = GetGitVersion();
            return String.IsNullOrEmpty(gitVersion) ?
                "build " + GetBuildVersion() :
                "commit " + gitVersion;
        }

        public static string GetFullVersionString()
        {
            return String.Format("{0} ({1})", GetVersionName(), GetCommitOrBuildVersion());
        }

        public static string GetUserAgent()
        {
            return String.Format("MPExtended/{0} ({1}; .NET {2})", GetVersionName(), GetCommitOrBuildVersion(), Environment.Version.ToString());
        }

        public static string GetUserAgent(string component, string componentVersion)
        {
            return String.Format("{0} {1}/{2}", GetUserAgent(), component, componentVersion);
        }

        public static string GetUserAgent(string component)
        {
            return GetUserAgent(component, GetVersionName());
        }
    }
}
