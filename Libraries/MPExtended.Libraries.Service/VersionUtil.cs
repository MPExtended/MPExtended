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
using System.Diagnostics;
using System.IO;
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
        public enum MediaPortalVersion 
        {
            NotAvailable = 1,
            Unknown = 2,
            MP1_1 = 3,
            MP1_2 = 4,
            MP1_3 = 5,
        }

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

        public static string GetFullVersionString()
        {
            string gitVersion = GetGitVersion();
            if (gitVersion.Length > 0)
            {
                return String.Format("{0} (commit {1})", GetVersionName(), gitVersion);
            }
            else
            {
                return string.Format("{0} (build {1})", GetVersionName(), GetBuildVersion());
            }
        }

        public static MediaPortalVersion GetMediaPortalVersion()
        {
            Version v = GetMediaPortalBuildVersion();
            if (v == null)
            {
                return MediaPortalVersion.NotAvailable;
            }
            else if ((v.Major == 1 && v.Minor == 3) || (v.Major == 1 && v.Minor == 2 && v.Build >= 100)) // MP1.3 Alpha used 1.2.100.0 as version number
            {
                return MediaPortalVersion.MP1_3;
            }
            else if (v.Major == 1 && v.Minor == 2) // Not sure about the alpha versions, but those are so ancient...
            {
                return MediaPortalVersion.MP1_2;
            }
            else if (v.Major == 1 && v.Minor == 1) // We don't even support this anymore, but whatever... 
            {
                return MediaPortalVersion.MP1_1;
            }

            return MediaPortalVersion.Unknown;
        }

        public static string GetMediaPortalVersionString()
        {
            // Until MediaPortal provides an informational version name somewhere, try to map them ourselves
            Version v = GetMediaPortalBuildVersion();
            if (v == null)
                return "(not installed)";

            // List all exceptions (pre-releases) from the normal versioning scheme here
            if (v.Major == 1 && v.Minor == 2 && v.Build == 100)
                return "1.3.0 Alpha";

            // Normal versioning scheme
            return String.Format("{0}.{1}.{2}", v.Major, v.Minor, v.Build);
        }

        public static Version GetMediaPortalBuildVersion()
        {
            var assemblyPath = GetMediaPortalAssemblyPath();
            return assemblyPath != null ? AssemblyName.GetAssemblyName(assemblyPath).Version : null;
        }

        private static string GetMediaPortalAssemblyPath()
        {
            string tv = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Team MediaPortal", "MediaPortal TV Server", "TvService.exe");
            if (File.Exists(tv))
            {
                return tv;
            }

            string mp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Team MediaPortal", "MediaPortal", "MediaPortal.exe");
            if (File.Exists(mp))
            {
                return mp;
            }

            Log.Error("Cannot find installed TvService.exe or MediaPortal.exe");
            return null;
        }
    }
}
