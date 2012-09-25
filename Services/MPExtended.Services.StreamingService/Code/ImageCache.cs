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
using System.IO;
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service;

namespace MPExtended.Services.StreamingService.Code
{
    /* The whole purpose of this class is to provide versioning of our imagecache directory. Old versions
     * of MPExtended have been known to write bogus entries into the cache. This makes sure we empty the
     * cache on access if it has been written by an older version of MPExtended. 
     */
    internal class ImageCache
    {
        private const int CACHE_VERSION = 3;

        private string path;

        public ImageCache()
        {
            path = Path.Combine(Installation.GetCacheDirectory(), "imagecache");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string versionFile = Path.Combine(path, "version.txt");
            int currentCacheVersion = -1;
            if (!File.Exists(versionFile) || !Int32.TryParse(File.ReadAllText(versionFile).Trim(), out currentCacheVersion) || currentCacheVersion < CACHE_VERSION)
            {
                Log.Info("Deleting invalid imagecache (version {0}), probably written by an older MPExtended version.", currentCacheVersion);
                EmptyCache();
                File.WriteAllText(versionFile, CACHE_VERSION.ToString());
            }
        }

        public void EmptyCache()
        {
            foreach (var file in Directory.GetFiles(path))
            {
                File.Delete(file);
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                Directory.Delete(dir, true);
            }
        }

        public bool Contains(string filename)
        {
            return File.Exists(GetPath(filename));
        }

        public string GetPath(string filename)
        {
            return Path.Combine(path, filename);
        }
    }
}
