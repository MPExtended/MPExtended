using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Code.Helper;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService.Code
{
    public class Shares
    {
        public enum ShareType
        {
            Picture,
            Video,
            Music
        }

        private static Dictionary<ShareType, string> map = new Dictionary<ShareType, string>() {
            { ShareType.Picture, "pictures" },
            { ShareType.Video, "movies" },
            { ShareType.Music, "music" }
        };

        /// <summary>
        /// Gets a list of all shares for a given type from MediaPortal.xml, which requires an awful lot of logic.
        /// </summary>
        /// <returns>List of all shares</returns>
        public static List<WebShare> GetAllShares(ShareType shareType)
        {
            List<WebShare> shares = new List<WebShare>();
            XElement root = XElement.Load(Utils.GetMpConfigPath());
            IEnumerable<KeyValuePair<string, string>> list = 
                root.Elements("section")
                .Where(x => (string)x.Attribute("name") == map[shareType])
                .Elements("entry")
                .Select(x => new KeyValuePair<string,string>((string)x.Attribute("name"), x.Value));

            string[] extensions = list.Where(x => x.Key == "extensions").Select(x => x.Value).First().Split(',');
            int count = list.Where(x => x.Key.StartsWith("sharename")).Count();

            for (int i = 0; i < count; i++)
            {
                shares.Add(new WebShare()
                {
                    ShareId = i,
                    Name = list.SelectShareNode("sharename", i),
                    Path = list.SelectShareNode("sharepath", i),
                    PinCode = list.SelectShareNode("pincode", i),
                    IsFtp = list.SelectShareNode("sharetype", i) == "yes",
                    FtpServer = list.SelectShareNode("shareserver", i),
                    FtpLogin = list.SelectShareNode("sharelogin", i),
                    FtpPassword = list.SelectShareNode("sharepassword", i),
                    FtpPath = list.SelectShareNode("shareremotepath", i),
                    FtpPort = Int32.Parse(list.SelectShareNode("shareport", i)),
                    Extensions = extensions
                });
            }

            return shares;
        }

        /// <summary>
        /// Get a list of all directories in a path
        /// </summary>
        public static List<String> GetDirectoryListByPath(string path)
        {
            if (!IsAllowedPath(path))
            {
                Log.Warn("Tried to get directory list of non-allowed or non-existent directory {0}", path);
                return new List<String>();
            }

            return Directory.EnumerateDirectories(path).ToList();
        }

        public static List<WebFileInfo> GetFileListByPath(string path)
        {
            if (!IsAllowedPath(path))
            {
                Log.Warn("Tried to get file list of non-allowed or non-existent directory {0}", path);
                return new List<WebFileInfo>();
            }

            return Directory.EnumerateFiles(path).Select(x => new FileInfo(x).ToWebFileInfo()).ToList();
        }

        public static WebFileInfo GetFileInfo(string path)
        {
            if (!IsAllowedPath(path))
            {
                Log.Warn("Tried to get file info of non-allowed or non-existent file {0}", path);
                return null;
            }

            return Filesystem.GetFileInfo(path);
        }

        /// <summary>
        /// Check if a path is one of the shares. Needs some improvement to be really secure.
        /// </summary>
        public static bool IsAllowedPath(string path, ShareType type)
        {
            try
            {
                // non-existent files don't yield results anyway, we might need to have them existent for the checks here later
                if (!File.Exists(path) && !Directory.Exists(path))
                    return false;

                // do not allow non-rooted parts for security
                if (!Path.IsPathRooted(path))
                    return false;

                // only check on directory
                if(File.Exists(path))
                    path = Path.GetDirectoryName(path);

                foreach (WebShare share in GetAllShares(type))
                {
                    if (share.IsFtp || !Directory.Exists(share.Path))
                        continue;

                    if (Utils.IsSubdir(share.Path, path))
                        return true;
                }
            }
            catch(Exception e) 
            {
                Log.Error(String.Format("Exception during IsAllowedPath with path = {0} and type = {1}", path, type), e);
            }
            return false;
        }

        public static bool IsAllowedPath(string path)
        {
            foreach (ShareType type in map.Keys)
            {
                if (IsAllowedPath(path, type))
                    return true;
            }
            return false;
        }
    }
}
