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
    public enum ConfigurationFile
    {
        Services = 1,
        MediaAccess = 2,
        Streaming = 3,
        Authentication = 4,
        WebMediaPortal = 5,
        WebMediaPortalHosting = 6
    }

    public class Configuration
    {
        public const int DEFAULT_PORT = 4322;

        public delegate void ConfigurationReloadedEventHandler();
        public static event ConfigurationReloadedEventHandler Reloaded;

        private static FileSystemWatcher watcher;
        private static Dictionary<string, long> reloadedFiles = new Dictionary<string, long>();
        private static ConfigurationList config;

        static Configuration()
        {
            TransformationCallbacks.Install();
            config = new ConfigurationList();
        }

        public static Authentication Authentication
        {
            get
            {
                return config.Get<Authentication>(ConfigurationFile.Authentication).Get();
            }
        }

        public static Config.Services Services
        {
            get
            {
                return config.Get<Config.Services>(ConfigurationFile.Services).Get();
            }
        }

        public static MediaAccess Media
        {
            get
            {
                return config.Get<MediaAccess>(ConfigurationFile.MediaAccess).Get();
            }
        }

        public static Streaming Streaming
        {
            get
            {
                return config.Get<Streaming>(ConfigurationFile.Streaming).Get();
            }
        }

        public static WebMediaPortalHosting WebMediaPortalHosting
        {
            get
            {
                return config.Get<WebMediaPortalHosting>(ConfigurationFile.WebMediaPortalHosting).Get();
            }
        }

        public static WebMediaPortal WebMediaPortal
        {
            get
            {
                return config.Get<WebMediaPortal>(ConfigurationFile.WebMediaPortal).Get();
            }
        }

        public static void Reset()
        {
            config = new ConfigurationList();
        }

        public static void Load()
        {
            config.ForEach(c => c.LoadIfExists());
        }

        public static bool Save()
        {
            return config.ForEach(c => c.Save()).All(c => c.Value);
        }

        public static IConfigurationSerializer GetSerializer(ConfigurationFile file)
        {
            return config[file];
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
                // This isn't strictly required, but makes sure we only reloaded the configuration once when it has been changed.
                long stamp = File.GetLastWriteTime(e.FullPath).Ticks;
                if (reloadedFiles.ContainsKey(e.Name) && reloadedFiles[e.Name] >= stamp)
                    return;

                reloadedFiles[e.Name] = stamp;
                var serializer = config.Select(s => s.Value).FirstOrDefault(s => s.Filename == e.Name);
                if (serializer != null)
                {
                    Log.Debug("Reloading configuration file '{0}' due to changes.", serializer.Filename);
                    serializer.Reload();
                    if (Reloaded != null)
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
