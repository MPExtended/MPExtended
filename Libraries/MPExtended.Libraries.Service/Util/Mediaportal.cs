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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using Microsoft.Win32;

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
        public enum MediaPortalVersion
        {
            NotAvailable = 1,
            Unknown = 2,
            MP1_1 = 3,
            MP1_2 = 4,
            MP1_3 = 5,
            MP1_4 = 6,
        }

        private static bool? hasValidConfig = null;
        private static bool? hasMpDirs = null;

        public static string GetClientInstallationDirectory()
        {
            object res = RegistryReader.ReadKeyAllViews(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal", "InstallPath");
            return res != null ? res.ToString() : null;
        }

        public static string GetServerInstallationDirectory()
        {
            object res = RegistryReader.ReadKeyAllViews(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server", "InstallPath");
            return res != null ? res.ToString() : null;
        }

        private static string GetAssemblyPath()
        {
            string tv = GetServerInstallationDirectory();
            if (tv != null && File.Exists(Path.Combine(tv, "TvService.exe")))
                return Path.Combine(tv, "TvService.exe");

            string client = GetClientInstallationDirectory();
            if (client != null && File.Exists(Path.Combine(client, "MediaPortal.exe")))
                return Path.Combine(client, "MediaPortal.exe");

            Log.Error("Cannot find installed TvService.exe or MediaPortal.exe");
            return null;
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

        private static XElement LoadConfigurationFile()
        {
            // Open the file with FileShare.ReadWrite to make sure that MP can still write to the file. I'm not 100% sure whether this will work correctly
            // when MP is actually writing to the file while we're reading from it, but worst case it'll throw an exception about invalid XML. Given that
            // the alternative, prohibiting MP from writing to the file, probably causes all kinds of problems in MP, this seems fair to me.
            using (var handle = File.Open(GetConfigFilePath(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return XElement.Load(handle);
            }
        }

        public static bool IsSectionInConfigFile(string sectionName)
        {
            if (!HasValidConfigFile())
                return false;

            XElement file = LoadConfigurationFile();
            return file.Elements("section").Any(x => x.Attribute("name").Value == sectionName);
        }

        public static Dictionary<string, string> ReadSectionFromConfigFile(string sectionName)
        {
            try
            {
                if (!HasValidConfigFile())
                    return new Dictionary<string, string>();

                // open the file with FileShare.ReadWrite to allow MP to 
                XElement file = LoadConfigurationFile();

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

        public static string GetConfigFilePath()
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
                return hasValidConfig.Value;

            hasValidConfig = false;
            try
            {
                string path = GetConfigFilePath();
                if(path == null || !File.Exists(path))
                {
                    Log.Info("MediaPortal configuration file does not exists (this isn't a problem for server-only installations)");
                    return false;
                }

                LoadConfigurationFile(); // try to parse the configuration file
                hasValidConfig = true;
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn("Error trying to detect valid MediaPortal configuration file", ex);
                return false;
            }
        }

        public static Version GetBuildVersion()
        {
            var assemblyPath = GetAssemblyPath();
            return assemblyPath != null ? AssemblyName.GetAssemblyName(assemblyPath).Version : null;
        }

        public static MediaPortalVersion GetVersion()
        {
            var minimumVersions = new Dictionary<MediaPortalVersion, Version>()
            {
                { MediaPortalVersion.MP1_4, new Version(1, 3, 100) },   // 1.3.100 used by pre-release, see http://git.io/akz_PQ
                { MediaPortalVersion.MP1_3, new Version(1, 2, 100) },   // 1.2.100 used by the alpha release
                { MediaPortalVersion.MP1_2, new Version(1, 2) },        // not sure about alpha versions, but those are ancient
                { MediaPortalVersion.MP1_1, new Version(1, 1) }         // unsupported, so whatever
            };

            Version build = GetBuildVersion();
            if (build == null)
                return MediaPortalVersion.NotAvailable;

            foreach (var mpVersion in minimumVersions)
            {
                if (build >= mpVersion.Value)
                    return mpVersion.Key;
            }

            return MediaPortalVersion.Unknown;
        }

        public static void LogVersionDetails()
        {
            try
            {
                var version = GetVersion();
                if (version == MediaPortalVersion.NotAvailable)
                {
                    Log.Info("No MediaPortal installed");
                }
                else if (version >= MediaPortalVersion.MP1_2)
                {
                    Log.Debug("Found supported MediaPortal installation ({0}, build {1})", version, GetBuildVersion());
                }
                else
                {
                    Log.Warn("Installed MediaPortal version is not supported! ({0}, build {1})", version, GetBuildVersion());
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to detect MediaPortal version", ex);
            }
        }

        public static string GetMediaPortalPath()
        {
            string mpdir = Mediaportal.GetClientInstallationDirectory();
            if (mpdir == null)
            {
                mpdir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Team MediaPortal", "MediaPortal");
            }
            return Path.Combine(mpdir, "MediaPortal.exe");
        }

        public static bool IsMediaPortalRunning()
        {
            return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(GetMediaPortalPath())).Length > 0;
        }
    }
}
