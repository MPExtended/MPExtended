#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using Microsoft.Win32;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service.Config
{
    public class InstallationProperties
    {
        public MPExtendedProduct Product { get; set; }
        public FileLayoutType FileLayout { get; set; }

        public string SourceRoot { get; set; }
        public string SourceBuildDirectory { get; set; }

        public string InstallationDirectory { get; set; }

        public string ConfigurationDirectory { get; set; }
        public string DefaultConfigurationDirectory { get; set; }
        public string ConfigurationBackupDirectory { get; set; }

        public string CacheDirectory { get; set; }
        public string LogDirectory { get; set; }

        public static InstallationProperties DetectForProduct(MPExtendedProduct product)
        {
            var prop = DetectForProductAuto(product);
            prop.Product = product;
            prop.CacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended", "Cache");
            prop.LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended", "Logs");
            return prop;
        }

        private static InstallationProperties DetectForProductAuto(MPExtendedProduct product)
        {
            // Source distributions are recognized by a GlobalVersion.cs file in one of our parent directories. In that
            // case, immediately build the properties for a source type. If we can't find the file, assume we are installed.
            // This depends on people not putting a GlobalVersion.cs in a parent directory of the installation.
            string binDir = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo info = new DirectoryInfo(binDir);
            do
            {
                if (File.Exists(Path.Combine(info.FullName, "GlobalVersion.cs")))
                    return CreateForSource();
                info = info.Parent;
            } while (info != null);

            return CreateForInstallation(product);
        }

        private static InstallationProperties CreateForSource()
        {
            var prop = new InstallationProperties();
            prop.FileLayout = FileLayoutType.Source;
            
            // It's a bit tricky to find the root source directory. The assembly might be in a different directory then where its
            // source tree is (which happens with WebMP for example), so we can't use the Location of the current executing assembly. 
            // The CodeBase points to it's original location on compilation, but this doesn't work if you send files to other people 
            // (so don't do it!). Also, not everybody names the root directory MPExtended. Instead, we look for a directory with a 
            // GlobalVersion.cs file, as we use that for detecting source installation too. It's not 100% foolproof but it works 
            // good enough.
            Uri originalPath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            DirectoryInfo info = new DirectoryInfo(Path.GetDirectoryName(originalPath.LocalPath));
            do
            {
                if (File.Exists(Path.Combine(info.FullName, "GlobalVersion.cs")))
                {
                    prop.SourceRoot = info.FullName;
                    break;
                }
                info = info.Parent;
            } while (info != null);

            // Set build directory name. This needs to be updated when we introduce different configurations.
            #if DEBUG
                prop.SourceBuildDirectory = "Debug";
            #else
                prop.SourceBuildDirectory = "Release";
            #endif

            // We don't have separate configuration directories when running from source
            prop.ConfigurationDirectory = Path.Combine(prop.SourceRoot, "Config");
            prop.DefaultConfigurationDirectory = prop.ConfigurationDirectory;
            prop.ConfigurationBackupDirectory = Path.Combine(prop.ConfigurationDirectory, "Backup");
            return prop;
        }

        private static InstallationProperties CreateForInstallation(MPExtendedProduct product)
        {
            var prop = new InstallationProperties();
            prop.FileLayout = FileLayoutType.Installed;
            prop.InstallationDirectory = GetInstallDirectory(product);
            prop.DefaultConfigurationDirectory = Path.Combine(prop.InstallationDirectory, "DefaultConfig");
            prop.ConfigurationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended");
            prop.ConfigurationBackupDirectory = Path.Combine(prop.ConfigurationDirectory, "ConfigBackup");
            return prop;
        }
    
        private static string GetInstallDirectory(MPExtendedProduct product)
        {
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
                    DirectoryInfo currentDir = new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                    // If we're executing from the WebMP context, we're inside the WebMediaPortal/www/bin directory, but if we're executing from the
                    // host service context, we're directly inside the WebMediaPortal folder.
                    return currentDir.FullName.Contains(@"\www\") ? currentDir.Parent.Parent.FullName : currentDir.FullName;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
