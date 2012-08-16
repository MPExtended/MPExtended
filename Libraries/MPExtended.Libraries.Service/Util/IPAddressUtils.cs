using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace MPExtended.Libraries.Service.Util
{
    public class IPAddressUtils
    {
        private const String WHATS_MY_IP_URL = "http://automation.whatismyip.com/n09230945.asp";
        private const String APP_ENGINE_URL = "http://agentgatech.appspot.com/";

        private static String CachedExternalIp;
        private static DateTime CacheLastUpdated;

        /// <summary>
        /// Retrieve the external ip of this pc from one of n external websites
        /// 
        /// Currently available and processed int this order:
        /// 1) whatismyip.com (url for automated access)
        /// 2) agentgatech.appspot.com (google app engine)
        /// </summary>
        /// <returns>External ip of pc</returns>
        public static string GetExternalIpAddress()
        {
            return GetExternalIpAddress(false);
        }

        /// <summary>
        /// Retrieve the external ip of this pc from one of n external websites
        /// 
        /// Currently available and processed int this order:
        /// 1) whatismyip.com (url for automated access)
        /// 2) agentgatech.appspot.com (google app engine)
        /// </summary>
        /// <param name="forceUpdate">If true, cache is ignored</param>
        /// <returns>External ip of pc</returns>
        public static String GetExternalIpAddress(bool forceUpdate)
        {
            if (!forceUpdate && CachedExternalIp != null
                && DateTime.Now.Subtract(CacheLastUpdated).TotalMinutes < 5)
            {
                return CachedExternalIp;
            }

            WebClient client = new WebClient();
            client.Headers.Add("user-agent",
                   "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            String ip = GetExternalIpAddressWhatsMyIp(client);
            if (ip == null)
            {
                ip = GetExternalIpAddressAppEngine(client);
            }

            if (ip != null)
            {
                CachedExternalIp = ip;
                CacheLastUpdated = DateTime.Now;
            }
            else
            {
                Log.Warn("Couldn't retrieve external ip from any of the external websites");
            }
            return ip;
        }

        private static string GetExternalIpAddressAppEngine(WebClient client)
        {
            try
            {
                Log.Info("Getting external ip from http://agentgatech.appspot.com/");
                return client.DownloadString(APP_ENGINE_URL);
            }
            catch (Exception ex)
            {
                Log.Warn("Error retrieving external ip from http://agentgatech.appspot.com/", ex);
            }
            return null;
        }

        private static string GetExternalIpAddressWhatsMyIp(WebClient client)
        {
            try
            {
                Log.Info("Getting external ip from whatismyip.com");
                return client.DownloadString(WHATS_MY_IP_URL);
            }
            catch (Exception ex)
            {
                Log.Warn("Error retrieving external ip from whatismyip.com", ex);
            }
            return null;
        }
    }
}
