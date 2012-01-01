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
using MPExtended.Libraries.General;

namespace MPExtended.Libraries.ServiceLib
{
    public static class WCFUtil
    {
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

        public static void SetResponseCode(HttpStatusCode code)
        {
            try
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = code;
            }
            catch (InvalidOperationException)
            {
                // probably a net.pipe binding, just ignore it
            }
        }

        public static void AddHeader(string header, string value)
        {
            try
            {
                WebOperationContext.Current.OutgoingResponse.Headers.Add(header, value);
            }
            catch (InvalidOperationException)
            {
                // probably a net.pipe binding, just ignore it
            }
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
                return "";
            }
        }
    }
}
