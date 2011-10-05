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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.PlugIns.MAS.MPShares
{
    [Export(typeof(IFileSystemLibrary))]
    [ExportMetadata("Database", "MP Shares")]
    [ExportMetadata("Version", "1.0.0.0")]
    public class MPShares : IFileSystemLibrary
    {
        private IPluginData data;
        private List<Share> shares = null;

        [ImportingConstructor]
        public MPShares(IPluginData data)
        {
            this.data = data;
        }

        public IEnumerable<WebDriveBasic> GetLocalDrives()
        {
            var localsharelist = new List<Share>();
            string[] sections = { "pictures", "movies", "music" };
            XElement root = XElement.Load(this.data.Configuration["config"].ConfigValue);

            foreach (string section in sections)
            {
                IEnumerable<KeyValuePair<string, string>> list = root
                    .Elements("section")
                    .Where(x => (string)x.Attribute("name") == section)
                    .Elements("entry")
                    .Select(x => new KeyValuePair<string, string>((string)x.Attribute("name"), x.Value));

                string[] extensions = list.Where(x => x.Key == "extensions").Select(x => x.Value).First().Split(',');
                int count = list.Where(x => x.Key.StartsWith("sharename")).Count();

                for (int i = 0; i < count; i++)
                {
                    if (list.Where(x => x.Key == "sharetype" + i).Select(x => x.Value).First() == "yes")
                    {
                        continue;
                    }

                    string path = list.Where(x => x.Key == "sharepath" + i).Select(x => x.Value).First();
                    localsharelist.Add(new Share()
                    {
                        Name = list.Where(x => x.Key == "sharename" + i).Select(x => x.Value).First(),
                        Path = path,
                        Extensions = extensions.ToList(),
                    });
                }
            }

            // make shares unique
            shares = localsharelist.GroupBy(x => x.Path, (path, gshares) => new Share()
            {
                Name = gshares.First().Name,
                Path = path,
                Extensions = gshares.SelectMany(x => x.Extensions).ToList()
            }).ToList();
            int shareNr = 0;
            foreach (Share share in shares)
            {
                share.Id = "s" + (shareNr++);
            }
            return shares.Select(x => x.ToWebDriveBasic());
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
                    Id = PathToIdentifier(file.FullName)
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
                    Id = PathToIdentifier(dir.FullName),
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
                Id = PathToIdentifier(file.FullName)
            };
        }

        public WebFileInfo GetFileInfo(string path)
        {
            return new WebFileInfo(new FileInfo(path));
        }

        public Stream GetFile(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        private string GetPath(string id)
        {
            if (id.StartsWith("s"))
            {
                return GetLocalDrives().Where(x => x.Id == id).First().Path;
            }
            else if (id.StartsWith("d") || id.StartsWith("f"))
            {
                string sid = id.Substring(1, id.IndexOf("_") - 1);
                string reldir = DecodeFrom64(id.Substring(id.IndexOf("_") + 1));
                if (!String.IsNullOrEmpty(reldir) && !String.IsNullOrEmpty(sid))
                {
                    string path = Path.GetFullPath(Path.Combine(shares.Where(x => x.Id == sid).First().Path, reldir));

                    // it is possible that someone tricks us into looking out of the shareroot by a /../ path
                    if (Security.IsAllowedPath(data.Log, path, shares))
                    {
                        return path;
                    }
                }

                return null;
            }
            else
            {
                return null;
            }
        }

        private string PathToIdentifier(string path)
        {
            foreach (Share share in shares)
            {
                if (path.StartsWith(share.Path + @"\"))
                {
                    string type = File.Exists(path) ? "f" : "d";
                    return type + share.Id + "_" + EncodeTo64(path.Substring(share.Path.Length + 1));
                }
                else if (path == share.Path)
                {
                    return "s" + share.Id;
                }
            }

            return String.Empty;
        }

        private string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        private string DecodeFrom64(string encodedData)
        {
            try
            {
                byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
                string returnValue = System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
                return returnValue;
            }
            catch (FormatException)
            {
                data.Log.Warn("MPShares: Invalid base64 input {0}", encodedData);
                return String.Empty;
            }
        }
    }
}
