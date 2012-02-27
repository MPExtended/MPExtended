#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service
{
    public static class WCFUtil
    {
        private static bool IsRestEnabled
        {
            get
            {
                return OperationContext.Current.IncomingMessageVersion == MessageVersion.None;
            }
        }

        public static string GetCurrentRoot()
        {
            // first try the HTTP host header
            try
            {
                string val = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Host];
                if (val != null)
                {
                    return "http://" + val + "/MPExtended/";
                }
            }
            catch (InvalidOperationException)
            {
                // probably a net.pipe binding, ignore that
            }

            // then try current IP address
            int port = Configuration.Services.Port;
            if (NetworkInformation.GetIPAddresses().Any())
            {
                return String.Format("http://{0}:{1}/MPExtended/", NetworkInformation.GetIPAddresses().First(), port);
            }

            // last resort: localhost
            return String.Format("http://localhost:{0}/MPExtended/", port);
        }

        public static string GetCurrentHostname()
        {
            Uri root = new Uri(GetCurrentRoot());
            return root.Host;
        }

        public static string GetClientIPAddress()
        {
            MessageProperties properties = OperationContext.Current.IncomingMessageProperties;
            if (properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                return endpoint.Address;
            }
            else
            {
                return properties.Via.Host;
            }
        }

        public static void SetResponseCode(HttpStatusCode code)
        {
            if(IsRestEnabled)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = code;
            }
        }

        public static void SetContentLength(long length)
        {
            if(IsRestEnabled)
            {
                // This doesn't work yet (#96)
                //AddHeader(HttpResponseHeader.ContentLength, length.ToString());
                WebOperationContext.Current.OutgoingResponse.ContentLength = length;
            }
        }


        public static void SetContentType(string type)
        {
            if(IsRestEnabled)
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = type;
            }
        }

        public static void AddHeader(string header, string value)
        {
            if (IsRestEnabled)
            {
                WebOperationContext.Current.OutgoingResponse.Headers.Add(header, value);
            }
        }

        public static void AddHeader(HttpResponseHeader header, string value)
        {
            if (IsRestEnabled)
            {
                WebOperationContext.Current.OutgoingResponse.Headers.Add(header, value);
            }
        }
    }
}
