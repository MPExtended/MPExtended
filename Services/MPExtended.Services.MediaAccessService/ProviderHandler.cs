#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService
{
    internal enum ProviderType
    {
        Movie,
        Music,
        TVShow,
        Filesystem,
        Picture
    }

    public abstract class ProviderHandler
    {
        #region MEF part
        [ImportMany]
        private Lazy<IMovieLibrary, IDictionary<string, object>>[] MovieLibrariesLoaded { get; set; }
        [ImportMany]
        private Lazy<ITVShowLibrary, IDictionary<string, object>>[] TVShowLibrariesLoaded { get; set; }
        [ImportMany]
        private Lazy<IPictureLibrary, IDictionary<string, object>>[] PictureLibrariesLoaded { get; set; }
        [ImportMany]
        private Lazy<IMusicLibrary, IDictionary<string, object>>[] MusicLibrariesLoaded { get; set; }
        [ImportMany]
        private Lazy<IFileSystemLibrary, IDictionary<string, object>>[] FileSystemLibrariesLoaded { get; set; }

        protected ILibraryList<IMovieLibrary> MovieLibraries { get; set; }
        protected ILibraryList<ITVShowLibrary> TVShowLibraries { get; set; }
        protected ILibraryList<IMusicLibrary> MusicLibraries { get; set; }
        protected ILibraryList<IPictureLibrary> PictureLibraries { get; set; }
        protected ILibraryList<IFileSystemLibrary> FileSystemLibraries { get; set; }

        internal ProviderHandler()
        {
            if (!Compose())
            {
                return;
            }

            try
            {
                MovieLibraries = new LazyLibraryList<IMovieLibrary>(FilterDisabled(MovieLibrariesLoaded).ToDictionary(x => (int)x.Metadata["Id"], x => x), ProviderType.Movie);
                MusicLibraries = new LazyLibraryList<IMusicLibrary>(FilterDisabled(MusicLibrariesLoaded).ToDictionary(x => (int)x.Metadata["Id"], x => x), ProviderType.Music);
                TVShowLibraries = new LazyLibraryList<ITVShowLibrary>(FilterDisabled(TVShowLibrariesLoaded).ToDictionary(x => (int)x.Metadata["Id"], x => x), ProviderType.TVShow);
                PictureLibraries = new LazyLibraryList<IPictureLibrary>(FilterDisabled(PictureLibrariesLoaded).ToDictionary(x => (int)x.Metadata["Id"], x => x), ProviderType.Picture);
                FileSystemLibraries = new LazyLibraryList<IFileSystemLibrary>(FilterDisabled(FileSystemLibrariesLoaded).ToDictionary(x => (int)x.Metadata["Id"], x => x), ProviderType.Filesystem);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to load MAS backends", ex);
            }

            Instance = this;
        }

        private bool Compose()
        {
            try
            {
                AggregateCatalog catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
#if DEBUG
                string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string pluginRoot = Path.Combine(Installation.GetRootDirectory(), "PlugIns");
                foreach (string pdir in Directory.GetDirectories(pluginRoot))
                {
                    string dir = Path.GetFullPath(Path.Combine(pluginRoot, pdir, "bin", "Debug"));
                    if (Directory.Exists(dir))
                        catalog.Catalogs.Add(new DirectoryCatalog(dir));
                }
#else
                string extensionDirectory = Path.GetFullPath(Path.Combine(Installation.GetRootDirectory(), "Service", "Extensions"));
                catalog.Catalogs.Add(new DirectoryCatalog(extensionDirectory));
#endif

                CompositionContainer container = new CompositionContainer(catalog);
                container.ComposeParts(this);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to create MEF service", ex);
                return false;
            }
        }

        private IEnumerable<Lazy<T, IDictionary<string, object>>> FilterDisabled<T>(Lazy<T, IDictionary<string, object>>[] list) 
        {
            return list.Where(x => !Configuration.Media.DisabledPlugins.Contains((string)x.Metadata["Name"]));
        }
        #endregion

        #region Static part
        private static ProviderHandler Instance { get; set; }

        internal static int GetProviderId(ProviderType prof, int? providerId)
        {
            if (providerId.HasValue)
            {
                return providerId.Value;
            }

            return GetDefaultProvider(prof);
        }

        internal static int GetDefaultProvider(ProviderType prof)
        {
            switch(prof) 
            {
                case ProviderType.Filesystem:
                    return Instance.FileSystemLibraries.GetKeyByName(Configuration.Media.DefaultPlugins.Filesystem);
                case ProviderType.Movie:
                    return Instance.MovieLibraries.GetKeyByName(Configuration.Media.DefaultPlugins.Movie);
                case ProviderType.Music:
                    return Instance.MusicLibraries.GetKeyByName(Configuration.Media.DefaultPlugins.Music);
                case ProviderType.Picture:
                    return Instance.PictureLibraries.GetKeyByName(Configuration.Media.DefaultPlugins.Picture);
                case ProviderType.TVShow:
                    return Instance.TVShowLibraries.GetKeyByName(Configuration.Media.DefaultPlugins.TVShow);
                default:
                    throw new ArgumentException();
            }
        }
        #endregion
    }
}
