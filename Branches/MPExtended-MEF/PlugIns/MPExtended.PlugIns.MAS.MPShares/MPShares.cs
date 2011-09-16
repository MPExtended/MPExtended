#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using System.Xml.Linq;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;

namespace MPExtended.PlugIns.MAS.MPShares
{
    [Export(typeof(IFileSystemLibrary))]
    [ExportMetadata("Database", "MP Shares")]
    public class MPShares : IFileSystemLibrary
    {
        private IPluginData data;
        private List<WebDriveBasic> shares = null;

        [ImportingConstructor]
        public MPShares(IPluginData data)
        {
            this.data = data;
        }

        public IEnumerable<WebDriveBasic> GetLocalDrives()
        {
            XElement root = XElement.Load(this.data.Configuration["configpath"]);
            IEnumerable<KeyValuePair<string, string>> list =
                root.Elements("section")
                .Where(x => (string)x.Attribute("name") == "pictures" || (string)x.Attribute("name") == "movies" || (string)x.Attribute("name") == "music")
                .Elements("entry")
                .Select(x => new KeyValuePair<string, string>((string)x.Attribute("name"), x.Value));

            string[] extensions = list.Where(x => x.Key == "extensions").Select(x => x.Value).First().Split(',');
            int count = list.Where(x => x.Key.StartsWith("sharename")).Count();

            var alreadyDonePath = new List<string>();
            shares = new List<WebDriveBasic>();
            int j = 0;
            for (int i = 0; i < count; i++)
            {
                string path = list.Where(x => x.Key == "sharepath" + i).Select(x => x.Value).First();
                if (alreadyDonePath.Contains(path))
                {
                    continue;
                }

                shares.Add(new WebDriveBasic()
                {
                    Name = list.Where(x => x.Key == "sharename" + i).Select(x => x.Value).First(),
                    Path = path,
                    Id = "s" + j++
                });
                alreadyDonePath.Add(path);
            }

            return shares;
        }

        public IEnumerable<WebFileBasic> GetFilesListing(string id)
        {
            string path = GetPath(id);
            if (!String.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                return new DirectoryInfo(path).GetFiles().Select(file => new WebFileBasic()
                {
                    Name = file.Name,
                    Path = new List<string>() { file.FullName },
                    DateAdded = file.CreationTime,
                    Id = "f" + EncodeTo64(file.FullName)
                });
            }

            return new List<WebFileBasic>();
        }

        public IEnumerable<WebFolderBasic> GetFoldersListing(string id)
        {
            string path = GetPath(id);
            if (!String.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                return new DirectoryInfo(path).GetDirectories().Select(dir => new WebFolderBasic()
                {
                    Name = dir.Name,
                    Path = dir.FullName,
                    DateAdded = dir.CreationTime,
                    Id = "d" + EncodeTo64(dir.FullName),
                });
            }

            return new List<WebFolderBasic>();
        }

        public WebFileBasic GetFileBasic(string id)
        {
            string path = GetPath(id);
            if (!File.Exists(path))
                return null;
            FileInfo file = new FileInfo(path);
            return new WebFileBasic()
            {
                Name = file.Name,
                Path = new List<string>() { file.FullName },
                DateAdded = file.CreationTime,
                Id = "f" + EncodeTo64(file.FullName)
            };
        }

        public bool IsLocalFile(string path)
        {
            return true;
        }

        public Stream GetFile(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        private string GetPath(string id)
        {
            if (id.StartsWith("s"))
            {
                int intId = Int32.Parse(id.Substring(1));
                return GetLocalDrives().ElementAt(intId).Path;
            }
            else if (id.StartsWith("d") || id.StartsWith("f"))
            {
                string path = DecodeFrom64(id.Substring(1));
                if (Security.IsAllowedPath(data.Log, path, GetLocalDrives()))
                {
                    return path;
                }

                return null;
            }
            else
            {
                return null;
            }
        }

        private string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        private string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }
    }
}
