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
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace MPExtended.Libraries.General
{
    public class VersionUtil
    {
        public enum MediaPortalVersion 
        {
            Unknown = 1,
            MP1_1 = 2,
            MP1_2 = 3,
            MP1_3_Alpha = 4,
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

        public static MediaPortalVersion GetMediaPortalVersion()
        {
            Version v = GetCompleteMediaPortalVersion();
            if (v.Major == 1 && v.Minor == 1)
            {
                return MediaPortalVersion.MP1_1;
            }
            else if (v.Major == 1 && v.Minor == 2)
            {
                return MediaPortalVersion.MP1_2;
            }
            else if (v.Major == 1 && v.Minor == 3)
            {
                return MediaPortalVersion.MP1_3_Alpha;
            }

            return MediaPortalVersion.Unknown;
        }

        public static Version GetCompleteMediaPortalVersion()
        {
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(GetMediaPortalAssemblyPath());
            return new Version(info.FileVersion);
        }

        public static Version GetMediaPortalBuildVersion()
        {
            return AssemblyName.GetAssemblyName(GetMediaPortalAssemblyPath()).Version;
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
