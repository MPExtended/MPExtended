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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Win32;

namespace MPExtended.Libraries.General
{
    public enum MPExtendedService
    {
        MediaAccessService,
        TVAccessService,
        StreamingService,
        UserSessionService,
        WifiRemote
    }

    public static class Installation
    {
        public static bool CheckInstalled(MPExtendedService service)
        {
            if (service == MPExtendedService.WifiRemote)
            {
                // TODO: FIXME
                return false;
            }

#if DEBUG
            return true;
#else
            return CheckRegistryKey(Registry.LocalMachine, @"Software\MPExtended", service.ToString() + "Installed");
#endif
        }

        public static string GetRootDirectory()
        {
            string curDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
#if DEBUG
            return Path.GetFullPath(Path.Combine(curDir, "..", "..", "..", ".."));
#else
            return Path.GetFullPath(curDir);
#endif
        }

        public static string GetConfigurationDirectory()
        {
#if DEBUG
            return Path.Combine(GetRootDirectory(), "Config", "Debug");
#else
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended");
#endif
        }

        public static string GetLogDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended", "Logs");
        }

        private static bool CheckRegistryKey(RegistryKey reg, string key, string name)
        {
            RegistryKey regkey = reg.OpenSubKey(key);
            if (regkey == null)
            {
                return false;
            }

            object value = regkey.GetValue(name);
            if (value == null)
            {
                return false;
            }

            return value.ToString() == "true";
        }

        public static List<Service> GetInstalledServices()
        {
            List<Service> allServices = new List<Service>()
            {
                new Service(MPExtendedService.MediaAccessService, "MPExtended.Services.MediaAccessService", "MediaAccessService", "_mpextended-mas._tcp"),
                new Service(MPExtendedService.TVAccessService, "MPExtended.Services.TVAccessService", "TVAccessService", "_mpextended-tas._tcp"),
                new Service(MPExtendedService.StreamingService, "MPExtended.Services.StreamingService", "StreamingService", "_mpextended-wss._tcp"),
                new Service(MPExtendedService.UserSessionService, "MPExtended.Services.UserSessionService", "UserSessionProxyService", "_mpextended-uss._tcp")
            };

            string[] disabled =
                XElement.Load(Configuration.GetPath("Services.xml"))
                .Element("disabledServices")
                .Elements("service")
                .Select(x => x.Value)
                .ToArray();

            return allServices.Where(x => x.IsInstalled && !disabled.Contains(x.Assembly)).ToList();
        }
    }
}
