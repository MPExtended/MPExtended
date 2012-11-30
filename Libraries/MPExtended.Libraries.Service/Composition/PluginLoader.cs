#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MPExtended.Libraries.Service;

namespace MPExtended.Libraries.Service.Composition
{
    public class PluginLoader
    {
        private AggregateCatalog catalog;
        private CompositionContainer container;
        private List<string> requiredMetadata;

        public PluginLoader()
        {
            catalog = new AggregateCatalog();
            requiredMetadata = new List<string>();
        }

        public void AddDirectory(string directory, string searchPattern)
        {
            if (container != null)
                throw new InvalidOperationException("Sources cannot be added after plugins have been loaded or exports are added");
            catalog.Catalogs.Add(new SafeDirectoryCatalog(directory, searchPattern));
        }

        public void AddDirectory(string directory)
        {
            if (container != null)
                throw new InvalidOperationException("Sources cannot be added after plugins have been loaded or exports are added");
            catalog.Catalogs.Add(new SafeDirectoryCatalog(directory));
        }

        public void AddFromTree(string sourceDirectory, string installedDirectory)
        {
            if (Installation.GetFileLayoutType() == FileLayoutType.Source)
            {
                AddFromSourceDirectoryList(Directory.EnumerateDirectories(Path.Combine(Installation.GetSourceRootDirectory(), sourceDirectory)), null);
            }
            else
            {
                AddDirectory(Path.Combine(Installation.GetInstallDirectory(), installedDirectory));
            }
        }

        public void AddFromTreeMatch(string sourceDirectoryGlob, string installedDirectory)
        {
            if (Installation.GetFileLayoutType() == FileLayoutType.Source)
            {
                AddFromSourceDirectoryList(Directory.EnumerateDirectories(Installation.GetSourceRootDirectory(), sourceDirectoryGlob), null);
            }
            else
            {
                AddDirectory(Path.Combine(Installation.GetInstallDirectory(), installedDirectory));
            }
        }

        public void AddFromTreeMatch(string sourceDirectoryGlob, string sourceSearchPattern, string installedDirectory)
        {
            if (Installation.GetFileLayoutType() == FileLayoutType.Source)
            {
                AddFromSourceDirectoryList(Directory.EnumerateDirectories(Installation.GetSourceRootDirectory(), sourceDirectoryGlob), sourceSearchPattern);
            }
            else
            {
                AddDirectory(Path.Combine(Installation.GetInstallDirectory(), installedDirectory));
            }
        }

        private void AddFromSourceDirectoryList(IEnumerable<string> directories, string searchPattern)
        {
            foreach (string pluginDirectory in directories)
            {
                string dir = Path.Combine(pluginDirectory, "bin", Installation.GetSourceBuildDirectoryName());
                if (Directory.Exists(dir))
                    AddDirectory(dir, searchPattern);
            }
        }

        public void AddAssembly(Assembly assembly)
        {
            if (container != null)
                throw new InvalidOperationException("Sources cannot be added after plugins have been loaded or exports are added");
            catalog.Catalogs.Add(new AssemblyCatalog(assembly));
        }

        private void CreateContainer()
        {
            if (catalog.Catalogs.Count == 0)
                throw new InvalidOperationException("No source has been added to this plugin loader");
            container = new CompositionContainer(catalog);
        }

        public void AddExport<TExport>(TExport instance)
        {
            if (container == null)
                CreateContainer();
            container.ComposeExportedValue<TExport>(instance);
        }

        public void AddRequiredMetadata(string name)
        {
            requiredMetadata.Add(name);
        }

        public IEnumerable<Plugin<TPlugin>> GetPlugins<TPlugin>()
        {
            if (container == null)
                CreateContainer();

            var plugins = new List<Plugin<TPlugin>>();

            var composedItems = container.GetExports<TPlugin, IDictionary<string, object>>();
            foreach (var item in composedItems)
            {
                try
                {
                    // This forces the Value of the Lazy object to be created, without actually doing anything. We want to
                    // create the value here, so that we can catch the exception and don't have to do error handling in the
                    // rest of the application.
                    string name = item.Value.GetType().FullName;

                    // Check for required metadata
                    if (!requiredMetadata.All(x => item.Metadata.ContainsKey(x)))
                    {
                        Log.Warn("Plugin '{0}' is missing required metadata, skipped loading this plugin");
                        continue;
                    }

                    // This plugin is fine, add it to the returned items
                    plugins.Add(new Plugin<TPlugin>(item));
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to create plugin", ex);
                }
            }

            return plugins;
        }
    }
}
