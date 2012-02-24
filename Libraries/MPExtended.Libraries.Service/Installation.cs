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
        MetaService
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
        private static List<ServiceAssemblyAttribute> installedServices;
        private static FileLayoutType? fileLayoutType;

        public static FileLayoutType GetFileLayoutType()
        {
            if(fileLayoutType.HasValue)
            {
                return fileLayoutType.Value;
            }

            // Default to binary installation as we don't have to recognize that
            fileLayoutType = FileLayoutType.Installed;

            // Source distribution: search for a parent directory with the GlobalVersion.cs file
            string binDir = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo info = new DirectoryInfo(binDir);
            do
            {
                if (File.Exists(Path.Combine(info.FullName, "GlobalVersion.cs")))
                {
                    fileLayoutType = FileLayoutType.Source;
                    break;
                }
                info = info.Parent;
            } while (info != null);

            // Return
            return fileLayoutType.Value;
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
                if (File.Exists(Path.Combine(info.FullName, "GlobalVersion.cs")))
                {
                    return info.FullName;
                }
                info = info.Parent;
            } while (info != null);

            string curDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.GetFullPath(Path.Combine(curDir, "..", "..", "..", ".."));
        }

        public static string GetSourceBuildDirectoryName()
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }

        public static string GetInstallDirectory(MPExtendedProduct product)
        {
            if (GetFileLayoutType() != FileLayoutType.Installed)
            {
                throw new InvalidOperationException("Install directory not available for source installations");
            }

            // If possible, try to read it from the registry, where the install location is set during installation. 
            string keyname = String.Format("{0}InstallLocation", Enum.GetName(typeof(MPExtendedProduct), product));
            object regLocation = RegistryReader.ReadKeyAllViews(RegistryHive.LocalMachine, @"Software\MPExtended", keyname);
            if (regLocation != null)
            {
                return regLocation.ToString();
            }

            // try default installation location
            string location = null;
            switch (product)
            {
                case MPExtendedProduct.Service:
                    location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "MPExtended", "Service");
                    break;
                case MPExtendedProduct.WebMediaPortal:
                    location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "MPExtended", "WebMediaPortal");
                    break;
            }
            if (Directory.Exists(location))
            {
                return location;
            }

            // Fallback to dynamic detection based upon the current execution location
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

        public static bool IsProductInstalled(MPExtendedProduct product)
        {
            if (GetFileLayoutType() == FileLayoutType.Source)
            {
                return true;
            }
            else
            {
                string keyname = String.Format("{0}InstallLocation", Enum.GetName(typeof(MPExtendedProduct), product));
                object regLocation = RegistryReader.ReadKeyAllViews(RegistryHive.LocalMachine, @"Software\MPExtended", keyname);
                return regLocation != null;
            }
        }

        public static string GetConfigurationDirectory()
        {
            if (GetFileLayoutType() == FileLayoutType.Source)
            {
                return Path.Combine(GetSourceRootDirectory(), "Config");
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

        internal static bool IsDebugBuild()
        {
            var attrs = (AssemblyConfigurationAttribute[])Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            if (attrs.Length > 0)
            {
                return attrs.First().Configuration == "Debug";
            }
            return false;
        }

        internal static List<ServiceAssemblyAttribute> GetAvailableServices()
        {
            if (installedServices == null)
            {
                if (GetFileLayoutType() == FileLayoutType.Installed)
                {
                    installedServices = Directory.GetFiles(GetInstallDirectory(MPExtendedProduct.Service), "MPExtended.Services.*.dll")
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
                        .Select(x => Path.Combine(x, "bin", IsDebugBuild() ? "Debug" : "Release"))
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
            return GetAvailableServices().Select(x => new ServiceConfiguration()
            {
                Port = Configuration.Services.Port,
                Service = x.Service,
            }).ToList();
        }

        public static bool IsServiceInstalled(MPExtendedService srv)
        {
            return GetInstalledServices().Any(x => x.Service == srv);
        }
    }
}
