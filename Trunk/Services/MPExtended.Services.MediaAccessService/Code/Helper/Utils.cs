using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService.Code.Helper
{
    public class DBLocations
    {
        public string Music;
        public string Pictures;
        public string TvSeries;
        public string MovingPictures;
        public string Shares;
        public string Videos;
    }

    public class Utils
    {
        private static string logDir = AppDomain.CurrentDomain.BaseDirectory + "\\logs";
        private static Dictionary<String, WebBannerPath> CachedWebBannerPaths = null;
        private static String CachedMPLocation;
        private static DBLocations CachedDbLocation;

        public static String GetMpConfigPath()
        {
            if (CachedMPLocation == null)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Configuration.GetPath("MediaAccess.xml"));
                XmlNode gNode = doc.SelectSingleNode("/appconfig/config/mpconfig");
                CachedMPLocation = gNode.InnerText;
            }

            return CachedMPLocation;
        }

        public static DBLocations GetMPDbLocations()
        {
            if (Utils.CachedDbLocation == null)
            {
                DBLocations dbLocations = new DBLocations();
                XmlDocument doc = new XmlDocument();
                doc.Load(Configuration.GetPath("MediaAccess.xml"));
                XmlNodeList dbNodes = doc.SelectNodes("/appconfig/mpdatabases/database");

                Log.Debug("Reading database paths");

                foreach (XmlNode node in dbNodes)
                {
                    String name = node.Attributes["name"].Value;
                    String value = node.Attributes["filename"].Value;
                    Log.Debug(name + ": " + value);
                    switch (name)
                    {
                        case "music":
                            dbLocations.Music = value;
                            break;
                        case "pictures":
                            dbLocations.Pictures = value;
                            break;
                        case "tvseries":
                            dbLocations.TvSeries = value;
                            break;
                        case "movingpictures":
                            dbLocations.MovingPictures = value;
                            break;
                        case "shares":
                            dbLocations.Shares = value;
                            break;
                        case "videos":
                            dbLocations.Videos = value;
                            break;
                    }
                }
                CachedDbLocation = dbLocations;
            }
            return CachedDbLocation;
        }

        public static void ChangeDbLocation(String _db, String _newPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Configuration.GetPath("MediaAccess.xml"));
            XmlNodeList dbNodes = doc.SelectNodes("/appconfig/mpdatabases/database");

            Log.Debug("Reading database paths");

            foreach (XmlNode node in dbNodes)
            {
                String name = node.Attributes["name"].Value;

                if (name.Equals(_db))
                {
                    node.Attributes["filename"].Value = _newPath;
                }
            }

            doc.Save(AppDomain.CurrentDomain.BaseDirectory + "config.xml");
        }


        public static Dictionary<String, WebBannerPath> GetWebBannerPaths()
        {
            if (Utils.CachedWebBannerPaths == null)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Configuration.GetPath("MediaAccess.xml"));
                XmlNodeList dbNodes = doc.SelectNodes("/appconfig/thumbpaths/thumb");
                Dictionary<String, WebBannerPath> retList = new Dictionary<String, WebBannerPath>();
                foreach (XmlNode node in dbNodes)
                {
                    retList.Add(node.Attributes["name"].Value, new WebBannerPath(node.Attributes["name"].Value,
                                                                              node.Attributes["path"].Value,
                                                                              node.Attributes["virtualpath"].Value));
                }
                CachedWebBannerPaths = retList;
                return retList;
            }
            else
            {
                return CachedWebBannerPaths;
            }
        }


        public static String[] SplitString(String _stringToSplit)
        {
            if (_stringToSplit != null)
            {
                _stringToSplit = _stringToSplit.Trim(new char[] { '|', ' ' });
                return _stringToSplit.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                return null;
            }
        }
        public static string ClearString(String _stringToClean)
        {
            return _stringToClean.Substring(2, (_stringToClean.Length - 5));
        }


        public static string GetCoverArtName(string strFolder, string strFileName)
        {
            if (string.IsNullOrEmpty(strFolder) || string.IsNullOrEmpty(strFileName))
                return string.Empty;

            return string.Format(@"{0}\{1}{2}", strFolder, Utils.MakeFileName(strFileName), ".jpg");
        }

        public static string GetLargeCoverArtName(string strFolder, string strFileName)
        {
            if (string.IsNullOrEmpty(strFolder) || string.IsNullOrEmpty(strFileName))
                return string.Empty;

            return Utils.GetCoverArtName(strFolder, strFileName + "L");
        }

        public static string GetLargeAlbumCover(string artistName, string albumName)
        {
            if (string.IsNullOrEmpty(artistName) || string.IsNullOrEmpty(albumName))
                return string.Empty;

            artistName = artistName.Trim(new char[] { '|', ' ' });
            albumName = albumName.Replace(":", "_");
            String musicThumbPath = Utils.GetWebBannerPaths()["music"].Path;

            return musicThumbPath + "\\Albums\\" + artistName + "-" + albumName + "L.jpg";
        }

        public static string MakeFileName(string strText)
        {
            if (strText == null) return string.Empty;
            if (strText.Length == 0) return string.Empty;

            string strFName = strText.Replace(':', '_');
            strFName = strFName.Replace('/', '_');
            strFName = strFName.Replace('\\', '_');
            strFName = strFName.Replace('*', '_');
            strFName = strFName.Replace('?', '_');
            strFName = strFName.Replace('\"', '_');
            strFName = strFName.Replace('<', '_');
            strFName = strFName.Replace('>', '_');
            strFName = strFName.Replace('|', '_');

            bool unclean = true;
            char[] invalids = Path.GetInvalidFileNameChars();
            while (unclean)
            {
                unclean = false;

                char[] filechars = strFName.ToCharArray();

                foreach (char c in filechars)
                {
                    if (!unclean)
                        foreach (char i in invalids)
                        {
                            if (c == i)
                            {
                                unclean = true;
                                //Log.Warn("Utils: *** File name {1} still contains invalid chars - {0}", Convert.ToString(c), strFName);
                                strFName = strFName.Replace(c, '_');
                                break;
                            }
                        }
                }
            }
            return strFName;
        }



    }
}