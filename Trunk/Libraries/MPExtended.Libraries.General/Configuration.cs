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

namespace MPExtended.Libraries.General
{
    public class Configuration
    {
        private static string cachedUsername;
        private static string cachedPassword;

        public static string GetPath(string filename)
        {
            string basedir = "";
#if DEBUG
            basedir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Config", "Debug"));
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
                Log.Error("Couldn't update credentials", ex);
                return false;
            }
        }

        public static bool SetPort(int _port)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(GetPath("Services.xml"));
                XmlNode portNode = doc.SelectSingleNode("/serviceconfig/port");
                portNode.InnerText = _port.ToString();

                doc.Save(GetPath("Services.xml"));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Couldn't set port in " + GetPath("Services.xml"), ex);
                return false;
            }
        }

        public static int GetPort()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(GetPath("Services.xml"));
                XmlNode portNode = doc.SelectSingleNode("/serviceconfig/port");
                String portString = portNode.InnerText;
                return Int32.Parse(portString);
            }
            catch (Exception ex)
            {
                Log.Error("Couldn't load port from " + GetPath("Services.xml"), ex);
                return -99;
            }
        }
    }
}
