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
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.Service.Config;
using MPExtended.Libraries.Service.Config.Upgrade;
using Microsoft.Xml.Serialization.GeneratedAssembly;

namespace MPExtended.Libraries.Service
{
    public class Configuration
    {
        public const int DEFAULT_PORT = 4322;

        public delegate void ConfigurationReloadedEventHandler();
        public static event ConfigurationReloadedEventHandler Reloaded;

        private static FileSystemWatcher watcher;

        private static ConfigurationSerializer<Services, ServicesSerializer, ServicesUpgrader> serviceConfig =
            new ConfigurationSerializer<Services, ServicesSerializer, ServicesUpgrader>("Services.xml");
        private static ConfigurationSerializer<MediaAccess, MediaAccessSerializer, MediaAccessUpgrader> mediaConfig = 
            new ConfigurationSerializer<MediaAccess, MediaAccessSerializer, MediaAccessUpgrader>("MediaAccess.xml");
        private static ConfigurationSerializer<Streaming, StreamingSerializer, StreamingUpgrader> streamConfig = 
            new ConfigurationSerializer<Streaming, StreamingSerializer, StreamingUpgrader>("Streaming.xml");
        private static ConfigurationSerializer<WebMediaPortalHosting, WebMediaPortalHostingSerializer, WebMediaPortalHostingUpgrader> webmpHostingConfig = 
            new ConfigurationSerializer<WebMediaPortalHosting, WebMediaPortalHostingSerializer, WebMediaPortalHostingUpgrader>("WebMediaPortalHosting.xml");
        private static ConfigurationSerializer<WebMediaPortal, WebMediaPortalSerializer> webmpConfig = 
            new ConfigurationSerializer<WebMediaPortal, WebMediaPortalSerializer>("WebMediaPortal.xml");

        static Configuration()
        {
            TransformationCallbacks.Install();
        }

        public static Services Services
        {
            get
            {
                return serviceConfig.Get();
            }
        }

        public static MediaAccess Media
        {
            get
            {
                return mediaConfig.Get();
            }
        }

        public static Streaming Streaming
        {
            get
            {
                return streamConfig.Get();
            }
        }

        public static WebMediaPortalHosting WebMediaPortalHosting
        {
            get
            {
                return webmpHostingConfig.Get();
            }
        }

        public static WebMediaPortal WebMediaPortal
        {
            get
            {
                return webmpConfig.Get();
            }
        }

        public static void Load()
        {
            serviceConfig.LoadIfExists();
            mediaConfig.LoadIfExists();
            streamConfig.LoadIfExists();
            webmpConfig.LoadIfExists();
            webmpHostingConfig.LoadIfExists();
        }

        public static bool Save()
        {
            // I use only one ampersand here on purpose: we don't want short-circuit as all config files should be saved. 
            return serviceConfig.Save() & mediaConfig.Save() & streamConfig.Save() & webmpHostingConfig.Save() & webmpConfig.Save();
        }

        internal static string GetPath(string filename)
        {
            string path = Path.Combine(Installation.GetConfigurationDirectory(), filename);

            if (!File.Exists(path) && Installation.IsProductInstalled(GetProductForFile(filename)))
            {
                if (Installation.GetFileLayoutType() == FileLayoutType.Source)
                {
                    // When running from source they should exists
                    throw new FileNotFoundException("Couldn't find config - what did you do?!?!");
                }
                else
                {
                    // copy from default location
                    File.Copy(GetDefaultPath(filename), path);

                    // allow everyone to write to the config
                    var acl = File.GetAccessControl(path);
                    SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                    FileSystemAccessRule rule = new FileSystemAccessRule(everyone, FileSystemRights.FullControl, AccessControlType.Allow);
                    acl.AddAccessRule(rule);
                    File.SetAccessControl(path, acl);
                }
            }

            return path;
        }

        internal static string GetDefaultPath(string filename)
        {
            if (Installation.GetFileLayoutType() == FileLayoutType.Installed)
            {
                return Path.Combine(Installation.GetInstallDirectory(GetProductForFile(filename)), "DefaultConfig", filename);
            }
            else
            {
                return GetPath(filename);
            }
        }

        private static MPExtendedProduct GetProductForFile(string filename)
        {
            return filename.StartsWith("WebMediaPortal") ? MPExtendedProduct.WebMediaPortal : MPExtendedProduct.Service;
        }

        public static void EnableChangeWatching()
        {
            if (watcher != null)
            {
                return;
            }

            watcher = new FileSystemWatcher(Installation.GetConfigurationDirectory(), "*.xml");
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += new FileSystemEventHandler(delegate(object sender, FileSystemEventArgs e)
            {
                string fileName = Path.GetFileName(e.FullPath);
                if (fileName == "Services.xml") serviceConfig.Reload();
                if (fileName == "MediaAccess.xml") mediaConfig.Reload();
                if (fileName == "Streaming.xml") streamConfig.Reload();
                if (fileName == "WebMediaPortal.xml") webmpConfig.Reload();
                if (fileName == "WebMediaPortalHosting.xml") webmpHostingConfig.Reload();

                if (Reloaded != null)
                {
                    Reloaded();
                }
            });

            // start watching
            watcher.EnableRaisingEvents = true;
        }

        public static void DisableChangeWatching()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher = null;
            }
        }
    }
}
