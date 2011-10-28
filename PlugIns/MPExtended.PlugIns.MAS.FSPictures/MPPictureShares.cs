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
using System.Xml.Linq;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.PlugIns.MAS.FSPictures
{
    [Export(typeof(IPictureLibrary))]
    [ExportMetadata("Name", "MP Picture Shares")]
    [ExportMetadata("Id", 8)]
    public class MPPictureShares : PictureLibraryBase
    {
        private struct Share
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public int Index { get; set; }
        }
        private List<Share> shares;
        private Dictionary<string, Share> shareCache;
        private string config;

        [ImportingConstructor]
        public MPPictureShares(IPluginData data) : base(data)
        {
            this.config = data.GetConfiguration("MP Picture Shares")["config"];

            XElement root = XElement.Load(this.config);
            IEnumerable<KeyValuePair<string, string>> list = root
                .Elements("section")
                .Where(x => (string)x.Attribute("name") == "pictures")
                .Elements("entry")
                .Select(x => new KeyValuePair<string, string>((string)x.Attribute("name"), x.Value));

            Extensions = list.Where(x => x.Key == "extensions").Select(x => x.Value).First().Split(',');

            shares = new List<Share>();
            int count = list.Where(x => x.Key.StartsWith("sharename")).Count();
            for (int i = 0; i < count; i++)
            {
                if (list.Where(x => x.Key == "sharetype" + i).Select(x => x.Value).First() == "yes")
                {
                    continue;
                }

                shares.Add(new Share() 
                {
                    Name = list.Where(x => x.Key == "sharename" + i).Select(x => x.Value).First(),
                    Path = Path.GetFullPath(list.Where(x => x.Key == "sharepath" + i).Select(x => x.Value).First()),
                    Index = i
                });
            }

            shareCache = new Dictionary<string, Share>();
        }

        public override IEnumerable<WebCategory> GetAllPictureCategories()
        {
            return shares.Select(x => new WebCategory() { Id = x.Index.ToString(), Title = x.Name });
        }

        protected override string PathToId(string fullpath)
        {
            string dir = Path.GetDirectoryName(Path.GetFullPath(fullpath));
            Share share;
            if (!shareCache.ContainsKey(dir))
            {
                share = shares.Where(x => dir.StartsWith(x.Path) && IsSubdir(x.Path, dir)).First();
                shareCache[dir] = share;
            }
            else
            {
                share = shareCache[dir];
            }

            string dircomponent = Path.GetFullPath(fullpath).Substring(share.Path.Length + 1);
            byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes(dircomponent);
            string base64dir = Convert.ToBase64String(toEncodeAsBytes);
            return String.Format("{0}|{1}", share.Index, base64dir);
        }

        protected override string IdToPath(string id)
        {
            if (!id.Contains("|"))
            {
                int i = Int32.Parse(id);
                return shares.Where(x => x.Index == i).First().Path;
            }

            int shareIndex = Int32.Parse(id.Substring(0, id.IndexOf("|")));
            string path64 = id.Substring(id.IndexOf("|") + 1);
            byte[] encodedDataAsBytes = Convert.FromBase64String(path64);
            string path = ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);

            return Path.GetFullPath(Path.Combine(shares.Where(x => x.Index == shareIndex).First().Path, path));
        }

        private static bool IsSubdir(string root, string testDir)
        {
            DirectoryInfo shareDir = new DirectoryInfo(root);
            DirectoryInfo currentDir = new DirectoryInfo(testDir);

            while (currentDir != null)
            {
                if (currentDir.FullName == shareDir.FullName)
                    return true;

                currentDir = currentDir.Parent;
            }

            return false;
        }
    }
}