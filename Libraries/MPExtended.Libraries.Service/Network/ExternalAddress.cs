#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Net;
using System.Text;

namespace MPExtended.Libraries.Service.Network
{
    public class ExternalAddress
    {
        private const int CACHE_LIFETIME = 300; // in seconds
        private const string APP_ENGINE_URL = "http://agentgatech.appspot.com/";
        private const string DYNDNS_URL = "http://checkip.dyndns.com/";

        private static IPAddress CachedIP;
        private static DateTime CacheLastUpdated;

        /// <summary>
        /// Get the external address of this server, either the external ip that
        /// is detected with an external website or a custom address that the user
        /// provided (e.g. dyndns address)
        /// </summary>
        /// <returns>External address of this server</returns>
        public static string GetAddress()
        {
            return GetAddress(false);
        }

        /// <summary>
        /// Get the external address of this server, either the external ip that
        /// is detected with an external website or a custom address that the user
        /// provided (e.g. dyndns address)
        /// </summary>
        /// <param name="forceUpdate">If true, cache is ignored</param>
        /// <returns>External address of this server</returns>
        public static string GetAddress(bool forceUpdate)
        {
            if (!Configuration.Services.ExternalAddress.Autodetect)
            {
                return Configuration.Services.ExternalAddress.Custom;
            }
            else
            {
                var ip = GetIP(forceUpdate);
                return ip != null ? ip.ToString() : null;
            }
        }

        /// <summary>
        /// Retrieve the external ip of this pc from one of n external websites
        /// 
        /// Currently available and processed in this order:
        /// 1) whatismyip.com (url for automated access)
        /// 2) dyndns.com (checkip.dyndns.com)
        /// 3) agentgatech.appspot.com (google app engine)
        /// </summary>
        /// <returns>External ip of pc</returns>
        public static IPAddress GetIP()
        {
            return GetIP(false);
        }

        /// <summary>
        /// Retrieve the external ip of this pc from one of n external websites
        /// 
        /// Currently available and processed in this order:
        /// 1) whatismyip.com (url for automated access)
        /// 2) dyndns.com (checkip.dyndns.com)
        /// 3) agentgatech.appspot.com (google app engine)
        /// </summary>
        /// <param name="forceUpdate">If true, cache is ignored</param>
        /// <returns>External ip of pc</returns>
        public static IPAddress GetIP(bool forceUpdate)
        {
            if (!forceUpdate && CachedIP != null && (DateTime.Now - CacheLastUpdated).TotalSeconds < CACHE_LIFETIME)
                return CachedIP;

            WebClient client = new WebClient();
            client.Headers[HttpRequestHeader.UserAgent] = VersionUtil.GetUserAgent();

            var methods = new Func<WebClient, string>[] { RetrieveFromDynDNS, RetrieveFromAppEngine };
            foreach (var method in methods)
            {
                string result = method.Invoke(client);
                if (result == null)
                    continue;

                if (!IPAddress.TryParse(result, out CachedIP))
                {
                    Log.Warn("Failed to parse retrieved external address '{0}'", result);
                    continue;
                }

                CacheLastUpdated = DateTime.Now;
                return CachedIP;
            }

            Log.Warn("Couldn't retrieve external IP from any of the external websites. Is your network functioning and the firewall configured correctly?");
            return null;
        }

        /// <summary>
        /// Get external ip from google app engine
        /// </summary>
        /// <param name="client">WebClient</param>
        /// <returns>IP of server</returns>
        private static string RetrieveFromAppEngine(WebClient client)
        {
            try
            {
                Log.Info("Getting external ip from {0}", APP_ENGINE_URL);
                return client.DownloadString(APP_ENGINE_URL);
            }
            catch (Exception ex)
            {
                Log.Warn("Error retrieving external IP address", ex);
            }
            return null;
        }

        /// <summary>
        /// Get external ip from dyndns
        /// </summary>
        /// <param name="client">WebClient</param>
        /// <returns>IP of server</returns>
        private static string RetrieveFromDynDNS(WebClient client)
        {
            try
            {
                Log.Info("Getting external ip from {0}", DYNDNS_URL);
                String result = client.DownloadString(DYNDNS_URL);

                // Search for the ip in the html
                int first = result.IndexOf(":") + 1;
                int last = result.LastIndexOf("</body>");
                result = result.Substring(first, last - first);

                return result.Trim();
            }
            catch (Exception ex)
            {
                Log.Warn("Error retrieving external IP address", ex);
            }
            return null;
        }
    }
}
