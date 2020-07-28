#region Copyright (C) 2011-2013 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;

namespace MPExtended.PlugIns.MAS.MPShares
{
    public abstract class ShareLibrary : IFileSystemLibrary
    {
        private IPluginData data;
        private Dictionary<string, string> configuration;
        private string[] sections;
        private List<Share> shares = null;

        protected string[] Extensions { get; set; }

        public bool Supported { get; set; }

        public ShareLibrary(IPluginData data, string[] sections)
        {
            this.data = data;
            this.configuration = data.GetConfiguration("MP Shares");
            this.sections = sections;

            Extensions = new string[] { ".jpg", ".png", ".bmp" };

            Supported = Mediaportal.HasValidConfigFile();
            if (Supported)
            {
                ReadConfiguration();
                ConfigurationChangeListener.ConfigurationChanged += ReadConfiguration;
                ConfigurationChangeListener.Enable();
            }
        }

        private void ReadConfiguration()
        {
            var localsharelist = new List<Share>();
            foreach (string section in sections)
            {
                IEnumerable<KeyValuePair<string, string>> list = Mediaportal.ReadSectionFromConfigFile(section);
                if (!list.Any())
                {
                    Log.Warn("MPShares: Failed to read section {0} from MediaPortal configuration file, aborting configuration reading", section);
                    return;
                }

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
                        Pincode = list.Where(x => x.Key == "pincode" + i).Select(x => x.Value).First(),
                        Extensions = extensions.ToList(),
                    });
                }
            }
            
            // make shares unique
            shares = localsharelist.GroupBy(x => x.Path, (path, gshares) => new Share()
            {
                Name = gshares.First().Name,
                Path = path,
                Pincode = gshares.First().Pincode,
                Extensions = gshares.SelectMany(x => x.Extensions).ToList()
            }).ToList();
            int shareNr = 0;
            foreach (Share share in shares)
            {
                share.Id = "s" + (shareNr++);
            }
        }

        public ShareLibrary(IPluginData data, string section)
            : this(data, new string[] { section })
        {
        }

        public IEnumerable<WebDriveBasic> GetDriveListing()
        {
            return shares.Select(x => ConvertShareToDriveBasic(x));
        }

        public IEnumerable<WebFolderBasic> GetFoldersListing(string id)
        {
            string path = GetPath(id);
            if (!String.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                return new DirectoryInfo(path).GetDirectories().Select(dir => ConvertDirectoryInfoToFolderBasic(dir));
            }

            return new List<WebFolderBasic>();
        }

        public IEnumerable<WebFileBasic> GetFilesListing(string id)
        {
            string path = GetPath(id);
            if (!String.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                Share share = GetShare(id);
                return new DirectoryInfo(path)
                    .GetFiles()
                    .Where(file => share.Extensions.Any(x => x.Equals(file.Extension, StringComparison.CurrentCultureIgnoreCase)))
                    .Select(file => ConvertFileInfoToFileBasic(file, share));
            }

            return new List<WebFileBasic>();
        }

        public WebDriveBasic GetDriveBasic(string id)
        {
            string path = GetPath(id);
            return ConvertShareToDriveBasic(shares.First(x => x.Path == path));
        }

        public WebFolderBasic GetFolderBasic(string id)
        {
            string path = GetPath(id);
            if (Directory.Exists(path))
            {
                return ConvertDirectoryInfoToFolderBasic(new DirectoryInfo(path));
            }
            return null;
        }

        public WebFileBasic GetFileBasic(string id)
        {
            string path = GetPath(id);
            if (!File.Exists(path))
                return null;
            return ConvertFileInfoToFileBasic(new FileInfo(PathUtil.StripFileProtocolPrefix(path)));
        }

        public WebFileInfo GetFileInfo(string path)
        {
            return new WebFileInfo(PathUtil.StripFileProtocolPrefix(path));
        }

        public Stream GetFile(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public IEnumerable<WebSearchResult> Search(string text)
        {
            return new List<WebSearchResult>();
        }

        public WebDictionary<string> GetExternalMediaInfo(WebMediaType type, string id)
        {
            string path = GetPath(id);
            return new WebDictionary<string>()
            {
                { "Type", File.Exists(path) ? "file" : "folder" },
                { "Path", path },
                { "Extensions", String.Join("|", GetShare(id).Extensions) }
            };
        }

        private WebDriveBasic ConvertShareToDriveBasic(Share share)
        {
            return new WebDriveBasic()
            {
                Title = share.Name,
                Path = new List<string>() { share.Path },
                Categories = GetHistory(share.Path),
                Id = share.Id,
                Pincode = share.Pincode,
                LastModifiedTime = DateTime.Now,
                LastAccessTime = DateTime.Now,
                Artwork = GetArtworkForDrive(share.Path)
            };
        }

        private WebFolderBasic ConvertDirectoryInfoToFolderBasic(DirectoryInfo dir, Share share = null)
        {
            return new WebFolderBasic()
            {
                Title = dir.Name,
                Path = new List<string>() { dir.FullName },
                Categories = GetHistory(dir.FullName),
                DateAdded = dir.CreationTime,
                Id = PathToIdentifier(dir, share),
                LastAccessTime = dir.LastAccessTime,
                LastModifiedTime = dir.LastWriteTime,
                Artwork = GetArtworkForFolder(dir.FullName)
            };
        }

        private WebFileBasic ConvertFileInfoToFileBasic(FileInfo file, Share share = null)
        {
            return new WebFileBasic()
            {
                Title = file.Name,
                Path = new List<string>() { file.FullName },
                Categories = GetHistory(file.FullName),
                DateAdded = file.CreationTime,
                Id = PathToIdentifier(file, share),
                LastAccessTime = file.LastAccessTime,
                LastModifiedTime = file.LastWriteTime,
                Size = file.Length,
                Artwork = GetArtworkForFile(file.FullName)
            };
        }

        private string GetPath(string id)
        {
            if (id.StartsWith("s"))
            {
                return GetShare(id).Path;
            }
            else if (id.StartsWith("d") || id.StartsWith("f"))
            {
                Share share = GetShare(id);
                string reldir = DecodeFrom64(id.Substring(id.IndexOf("_") + 1));
                if (!String.IsNullOrEmpty(reldir) && share != null)
                {
                    string path = Path.GetFullPath(Path.Combine(share.Path, reldir));

                    // it is possible that someone tricks us into looking outside of the shareroot by a /../ path
                    if (Security.IsInShare(path, share))
                    {
                        return path;
                    }
                }

                return null;
            }
            else
            {
                // malformed identifier, do not decode it as a path here because that would give access to arbitrary files
                return null;
            }
        }

        private Share GetShare(string id)
        {
            if (id.StartsWith("s"))
            {
                return shares.Where(x => x.Id == id).First();
            }
            else if (id.StartsWith("d") || id.StartsWith("f"))
            {
                string sid = id.Substring(1, id.IndexOf("_") - 1);
                if (!String.IsNullOrEmpty(sid))
                {
                    return shares.FirstOrDefault(x => x.Id == sid);
                }

                return null;
            }
            else
            {
                return null;
            }
        }

        private string PathToIdentifier(FileInfo info, Share useShare = null)
        {
            if(useShare == null)
            {
                return PathToIdentifier(info.FullName, useShare);
            }
            
            return "f" + useShare.Id + "_" + EncodeTo64(info.FullName.Substring(useShare.Path.Length + 1));
        }

        private string PathToIdentifier(DirectoryInfo info, Share useShare = null)
        {
            if (useShare == null)
            {
                return PathToIdentifier(info.FullName, useShare);
            }

            return "d" + useShare.Id + "_" + EncodeTo64(info.FullName.Substring(useShare.Path.Length + 1));
        }

        private string PathToIdentifier(string path, Share useShare = null)
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
                Log.Warn("MPShares: Invalid base64 input {0}", encodedData);
                return String.Empty;
            }
        }

        #region Artworks

        private List<WebArtwork> GetArtworkForDrive(string drive)
        {
            var artwork = new List<WebArtwork>();
            if (string.IsNullOrEmpty(drive) || !Directory.Exists(drive))
                return artwork;

            // Poster
            int i = 0;
            string folder = Path.Combine(drive, "folder{0}");
            var files = Extensions.Select(x => string.Format(folder, x))
                                  .Where(x => File.Exists(x))
                                  .Distinct();
            foreach (string file in files)
            {
                artwork.Add(new WebArtworkDetailed()
                {
                    Type = WebFileType.Cover,
                    Offset = i++,
                    Path = file,
                    Rating = 1 + i,
                    Id = file.GetHashCode().ToString(),
                    Filetype = Path.GetExtension(file).Substring(1)
                });
            }

            return artwork;
        }
        
        private List<WebArtwork> GetArtworkForFolder(string path)
        {
            var artwork = new List<WebArtwork>();
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return artwork;

            // Poster
            int i = 0;
            string folder = Path.Combine(path, "folder{0}");
            var files = Extensions.Select(x => string.Format(folder, x))
                                  .Where(x => File.Exists(x))
                                  .Distinct();
            foreach (string file in files)
            {
                artwork.Add(new WebArtworkDetailed()
                {
                    Type = WebFileType.Cover,
                    Offset = i++,
                    Path = file,
                    Rating = 1 + i,
                    Id = file.GetHashCode().ToString(),
                    Filetype = Path.GetExtension(file).Substring(1)
                });
            }

            return artwork;
        }

        private List<WebArtwork> GetArtworkForFile(string filename)
        {
            var artwork = new List<WebArtwork>();
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                return artwork;
                
            string name = Path.GetFileNameWithoutExtension(filename);
            string path = Path.GetDirectoryName(filename);

            // Poster
            int i = 0;
            string folder = Path.Combine(path, name + "{0}");
            var files = Extensions.Select(x => string.Format(folder, x))
                                  .Where(x => File.Exists(x))
                                  .Distinct();
            foreach (string file in files)
            {
                artwork.Add(new WebArtworkDetailed()
                {
                    Type = WebFileType.Cover,
                    Offset = i++,
                    Path = file,
                    Rating = 1 + i,
                    Id = file.GetHashCode().ToString(),
                    Filetype = Path.GetExtension(file).Substring(1)
                });
            }

            return artwork;
        }

        #endregion

        protected List<WebCategory> GetHistory(string fullpath)
        {
            List<WebCategory> history = new List<WebCategory>();
            if (string.IsNullOrEmpty(fullpath))
            {
                return history;
            }

            fullpath = Path.GetFullPath(fullpath);
            DirectoryInfo dir;
            if (File.Exists(fullpath))
            {
              dir = new DirectoryInfo(Path.GetDirectoryName(fullpath));
            }
            else if (Directory.Exists(fullpath))
            {
              dir = new DirectoryInfo(fullpath);
            }
            else
            {
              return history;
            }

            while (dir != null)
            {
                if (shares.Any(x => dir.FullName == x.Path))
                {
                   history.Add(new WebCategory() { Title = shares.Where(x => x.Path == dir.FullName).First().Name, Id = PathToIdentifier(dir.FullName) });
                }
                else
                { 
                    history.Add(new WebCategory() { Title = dir.Name, Id = PathToIdentifier(dir.FullName) });
                }
                if (shares.Any(x => dir.FullName == x.Path))
                {
                    break;
                }
                dir = dir.Parent;
            }
            history.Reverse();
            return history;
        }

    }
}
