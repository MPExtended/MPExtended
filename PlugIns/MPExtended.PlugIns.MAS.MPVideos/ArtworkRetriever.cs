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
using System.Linq;
using System.IO;
using System.Text;
using System.Net;
using System.Data.SQLite;
using MPExtended.Libraries.General;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.PlugIns.MAS.MPVideos
{
    internal static class ArtworkRetriever
    {
        [MergeListReader]
        public static List<WebArtworkDetailed> ArtworkReader(SQLiteDataReader reader, int idx)
        {
            string url = reader.ReadString(idx);
            var item = new WebArtworkDetailed()
            {
                Filetype = "jpg",
                Id = url.GetHashCode().ToString(),
                Offset = 0,
                Rating = 1,
                Type = WebFileType.Cover,
                Path = url
            };
            return new List<WebArtworkDetailed>() { item };
        }

        public static WebFileInfo GetFileInfo(string url)
        {
            Uri fullUri = new Uri(url);
            string path = DownloadFile(url);
            FileInfo info = new FileInfo(path);
            return new WebFileInfo()
            {
                Exists = true,
                Extension = info.Extension,
                IsLocalFile = false,
                IsReadOnly = true,
                LastAccessTime = DateTime.Now,
                LastModifiedTime = info.LastWriteTime,
                Name = Path.GetFileName(fullUri.LocalPath),
                Path = path,
                Size = info.Length,
                OnNetworkDrive = false
            };
        }

        public static Stream GetStream(string url)
        {
            string path = DownloadFile(url);
            return File.OpenRead(path);
        }

        private static string DownloadFile(string url)
        {
            string tmpPath = Path.Combine(Path.GetTempPath(), "MPExtended", "imagecache", String.Format("cover_{0}.jpg", url.GetHashCode().ToString()));
            if (File.Exists(tmpPath))
            {
                return tmpPath;
            }

            // create directory if unavailable
            if (!Directory.Exists(Path.GetDirectoryName(tmpPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(tmpPath));
            }

            // download the image
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(url, tmpPath);
                    return tmpPath;
                }
                catch (Exception ex)
                {
                    Log.Info(String.Format("Failed to download {0}", url), ex);
                    return null;
                }
            }
        }
    }
}
