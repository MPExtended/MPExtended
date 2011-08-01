using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace MPExtended.Libraries.ServiceLib
{
    public class Configuration
    {
        private static string cachedUsername;
        private static string cachedPassword;

        public static string GetPath(string filename) 
        {
            string basedir = "";
#if DEBUG
            basedir = AppDomain.CurrentDomain.BaseDirectory;
#else
            basedir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
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
