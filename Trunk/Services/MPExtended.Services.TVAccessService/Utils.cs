using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace MPExtended.Services.TVAccessService
{
    public class Utils
    {
        private static String CachedUser;
        private static String CachedPassword;

        public static void GetLogin(out string uid, out string pwd, bool overwriteCached)
        {
            if (overwriteCached || CachedUser == null || CachedPassword == null)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + "config.xml");
                XmlNode userNode = doc.SelectSingleNode("/appconfig/config/username");
                XmlNode passNode = doc.SelectSingleNode("/appconfig/config/password");
                CachedUser = userNode.InnerText;
                CachedPassword = passNode.InnerText;
            }
            uid = CachedUser;
            pwd = CachedPassword;
        }

        public static bool SetLogin(string uid, string pwd)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + "config.xml");
                XmlNode userNode = doc.SelectSingleNode("/appconfig/config/username");
                userNode.InnerText = uid;

                XmlNode passNode = doc.SelectSingleNode("/appconfig/config/password");
                passNode.InnerText = pwd;

                doc.Save(AppDomain.CurrentDomain.BaseDirectory + "config.xml");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
