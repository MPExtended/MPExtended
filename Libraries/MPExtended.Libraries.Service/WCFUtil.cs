﻿#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Libraries.Service.Network;
using MPExtended.Libraries.Service.Extensions;

namespace MPExtended.Libraries.Service
{
    public static class WCFUtil
    {
        internal const string HEADER_NAMESPACE = "http://mpextended.github.com/";
        internal const string ORIGINAL_URL_HEADER = "X-Root-URL";

        private static bool IsRestEnabled
        {
            get
            {
                return OperationContext.Current.IncomingMessageVersion == MessageVersion.None;
            }
        }

        public static string GetCurrentRoot()
        {
            // if a service address is configured, use that
            if (!String.IsNullOrEmpty(Configuration.Services.ServiceAddress))
            {
                return Configuration.Services.ServiceAddress + "MPExtended/";
            }

            try
            {
                // then try the X-Root-URL header
                if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers.AllKeys.Contains(ORIGINAL_URL_HEADER))
                {
                    /*
                     * If MPExtended is behind a web proxy which alters the scheme and port e.g. https://mediaportal.external.com to http://mpserver:4332
                     * then the Host header won't contain enough information for absolute locations such as those in WebStringResult to be set correctly.
                     *
                     * This header allows the proxy to supply the original URL in full to fix this problem.
                     */
                    string val = WebOperationContext.Current.IncomingRequest.Headers[ORIGINAL_URL_HEADER];
                    if (val != null)
                    {
                        try
                        {
                            Uri uri = new Uri(val);
                            string portStr = uri.IsDefaultPort ? String.Empty : ":" + uri.Port;
                            return String.Format("{0}://{1}{2}/MPExtended/", uri.Scheme, uri.Host, portStr);
                        }
                        catch (UriFormatException e)
                        {
                            Log.Error(String.Format("Cannot format current root from {0}", ORIGINAL_URL_HEADER), e);
                        }
                    }
                }

                // try the HTTP host header
                if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers.AllKeys.Contains(HttpRequestHeader.Host.ToString()))
                {
                    string val = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Host];
                    if (val != null)
                        return String.Format("http://{0}/MPExtended/", val);
                }
            }
            catch (InvalidOperationException)
            {
                // probably a net.pipe binding, ignore that
            }

            // then try the current IP address
            if (NetworkInformation.GetIPAddresses().Any())
            {
                return String.Format("http://{0}:{1}/MPExtended/", NetworkInformation.GetIPAddressForUri(), Configuration.Services.Port);
            }

            // last resort: localhost
            return String.Format("http://localhost:{0}/MPExtended/", Configuration.Services.Port);
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
            SetCustomSOAPHeader("responseCode", (int)code);
            if (IsRestEnabled)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = code;
            }
        }

        public static void SetContentLength(long length)
        {
            SetCustomSOAPHeader("contentLength", length);
            if(IsRestEnabled)
            {
                // This doesn't work yet (#96)
                WebOperationContext.Current.OutgoingResponse.ContentLength = length;
                AddHeader("X-Content-Length", length.ToString());
            }
        }


        public static void SetContentType(string type)
        {
            SetCustomSOAPHeader("contentType", type);
            if(IsRestEnabled)
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = type;
            }
        }

        public static void AddHeader(string header, string value)
        {
            SetCustomSOAPHeader(header.Replace("-", "").ToLowerFirst(), value);
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

        internal static MessageHeader CreateCustomSOAPHeader<T>(string name, T value)
        {
            MessageHeader<T> header = new MessageHeader<T>(value);
            MessageHeader untyped = header.GetUntypedHeader(name, HEADER_NAMESPACE);
            return untyped;
        }

        private static void SetCustomSOAPHeader<T>(string name, T value)
        {
            if (!IsRestEnabled)
            {
                OperationContext.Current.OutgoingMessageHeaders.Add(CreateCustomSOAPHeader(name, value));
            }
        }

        public static string GetHeaderValue(string soapHeaderName, string soapHeaderNamespace, string webHeaderName)
        {
            if (OperationContext.Current.IncomingMessageHeaders.FindHeader(soapHeaderName, soapHeaderNamespace) != -1)
            {
                return OperationContext.Current.IncomingMessageHeaders.GetHeader<string>(soapHeaderName, soapHeaderNamespace);
            }

            if (IsRestEnabled && WebOperationContext.Current.IncomingRequest.Headers[webHeaderName] != null)
            {
                return WebOperationContext.Current.IncomingRequest.Headers[webHeaderName];
            }

            return null;
        }

        public static string GetHeaderValue(string soapHeaderName, string webHeaderName)
        {
            return GetHeaderValue(soapHeaderName, HEADER_NAMESPACE, webHeaderName);
        }

        public static string GetHeaderValue(string headerName)
        {
            string soapHeaderName = headerName.Replace("-", "").ToLowerFirst();
            return GetHeaderValue(soapHeaderName, headerName);
        }
    }
}
