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

        private static ConfigurationSerializer<Services, ServicesSerializer, ServicesUpgrader> serviceConfig;
        private static ConfigurationSerializer<MediaAccess, MediaAccessSerializer, MediaAccessUpgrader> mediaConfig;
        private static ConfigurationSerializer<Streaming, StreamingSerializer, StreamingUpgrader> streamConfig;
        private static ConfigurationSerializer<WebMediaPortalHosting, WebMediaPortalHostingSerializer, WebMediaPortalHostingUpgrader> webmpHostingConfig;
        private static ConfigurationSerializer<WebMediaPortal, WebMediaPortalSerializer> webmpConfig;
        private static ConfigurationSerializer<Authentication, AuthenticationSerializer, AuthenticationUpgrader> authenticationConfig;

        static Configuration()
        {
            TransformationCallbacks.Install();
            Reset();
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

        public static Authentication Authentication
        {
            get
            {
                return authenticationConfig.Get();
            }
        }

        public static void Reset()
        {
            serviceConfig = new ConfigurationSerializer<Services, ServicesSerializer, ServicesUpgrader>("Services.xml");
            mediaConfig = new ConfigurationSerializer<MediaAccess, MediaAccessSerializer, MediaAccessUpgrader>("MediaAccess.xml");
            streamConfig = new ConfigurationSerializer<Streaming, StreamingSerializer, StreamingUpgrader>("Streaming.xml");
            webmpHostingConfig = new ConfigurationSerializer<WebMediaPortalHosting, WebMediaPortalHostingSerializer, WebMediaPortalHostingUpgrader>("WebMediaPortalHosting.xml");
            webmpConfig = new ConfigurationSerializer<WebMediaPortal, WebMediaPortalSerializer>("WebMediaPortal.xml");
            authenticationConfig = new ConfigurationSerializer<Authentication, AuthenticationSerializer, AuthenticationUpgrader>("Authentication.xml", "Services.xml");
        }

        public static void Load()
        {
            authenticationConfig.LoadIfExists();
            serviceConfig.LoadIfExists();
            mediaConfig.LoadIfExists();
            streamConfig.LoadIfExists();
            webmpConfig.LoadIfExists();
            webmpHostingConfig.LoadIfExists();
        }

        public static bool Save()
        {
            // I use only one ampersand here on purpose: we don't want short-circuit as all config files should be saved. 
            return authenticationConfig.Save() & serviceConfig.Save() & mediaConfig.Save() & streamConfig.Save() & webmpHostingConfig.Save() & webmpConfig.Save();
        }

        public static void EnableChangeWatching()
        {
            if (watcher != null)
            {
                return;
            }

            watcher = new FileSystemWatcher(Installation.Properties.ConfigurationDirectory, "*.xml");
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += new FileSystemEventHandler(delegate(object sender, FileSystemEventArgs e)
            {
                string fileName = Path.GetFileName(e.FullPath);
                if (fileName == "Users.xml") authenticationConfig.Reload();
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
