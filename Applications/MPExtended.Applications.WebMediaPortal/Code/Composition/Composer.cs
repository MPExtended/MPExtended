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
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using MoreLinq;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Composition;

namespace MPExtended.Applications.WebMediaPortal.Code.Composition
{
    internal class Composer
    {
        private static Composer _instance;
        public static Composer Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Composer();
                return _instance;
            }
        }

        private string rootDirectory;
        private bool compositionDone = false;
        private IEnumerable<Plugin<IController>> controllers;
        private List<string> installedSkins;
        private List<string> installedPlugins;

        public Composer()
        {
            rootDirectory = WebMediaPortalApplication.GetInstallationDirectory();
        }

        public void Compose()
        {
            var pluginLoader = new PluginLoader();

            var pluginFinder = new PluginFinder();
            installedPlugins = RegisterExtensions(pluginFinder, pluginLoader, "plugins", true);

            var skinFinder = new SkinFinder();
            installedSkins = RegisterExtensions(skinFinder, pluginLoader, "skins", false);

            controllers = pluginLoader.GetPlugins<IController>();
            compositionDone = true;
        }

        private static List<string> RegisterExtensions(ExtensionFinder finder, PluginLoader loader, string logName, bool requireBinary)
        {
            var directories = finder.GetDirectories()
                .Where(dir => !requireBinary || Directory.Exists(Path.Combine(dir, "bin")));

            if (directories.Count() > 0)
            {
                Log.Debug("Installed {0}:", logName);
                foreach (var dir in directories)
                {
					bool hasBinary = Directory.Exists(Path.Combine(dir, "bin"));
                    Log.Debug("- {0} {1}", Path.GetFileName(dir), hasBinary ? " (with binary)" : "");
                    if (hasBinary)
                        loader.AddDirectory(Path.Combine(dir, "bin"), true);
                }
            }

            return directories
                .Select(x => Path.GetFileName(x))
                .ToList();
        }

        public IEnumerable<Assembly> GetAllAssemblies()
        {
            if (!compositionDone)
                throw new InvalidOperationException("Composition needs to happen fist");

            return controllers
                .Select(x => x.Value.GetType().Assembly)
                .DistinctBy(x => x.Location.ToLower())
                .ToList();
        }

        public IEnumerable<Plugin<IController>> GetActiveControllers()
        {
            if (!compositionDone)
                throw new InvalidOperationException("Composition needs to happen first");

            var skinsDirectory = Path.Combine(rootDirectory, "Skins");
            var currentSkinDirectory = Path.Combine(rootDirectory, "Skins", Settings.ActiveSettings.Skin, "bin");

            // Because we have to load the controllers at application start, we also load the controllers from disabled
            // skins, so that the user can switch skins without having to restart. We don't want those controllers to be
            // active yet though, so we need to filter the controllers from disabled skins here. 
            return controllers
                .Where(controller => {
                    var location = controller.Value.GetType().Assembly.Location;
                    return location.StartsWith(skinsDirectory, StringComparison.OrdinalIgnoreCase)
                        ? location.StartsWith(currentSkinDirectory, StringComparison.OrdinalIgnoreCase)
                        : true;
                })
                .ToList();
        }

        public IEnumerable<string> GetInstalledSkins()
        {
            return installedSkins;
        }

        public IEnumerable<string> GetInstalledPlugins()
        {
            return installedPlugins;
        }
    }
}