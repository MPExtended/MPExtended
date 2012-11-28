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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Microsoft.Win32;
using MPExtended.Libraries.Service.Config;
using MPExtended.Libraries.Service.Hosting;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service
{
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

    public class ServiceConfiguration
    {
        public string Service { get; set; }
        public int Port { get; set; }
    }

    public static class Installation
    {
        private static List<ServiceConfiguration> installedServicesList;
        public static InstallationProperties Properties { get; internal set; }

        public static void Load(MPExtendedProduct product)
        {
            Properties = InstallationProperties.DetectForProduct(product);
        }

        public static FileLayoutType GetFileLayoutType()
        {
            return Properties.FileLayout;
        }

        public static string GetSourceRootDirectory()
        {
            if (Properties.FileLayout != FileLayoutType.Source)
                throw new InvalidOperationException("Source root directory not available for release installations");

            return Properties.SourceRoot;
        }

        public static string GetSourceBuildDirectoryName()
        {
            return Properties.SourceBuildDirectory;
        }

        public static string GetInstallDirectory()
        {
            if (Properties.FileLayout != FileLayoutType.Installed)
                throw new InvalidOperationException("Install directory not available for source installations");

            return Properties.InstallationDirectory;
        }

        public static string GetConfigurationDirectory()
        {
            return Properties.ConfigurationDirectory;
        }

        public static string GetCacheDirectory()
        {
            return EnsureDirectoryExists(Properties.CacheDirectory);
        }

        public static string GetLogDirectory()
        {
            // This one is special, as it might be called (by Log.Setup) before the properties are loaded.
            if (Properties != null)
            {
                return EnsureDirectoryExists(Properties.LogDirectory);
            }
            else
            {
                return EnsureDirectoryExists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended", "Logs"));
            }
        }

        private static string EnsureDirectoryExists(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        public static bool IsProductInstalled(MPExtendedProduct product)
        {
            if (Properties.FileLayout == FileLayoutType.Source)
                return true;

            string keyname = String.Format("{0}InstallLocation", Enum.GetName(typeof(MPExtendedProduct), product));
            object regLocation = RegistryReader.ReadKeyAllViews(RegistryHive.LocalMachine, @"Software\MPExtended", keyname);
            return regLocation != null;
        }

        public static List<ServiceConfiguration> GetInstalledServices()
        {
            if (installedServicesList != null)
                return installedServicesList;

            installedServicesList = new List<ServiceConfiguration>();
            installedServicesList.AddRange(ServiceInstallation.Instance.GetServices().Select(x => new ServiceConfiguration()
            {
                Service = (string)x.Metadata["ServiceName"],
                Port = Configuration.Services.Port
            }));
            installedServicesList.AddRange(ServiceInstallation.Instance.GetWcfServices().Select(x => new ServiceConfiguration()
            {
                Service = (string)x.Metadata["ServiceName"],
                Port = Configuration.Services.Port
            }));

            if (WifiRemote.IsInstalled)
                installedServicesList.Add(new ServiceConfiguration()
                {
                    Service = "WifiRemote",
                    Port = WifiRemote.Port
                });

            return installedServicesList;
        }

        public static bool IsServiceInstalled(string service)
        {
            return GetInstalledServices().Any(x => x.Service == service);
        }
    }
}
