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
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace MPExtended.Libraries.ServiceLib
{
    public class DBLocations
    {
        public string Music { get; set; }
        public string Pictures { get; set; }
        public string TvSeries { get; set; }
        public string MovingPictures { get; set; }
        public string Shares { get; set; }
        public string Videos { get; set; }
    }

    public class Configuration
    {
        private static string cachedUsername;
        private static string cachedPassword;
        private static String CachedMPLocation;
        private static DBLocations cachedDatabaseLocations;

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
            if (cachedDatabaseLocations == null)
            {
                var dbs = XElement.Load(GetPath("MediaAccess.xml"))
                    .Element("mpdatabases").Elements("database").Select(
                        el => new KeyValuePair<string, IEnumerable<string>>(
                            (string)el.Attribute("name"), 
                            el.Elements("path").Select(x => x.Value).Where(x => File.Exists(x) && new FileInfo(x).Length > 0)
                        )
                    );

                cachedDatabaseLocations = new DBLocations()
                {
                    Music = dbs.Where(x => x.Key == "music").First().Value.FirstOrDefault(),
                    Pictures = dbs.Where(x => x.Key == "pictures").First().Value.FirstOrDefault(),
                    TvSeries = dbs.Where(x => x.Key == "tvseries").First().Value.FirstOrDefault(),
                    MovingPictures = dbs.Where(x => x.Key == "movingpictures").First().Value.FirstOrDefault(),
                    Shares = dbs.Where(x => x.Key == "shares").First().Value.FirstOrDefault(),
                    Videos = dbs.Where(x => x.Key == "videos").First().Value.FirstOrDefault(),
                };
            }

            return cachedDatabaseLocations;
        }

        public static void ChangeDbLocation(string db, string newPath)
        {
            XDocument doc = XDocument.Load(GetPath("MediaAccess.xml"));
            doc.Element("mpdatabases").Elements("database").Where(x => x.Name == db).Remove();

            XElement newnode = new XElement("database", new XElement("path", newPath));
            newnode.SetAttributeValue("name", db);
            doc.Element("mpdatabases").Add(newnode);

            doc.Save(GetPath("MediaAccess.xml"));
        }

        public static string GetPath(string filename)
        {
            string basedir = "";
#if DEBUG
            basedir = AppDomain.CurrentDomain.BaseDirectory;
#else
            basedir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended");
#endif
            return Path.Combine(basedir, filename);
        }


        public static void GetCredentials(out string username, out string password, bool overwriteCached)
        {
            if (overwriteCached || cachedUsername == null || cachedPassword == null)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(GetPath("Services.xml"));
                XmlNode userNode = doc.SelectSingleNode("/serviceconfig/config/username");
                XmlNode passNode = doc.SelectSingleNode("/serviceconfig/config/password");
                cachedUsername = userNode.InnerText;
                cachedPassword = passNode.InnerText;
            }
            username = cachedUsername;
            password = cachedPassword;
        }

        public static bool SetCredentials(string username, string password)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(GetPath("Services.xml"));
                XmlNode userNode = doc.SelectSingleNode("/serviceconfig/config/username");
                userNode.InnerText = username;

                XmlNode passNode = doc.SelectSingleNode("/serviceconfig/config/password");
                passNode.InnerText = password;

                doc.Save(GetPath("Services.xml"));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to set login", ex);
                return false;
            }
        }
    }
}
