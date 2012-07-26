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
using Microsoft.Xml.Serialization.GeneratedAssembly;

namespace MPExtended.Libraries.Service
{
    public class Configuration
    {
        private static FileSystemWatcher watcher;

        private static ConfigurationSerializer<Services, ServicesSerializer> serviceConfig = 
            new ConfigurationSerializer<Services, ServicesSerializer>("Services.xml");
        private static ConfigurationSerializer<MediaAccess, MediaAccessSerializer> mediaConfig = 
            new ConfigurationSerializer<MediaAccess, MediaAccessSerializer>("MediaAccess.xml");
        private static ConfigurationSerializer<Streaming, StreamingSerializer> streamConfig = 
            new ConfigurationSerializer<Streaming, StreamingSerializer>("Streaming.xml");
        private static ConfigurationSerializer<WebMediaPortalHosting, WebMediaPortalHostingSerializer> webmpHostingConfig = 
            new ConfigurationSerializer<WebMediaPortalHosting, WebMediaPortalHostingSerializer>("WebMediaPortalHosting.xml");
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

        public static bool Save()
        {
            // I use only one ampersand here on purpose, we don't want short-circuit here as all config files should be saved. 
            return serviceConfig.Save() & mediaConfig.Save() & streamConfig.Save() & webmpHostingConfig.Save() & webmpConfig.Save();
        }

        internal static string GetPath(string filename)
        {
            string path = Path.Combine(Installation.GetConfigurationDirectory(), filename);

            if (!File.Exists(path))
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
                MPExtendedProduct product = filename.StartsWith("WebMediaPortal") ? MPExtendedProduct.WebMediaPortal : MPExtendedProduct.Service;
                return Path.Combine(Installation.GetInstallDirectory(product), "DefaultConfig", filename);
            }
            else
            {
                return GetPath(filename);
            }
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
                if (fileName == "Services.xml") serviceConfig = null;
                if (fileName == "MediaAccess.xml") mediaConfig = null;
                if (fileName == "Streaming.xml") streamConfig = null;
                if (fileName == "WebMediaPortalHosting.xml") webmpHostingConfig = null;
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
