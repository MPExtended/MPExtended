using System;
using System.Xml;

namespace MPExtended.Services.StreamingService.Util
{
    internal static class Config
    {
        private static string cachedUser;
        private static string cachedPassword;

        public static void GetLogin(out string user, out string pass, bool overwriteCached) 
        {
            if(overwriteCached || cachedUser == null || cachedPassword == null) {
                XmlDocument doc = new XmlDocument();
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + "config.xml");
                XmlNode userNode = doc.SelectSingleNode("/appconfig/config/username");
                XmlNode passNode = doc.SelectSingleNode("/appconfig/config/password");
                cachedUser = userNode.InnerText;
                cachedPassword = passNode.InnerText;
            }

            user = cachedUser;
            pass = cachedPassword;
        }
    }
}
