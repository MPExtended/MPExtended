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
using System.IO;
using System.Xml.Linq;
using Microsoft.Win32;
using System.Diagnostics;

namespace MPExtended.Libraries.Service.Util
{
    public enum MediaportalDirectory
    {
        Config,
        Database,
        Thumbs,
        Cache,
        Skin
    }

    public static class Mediaportal
    {
        private static bool? hasValidConfig = null;
        private static bool? hasMpDirs = null;

        public static string GetClientInstallationDirectory()
        {
            object res = RegistryReader.ReadKeyAllViews(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal", "InstallPath");
            if (res != null)
            {
                return res.ToString();
            }
            else
            {
                return null;
            }
        }

        public static string GetLocation(MediaportalDirectory type)
        {
            if (hasMpDirs.HasValue && !hasMpDirs.Value)
            {
                return null;
            }

            try
            {
                // read from MediaPortalDirs.xml
                string clientInstallDir = GetClientInstallationDirectory();
                string mpDirs = clientInstallDir == null ? null : Path.Combine(clientInstallDir, "MediaPortalDirs.xml");
                if (mpDirs == null || !File.Exists(mpDirs))
                {
                    Log.Debug("Could not find MediaPortalDirs.xml");
                    hasMpDirs = false;
                    return null;
                }

                XElement file = XElement.Load(mpDirs);
                var element = file.Elements("Dir").Where(x => x.Attribute("id").Value == type.ToString());
                if (element.Count() == 0)
                {
                    Log.Debug("Could not find directory with id {0} in MediaPortalDirs.xml", type);
                    return null;
                }

                // apply transformations
                var path = element.First().Element("Path").Value;
                path = path.Replace("%ProgramData%", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
                if (!Path.IsPathRooted(path))
                {
                    path = Path.Combine(GetClientInstallationDirectory(), path);
                }

                // and return it
                return Path.GetFullPath(path);
            }
            catch (Exception ex)
            {
                Log.Warn("Error while loading MediaPortalDirs.xml", ex);
                return null;
            }
        }

        public static Dictionary<string, string> ReadSectionFromConfigFile(string sectionName)
        {
            try
            {
                if (!HasValidConfigFile())
                {
                    return new Dictionary<string, string>();
                }

                XElement file = XElement.Load(GetConfigFilePath());

                // find section
                var elements = file.Elements("section").Where(x => x.Attribute("name").Value == sectionName);
                if (elements.Count() == 0)
                {
                    Log.Debug("Requested MediaPortal config for non-available section {0}", sectionName);
                    return new Dictionary<string, string>();
                }

                // return list
                return elements.First().Elements("entry").ToDictionary(x => x.Attribute("name").Value, x => x.Value);
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to read section {0} from MediaPortal.xml", sectionName), ex);
                return new Dictionary<string, string>();
            }
        }

        private static string GetConfigFilePath()
        {
            string location = GetLocation(MediaportalDirectory.Config);
            if (location == null)
            {
                return null;
            }
            else
            {
                return Path.Combine(location, "MediaPortal.xml");
            }
        }

        public static bool HasValidConfigFile()
        {
            if (hasValidConfig.HasValue)
            {
                return hasValidConfig.Value;
            }

            hasValidConfig = false;
            try
            {
                string path = GetConfigFilePath();
                if(path == null || !File.Exists(path))
                {
                    Log.Info("MediaPortal configuration file does not exists (this isn't a problem for server-only installations)");
                    return false;
                }

                var xml = XElement.Load(path); // try to parse it
                hasValidConfig = true;
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn("Error trying to detect valid MediaPortal configuration file", ex);
                return false;
            }
        }


        public static void LogVersionDetails()
        {
            try
            {
                var version = VersionUtil.GetMediaPortalVersion();
                if (version == VersionUtil.MediaPortalVersion.NotAvailable)
                {
                    Log.Info("No MediaPortal installed");
                }
                else if (version >= VersionUtil.MediaPortalVersion.MP1_2)
                {
                    Log.Debug("Found supported MediaPortal installation ({0}, build {1})", version, VersionUtil.GetMediaPortalBuildVersion());
                }
                else
                {
                    Log.Warn("Installed MediaPortal version is not supported! ({0}, build {1})", version, VersionUtil.GetMediaPortalBuildVersion());
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to detect MediaPortal version", ex);
            }
        }

        /// <summary>
        /// Return the MediaPortal path
        /// </summary>
        /// <returns>MediaPortal path</returns>
        public static string GetMediaPortalPath()
        {
            string mpdir = Mediaportal.GetClientInstallationDirectory();
            if (mpdir == null)
            {
                mpdir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Team MediaPortal", "MediaPortal");
            }
            return Path.Combine(mpdir, "MediaPortal.exe");
        }

        /// <summary>
        /// Check if MediaPortal is running
        /// </summary>
        /// <returns></returns>
        public static bool IsMediaPortalRunning()
        {
            return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(GetMediaPortalPath())).Length > 0;
        }
    }
}
