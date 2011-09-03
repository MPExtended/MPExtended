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
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;

namespace MPExtended.Libraries.MASPlugin
{
    public class Configuration
    {
        private static Dictionary<string, Dictionary<string, string>> cachedConfig = new Dictionary<string, Dictionary<string, string>>();

        public static Dictionary<string, string> GetPluginConfiguration()
        {
            string name = Assembly.GetCallingAssembly().GetName().Name;
            if (!cachedConfig.ContainsKey(name))
            {
                var config = XElement.Load(ServiceLib.Configuration.GetPath("MediaAccess.xml"))
                    .Element("pluginConfiguration")
                    .Elements("plugin")
                    .Where(p => p.Attribute("name").Value == name)
                    .First()
                    .Descendants()
                    .Select(n => new KeyValuePair<string, string>(n.Name.LocalName, (string)n.Value))
                    .ToDictionary(x => x.Key, x => x.Value);
                cachedConfig[name] = config;
            }

            return cachedConfig[name];
        }

        // everything below this is obsolete
        private static Dictionary<string, string> cachedConfigLocations = new Dictionary<string, string>();
        private static Dictionary<string, string> cachedDataLocations = new Dictionary<string, string>();
        private static Dictionary<string, string> cachedDatabaseLocations = new Dictionary<string, string>();

        public static String GetConfigPath(string type)
        {
            if (!cachedConfigLocations.ContainsKey(type))
            {
                var paths = XElement.Load(ServiceLib.Configuration.GetPath("MediaAccess.xml"))
                    .Element("config")
                    .Elements(type);

                if (paths.Count() == 0)
                    return null;
                cachedConfigLocations[type] = paths.First().Value;
            }

            return cachedConfigLocations[type];
        }

        public static string GetDataLocation(string data)
        {
            if (!cachedDataLocations.ContainsKey(data))
            {
                var paths = XElement.Load(ServiceLib.Configuration.GetPath("MediaAccess.xml"))
                    .Element("datalocations")
                    .Elements(data);

                if (paths.Count() == 0)
                    return null;
                cachedDataLocations[data] = paths.First().Value;
            }

            return cachedDataLocations[data];
        }


        public static string GetDatabaseLocation(string db)
        {
            if (!cachedDatabaseLocations.ContainsKey(db))
            {
                var dbs = XElement.Load(ServiceLib.Configuration.GetPath("MediaAccess.xml"))
                    .Element("databases")
                    .Elements("database")
                    .Where(x => x.Attribute("name").Value == db)
                    .Elements("path")
                    .Select(p => p.Value)
                    .Where(p => File.Exists(p) && new FileInfo(p).Length > 0);

                if (dbs.Count() == 0)
                    return null;
                cachedDatabaseLocations[db] = dbs.First();
            }
            
            return cachedDatabaseLocations[db];
        }

        public static void ChangeDbLocation(string db, string newPath)
        {
            string path = ServiceLib.Configuration.GetPath("MediaAccess.xml");

            XDocument doc = XDocument.Load(path);
            doc.Element("databases").Elements("database").Where(x => x.Name == db).Remove();

            XElement newnode = new XElement("database", new XElement("path", newPath));
            newnode.SetAttributeValue("name", db);
            doc.Element("mpdatabases").Add(newnode);

            doc.Save(path);
        }
    }
}
