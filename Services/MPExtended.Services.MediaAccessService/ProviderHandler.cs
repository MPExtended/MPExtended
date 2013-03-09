#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Libraries.Service.Composition;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService
{
    internal enum ProviderType
    {
        Movie,
        Music,
        TVShow,
        Filesystem,
        Picture,
        Playlist
    }

    public abstract class ProviderHandler
    {
        private static ProviderHandler Instance { get; set; }

        internal static int GetProviderId(ProviderType prof, int? providerId)
        {
            if (providerId.HasValue)
                return providerId.Value;

            return GetDefaultProvider(prof);
        }

        internal static int GetDefaultProvider(ProviderType prof)
        {
            switch (prof)
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
                case ProviderType.Playlist: // TODO: Maybe we want to set a separate default in the future?
                    return Instance.PlaylistLibraries.GetKeyByName(Configuration.Media.DefaultPlugins.Music);
                default:
                    throw new ArgumentException();
            }
        }

        protected ILibraryList<IMovieLibrary> MovieLibraries { get; set; }
        protected ILibraryList<ITVShowLibrary> TVShowLibraries { get; set; }
        protected ILibraryList<IMusicLibrary> MusicLibraries { get; set; }
        protected ILibraryList<IPictureLibrary> PictureLibraries { get; set; }
        protected ILibraryList<IFileSystemLibrary> FileSystemLibraries { get; set; }
        protected ILibraryList<IPlaylistLibrary> PlaylistLibraries { get; set; }

        private object concurrentCompositionLock;

        internal ProviderHandler()
        {
            concurrentCompositionLock = new object();
            LoadProviders();
            Instance = this;
            Configuration.Reloaded += delegate(ConfigurationFile file)
            {
                if (file == ConfigurationFile.MediaAccess)
                {
                    Log.Debug("Reloading all libraries because of changes to MediaAccess.xml");
                    LoadProviders();
                }
            };
        }

        protected void LoadProviders()
        {
            lock (concurrentCompositionLock)
            {
                var loader = new IndexedPluginLoader<int>("Id");
                loader.AddFromTreeMatch(@"PlugIns\MPExtended.PlugIns.MAS.*", @"Plugins\Media");
                loader.AddExport<IPluginData>(new PluginData());
                loader.AddRequiredMetadata("Id");
                loader.AddRequiredMetadata("Name");

                MovieLibraries = new LibraryList<IMovieLibrary>(loader.GetIndexedPlugins<IMovieLibrary>(), ProviderType.Movie);
                MusicLibraries = new LibraryList<IMusicLibrary>(loader.GetIndexedPlugins<IMusicLibrary>(), ProviderType.Music);
                TVShowLibraries = new LibraryList<ITVShowLibrary>(loader.GetIndexedPlugins<ITVShowLibrary>(), ProviderType.TVShow);
                PictureLibraries = new LibraryList<IPictureLibrary>(loader.GetIndexedPlugins<IPictureLibrary>(), ProviderType.Picture);
                FileSystemLibraries = new LibraryList<IFileSystemLibrary>(loader.GetIndexedPlugins<IFileSystemLibrary>(), ProviderType.Filesystem);
                PlaylistLibraries = new LibraryList<IPlaylistLibrary>(loader.GetIndexedPlugins<IPlaylistLibrary>(), ProviderType.Playlist);
            }
        }
    }
}
