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
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;

namespace MPExtended.PlugIns.MAS.LocalFileSystem
{
    [Export(typeof(IFileSystemLibrary))]
    [ExportMetadata("Name", "LocalFileSystem")]
    [ExportMetadata("Id", 2)]
    public class LocalFileSystem : IFileSystemLibrary
    {
        public bool Supported { get { return true; } }

        public IEnumerable<WebDriveBasic> GetDriveListing()
        {
            return DriveInfo.GetDrives().Select(x => new WebDriveBasic()
            {
                Id = EncodeTo64(x.RootDirectory.Name),
                Title = x.Name,
                Path = new List<string>() { x.RootDirectory.FullName },
                LastAccessTime = DateTime.Now,
                LastModifiedTime = DateTime.Now
            });
        }

        public IEnumerable<WebFileBasic> GetFilesListing(string id)
        {
            string path = DecodeFrom64(id);
            if (!String.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                return new DirectoryInfo(path).GetFiles().Select(file => ConvertFileInfoToFileBasic(file));
            }

            return new List<WebFileBasic>();
        }

        public IEnumerable<WebFolderBasic> GetFoldersListing(string id)
        {
            string path = DecodeFrom64(id);
            if (!String.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                return new DirectoryInfo(path).GetDirectories().Select(dir => new WebFolderBasic()
                {
                    Title = dir.Name,
                    Path = new List<string>() { dir.FullName },
                    DateAdded = dir.CreationTime,
                    Id = EncodeTo64(dir.FullName),
                    LastAccessTime = dir.LastAccessTime,
                    LastModifiedTime = dir.LastWriteTime
                });
            }

            return new List<WebFolderBasic>();
        }

        public WebDriveBasic GetDriveBasic(string id)
        {
            return GetDriveListing().First(x => x.Id == id);
        }

        public WebFolderBasic GetFolderBasic(string id)
        {
            string path = DecodeFrom64(id);
            if (!Directory.Exists(id))
                return null;
            return ConvertDirectoryInfoToFolderBasic(new DirectoryInfo(path));
        }

        public WebFileBasic GetFileBasic(string id)
        {
            string path = DecodeFrom64(id);
            if (!File.Exists(path))
                return null;
            return ConvertFileInfoToFileBasic(new FileInfo(PathUtil.StripFileProtocolPrefix(path)));
        }

        public IEnumerable<WebSearchResult> Search(string text)
        {
            return new List<WebSearchResult>();
        }

        public WebFileInfo GetFileInfo(string path)
        {
            return new WebFileInfo(PathUtil.StripFileProtocolPrefix(path));
        }

        public Stream GetFile(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public WebDictionary<string> GetExternalMediaInfo(WebMediaType type, string id)
        {
            string path = GetFileBasic(id).Path.First();
            return new WebDictionary<string>()
            {
                { "Type", File.Exists(path) ? "file" : "folder" },
                { "Path", path },
                { "Extensions", ".*" }
            };
        }

        private WebFileBasic ConvertFileInfoToFileBasic(FileInfo file)
        {
            return new WebFileBasic()
            {
                Title = file.Name,
                Path = new List<string>() { file.FullName },
                DateAdded = file.CreationTime,
                Id = EncodeTo64(file.FullName),
                LastAccessTime = file.LastAccessTime,
                LastModifiedTime = file.LastWriteTime,
                Size = file.Length
            };
        }

        private WebFolderBasic ConvertDirectoryInfoToFolderBasic(DirectoryInfo dir)
        {
            return new WebFolderBasic()
            {
                Title = dir.Name,
                Path = new List<string>() { dir.FullName },
                DateAdded = dir.CreationTime,
                Id = EncodeTo64(dir.FullName),
                LastAccessTime = dir.LastAccessTime,
                LastModifiedTime = dir.LastWriteTime
            };
        }

        private string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        private string DecodeFrom64(string encodedData)
        {
            try
            {
                byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
                return Encoding.UTF8.GetString(encodedDataAsBytes);
            }
            catch (FormatException)
            {
                return String.Empty;
            }
        }
    }
}
