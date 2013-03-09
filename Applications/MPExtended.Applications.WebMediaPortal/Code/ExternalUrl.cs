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