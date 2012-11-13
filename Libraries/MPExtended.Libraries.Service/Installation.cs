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
    public enum MPExtendedService
    {
        MediaAccessService,
        TVAccessService,
        StreamingService,
        UserSessionService,
        MetaService,
        WifiRemote,
        ScraperService
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

    public class ServiceConfiguration
    {
        public MPExtendedService Service { get; set; }
        public int Port { get; set; }
    }

    public static class Installation
    {
        internal static List<ServiceAssemblyAttribute> installedServices;
        private static List<ServiceConfiguration> installedServicesReturnList;
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

        internal static List<ServiceAssemblyAttribute> GetAvailableServices()
        {
            if (installedServices == null)
            {
                if (Properties.Product != MPExtendedProduct.Service)
                    throw new InvalidOperationException("GetAvailableServices() can only be called from the services");

                if (GetFileLayoutType() == FileLayoutType.Installed)
                {
                    installedServices = Directory.GetFiles(Properties.InstallationDirectory, "MPExtended.Services.*.dll")
                        .Select(path => Assembly.LoadFrom(path))
                        .SelectMany(asm => asm.GetCustomAttributes(typeof(ServiceAssemblyAttribute), false).Cast<ServiceAssemblyAttribute>())
                        .ToList();
                }
                else
                {
                    // Loading the assemblies in a mix-and-match style from different directories doesn't work and gives all kind of 
                    // weird errors, such as MethodMissingException in some classes from an assembly. So we now prefer to load all assemblies
                    // from the directory where the current assembly runs, and fallback to the Services directory only if some services aren't
                    // available in our own directory. This makes those services unstable, but that should only happen in non-hosting processes
                    // such as the configurator. The configurator does load USS from it's own directory (it has a reference) so the only things
                    // that don't work are MAS, TAS and WSS but those aren't used there anyway. However, it does need to know whether they are
                    // available because it configures the display of tabs based upon the installed services. This isn't relevant for installed
                    // services, because everything is loaded from the same directory there anyway. 

                    var myDir = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MPExtended.Services.*.dll")
                        .Where(x => !x.Contains(".Interfaces.dll"));
                    var myFileNames = myDir.Select(x => Path.GetFileName(x));
                    var serviceFiles = Directory.GetDirectories(Path.Combine(GetSourceRootDirectory(), "Services"))
                        .Select(x => Path.Combine(x, "bin", GetSourceBuildDirectoryName()))
                        .SelectMany(x => Directory.GetFiles(x, "MPExtended.Services.*.dll"))
                        .Where(x => !x.Contains(".Interfaces.dll") && !myFileNames.Contains(Path.GetFileName(x)))
                        .GroupBy(x => Path.GetFileName(x), (x, y) => y.First());

                    installedServices = myDir.Concat(serviceFiles)
                        .Select(path => Assembly.LoadFrom(path))
                        .SelectMany(asm => asm.GetCustomAttributes(typeof(ServiceAssemblyAttribute), false).Cast<ServiceAssemblyAttribute>())
                        .ToList();
                }
            }

            return installedServices;
        }

        public static List<ServiceConfiguration> GetInstalledServices()
        {
            if (installedServicesReturnList != null)
                return installedServicesReturnList;

            installedServicesReturnList = GetAvailableServices().Select(x => new ServiceConfiguration()
            {
                Port = Configuration.Services.Port,
                Service = x.Service,
            }).ToList();

            if (WifiRemote.IsInstalled)
            {
                installedServicesReturnList.Add(new ServiceConfiguration()
                {
                    Port = WifiRemote.Port,
                    Service = MPExtendedService.WifiRemote,
                });
            }

            return installedServicesReturnList;
        }

        public static bool IsServiceInstalled(MPExtendedService srv)
        {
            return GetInstalledServices().Any(x => x.Service == srv);
        }
    }
}
