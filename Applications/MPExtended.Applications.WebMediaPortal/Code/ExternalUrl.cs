#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MPExtended.Libraries.Service.Config;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public class ExternalUrl
    {
        public static string GetScheme(HttpRequestBase request)
        {
            switch (Configuration.WebMediaPortal.ExternalUrlScheme)
            {
                case UrlScheme.Http:
                    return "http";
                case UrlScheme.Https:
                    return "https";
                default:
                    return request.Url.Scheme;
            }
        }

        public static string GetHost(HttpRequestBase request)
        {
            if (!String.IsNullOrWhiteSpace(Configuration.WebMediaPortal.ExternalUrlHost))
                return Configuration.WebMediaPortal.ExternalUrlHost;

            // Now, this is tricky. When port forwards (or another complex setup) is in use, we want to return the host
            // that the user entered in the address bar (because we can be pretty sure that one is working from the
            // location where the user currently is), which is send in the HTTP/1.1 Host: header. However, .NET rewrites
            // the host in Request.Url for unknown reasons, so we need to access the header directly.
            if (request.Headers["Host"] != null)
                return request.Headers["Host"];

            // Request.Url.Host only contains the hostname, not the port, which we want to include if the default port
            // isn't being used. Request.Url.Authority also does some DNS-lookups according to a StackOverflow answer,
            // so we combine host and port ourselves.
            return request.Url.IsDefaultPort 
                ? request.Url.Host
                : String.Format("{0}:{1}", request.Url.Host, request.Url.Port);
        }
    }
}