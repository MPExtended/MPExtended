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
using System.IO;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;

namespace MPExtended.PlugIns.MAS.LocalFileSystem
{
    [Export(typeof(IFileSystemLibrary))]
    [ExportMetadata("Name", "LocalFileSystem")]
    [ExportMetadata("Type", typeof(LocalFileSystem))]
    [ExportMetadata("Id", 2)]
    public class LocalFileSystem : IFileSystemLibrary
    {
        public void Init() 
        {
        }

        public IEnumerable<WebDriveBasic> GetLocalDrives()
        {
            return DriveInfo.GetDrives().Select(x => new WebDriveBasic()
            {
                Id = EncodeTo64(x.RootDirectory.Name),
                Name = x.Name,
                Path = x.RootDirectory.FullName
            });
        }

        public IEnumerable<WebFileBasic> GetFilesListing(string id)
        {
            string path = DecodeFrom64(id);
            if (!String.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                return new DirectoryInfo(path).GetFiles().Select(file => new WebFileBasic()
                {
                    Name = file.Name,
                    Path = new List<string>() { file.FullName },
                    DateAdded = file.CreationTime,
                    Id = EncodeTo64(file.FullName)
                });
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
                    Name = dir.Name,
                    Path = dir.FullName,
                    DateAdded = dir.CreationTime,
                    Id = EncodeTo64(dir.FullName),
                });
            }

            return new List<WebFolderBasic>();
        }

        public WebFileBasic GetFileBasic(string id)
        {
            string path = DecodeFrom64(id);
            if (!File.Exists(path))
                return null;
            FileInfo file = new FileInfo(path);
            return new WebFileBasic()
                {
                    Name = file.Name,
                    Path = new List<string>() { file.FullName },
                    DateAdded = file.CreationTime,
                    Id = EncodeTo64(file.FullName)
                };
        }

        public IEnumerable<WebSearchResult> Search(string text)
        {
            return new List<WebSearchResult>();
        }

        public WebFileInfo GetFileInfo(string path)
        {
            return new WebFileInfo(new FileInfo(path));
        }

        public Stream GetFile(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        static private string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
        static private string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }
    }
}
