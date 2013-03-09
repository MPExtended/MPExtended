using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MPExtended.Libraries.Service.Config;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public class ExternalUrl
    {
        public static string GetScheme(string requestScheme)
        {
            switch (Configuration.WebMediaPortal.ExternalUrlScheme)
            {
                case UrlScheme.Http:
                    return "http";
                case UrlScheme.Https:
                    return "https";
                default:
                    return requestScheme;
            }
        }

        public static string GetScheme(Uri requestUri)
        {
            return GetScheme(requestUri.Scheme);
        }

        public static string GetHost(string requestHost)
        {
            return String.IsNullOrWhiteSpace(Configuration.WebMediaPortal.ExternalUrlHost) ? requestHost : Configuration.WebMediaPortal.ExternalUrlHost;
        }

        public static string GetHost(Uri requestUri)
        {
            return GetHost(requestUri.Host);
        }
    }
}