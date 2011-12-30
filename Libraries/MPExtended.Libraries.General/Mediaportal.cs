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
using System.IO;
using System.Xml.Linq;
using Microsoft.Win32;

namespace MPExtended.Libraries.General
{
    public enum MediaportalDirectory
    {
        Config,
        Database,
        Thumbs,
        Cache
    }

    public static class Mediaportal
    {
        public static string GetClientInstallationDirectory()
        {
            try
            {
                // I can't make any sense of which one is used when, so just always try both keys.
                string[] keys = new string[] {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal",
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
                };

                RegistryKey key = null;
                foreach (var keyPath in keys)
                {
                    key = Registry.LocalMachine.OpenSubKey(keyPath);
                    if (key != null)
                    {
                        break;
                    }
                }

                if (key == null)
                {
                    Log.Info("Could not find MediaPortal client installation path key in registry, is MediaPortal client installed?");
                    return null;
                }

                object value = key.GetValue("InstallPath", null);
                if (value == null)
                {
                    Log.Warn("Could not find InstallPath property, is MediaPortal corrupt?");
                    return null;
                }

                return value.ToString();
            }
            catch (Exception ex)
            {
                Log.Error("Exception in Mediaportal.GetClientInstallationDirectory()", ex);
                return null;
            }
        }

        public static string GetLocation(MediaportalDirectory type)
        {
            try
            {
                // read from MediaPortalDirs.xml
                string clientInstallDir = GetClientInstallationDirectory();
                string mpDirs = clientInstallDir == null ? null : Path.Combine(clientInstallDir, "MediaPortalDirs.xml");
                if (mpDirs == null || !File.Exists(mpDirs))
                {
                    Log.Warn("Could not find MediaPortalDirs.xml");
                    return null;
                }

                XElement file = XElement.Load(mpDirs);
                var element = file.Elements("Dir").Where(x => x.Attribute("id").Value == type.ToString());
                if (element.Count() == 0)
                {
                    Log.Warn("Could not find directory with id {0} in MediaPortalDirs.xml", type);
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
                if (!File.Exists(GetConfigFilePath()))
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
            try
            {
                string path = GetConfigFilePath();
                if(path == null)
                {
                    return false;
                }

                var xml = XElement.Load(path); // try to parse it
                return true;
            }
            catch (Exception)
            {
                Log.Warn("No valid MediaPortal config file detected");
                return false;
            }
        }
    }
}
