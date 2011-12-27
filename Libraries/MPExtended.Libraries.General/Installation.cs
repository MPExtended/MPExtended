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
using System.Reflection;
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

    public enum MPExtendedProduct
    {
        Service,
        WebMediaPortal
    }

    public enum FileLayoutType
    {
        Source,
        Installed
    }

    public static class Installation
    {
        public static FileLayoutType GetFileLayoutType()
        {
            string binDir = AppDomain.CurrentDomain.BaseDirectory;
            string dirName = Path.GetFileName(Path.GetDirectoryName(binDir));
            if (dirName == "Debug" || dirName == "Release")
            {
                return FileLayoutType.Source;
            }
            else
            {
                return FileLayoutType.Installed;
            }
        }

        public static string GetSourceRootDirectory()
        {
            if (GetFileLayoutType() != FileLayoutType.Source)
            {
                throw new InvalidOperationException("Source root directory not available for release installations");
            }

            // It's a bit tricky to find the root source directory for Debug builds. The assembly might be in a different
            // directory then where it's source tree is (which happens with WebMP for example), so we can't use the Location
            // of the current executing assembly. The CodeBase points to it's original location on compilation, but this 
            // doesn't work if you send files to other people (so don't do it!).
            // Also, not everybody names the root directory MPExtended. Instead, we look for a directory with a child directory
            // named Config, which has a Debug child directory. It's not 100% foolproof but it works good enough.
            Uri originalPath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            DirectoryInfo info = new DirectoryInfo(Path.GetDirectoryName(originalPath.LocalPath));
            do
            {
                if (Directory.Exists(Path.Combine(info.FullName, "Config")) && Directory.Exists(Path.Combine(info.FullName, "Config", "Debug")))
                {
                    return info.FullName;
                }
                info = info.Parent;
            } while (info != null);

            string curDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.GetFullPath(Path.Combine(curDir, "..", "..", "..", ".."));
        }

        public static string GetInstallDirectory(MPExtendedProduct product)
        {
            if (GetFileLayoutType() != FileLayoutType.Installed)
            {
                throw new InvalidOperationException("Install directory not available for source installations");
            }

            // If possible, try to read it from the registry, where the install location is set during installation. 
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\MPExtended");
            if (key != null)
            {
                object value = key.GetValue(String.Format("{0}InstallLocation", Enum.GetName(typeof(MPExtendedProduct), product)));
                if (value != null)
                {
                    return value.ToString();
                }
            }

            // Fallback to dynamic detection based upon the default install location of the services
            switch(product)
            {
                case MPExtendedProduct.Service:
                    return Path.GetFullPath(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                case MPExtendedProduct.WebMediaPortal:
                    DirectoryInfo webmpinfo = new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                    return webmpinfo.Parent.FullName;
                default:
                    throw new ArgumentException();
            }
        }

        public static string GetConfigurationDirectory()
        {
            if (GetFileLayoutType() == FileLayoutType.Source)
            {
                return Path.Combine(GetSourceRootDirectory(), "Config", "Debug");
            }
            else
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended");
            }
        }

        public static string GetLogDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended", "Logs");
        }

        public static List<Service> GetInstalledServices()
        {
            List<Service> allServices = new List<Service>()
            {
                new Service(MPExtendedService.MediaAccessService, "MPExtended.Services.MediaAccessService", "MediaAccessService", "_mpextended-mas._tcp"),
                new Service(MPExtendedService.TVAccessService, "MPExtended.Services.TVAccessService", "TVAccessService", "_mpextended-tas._tcp"),
                new Service(MPExtendedService.StreamingService, "MPExtended.Services.StreamingService", "StreamingService", "_mpextended-wss._tcp"),
                new Service(MPExtendedService.UserSessionService, "MPExtended.Services.UserSessionService", "UserSessionProxyService", "_mpextended-uss._tcp"),
                new WifiRemoteService()
            };

            string[] disabled =
                XElement.Load(Configuration.GetPath("Services.xml"))
                .Element("disabledServices")
                .Elements("service")
                .Select(x => x.Value)
                .ToArray();

            return allServices.Where(x => x.IsInstalled && !disabled.Contains(x.Assembly)).ToList();
        }

        public static bool IsServiceInstalled(MPExtendedService srv)
        {
            return GetInstalledServices().Any(x => x.ServiceName == srv);
        }
    }
}
