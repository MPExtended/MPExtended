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
                string keyPath = Environment.Is64BitOperatingSystem ?
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal" :
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal";
                RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
                if (key == null)
                {
                    Log.Warn("Could not find MediaPortal installation path key in registry, is MediaPortal installed?");
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
            // read from MediaPortalDirs.xml
            string mpDirs = Path.Combine(GetClientInstallationDirectory(), "MediaPortalDirs.xml");
            if (!File.Exists(mpDirs))
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
            if(!Path.IsPathRooted(path))
            {
                path = Path.Combine(GetClientInstallationDirectory(), path);
            }

            // and return it
            return Path.GetFullPath(path);
        }

        public static Dictionary<string, string> ReadSectionFromConfigFile(string sectionName)
        {
            string path = Path.Combine(GetLocation(MediaportalDirectory.Config), "MediaPortal.xml");
            XElement file = XElement.Load(path);

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
    }
}
