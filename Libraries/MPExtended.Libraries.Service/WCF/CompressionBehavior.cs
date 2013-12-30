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
//
// Based on code from Francesco De Vittori, 
// http://www.frenk.com/2009/12/gzip-compression-wcfsilverlight/
#endregion

using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using MPExtended.Libraries.Service.Compression;

namespace MPExtended.Libraries.Service.WCF
{
    internal class CompressionMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            try
            {
                var prop = request.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
                var accept = prop.Headers[HttpRequestHeader.AcceptEncoding];
                if (!String.IsNullOrEmpty(accept))
                {
                    CompressionType type = CompressionType.None;

                    foreach (string encoding in accept.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (encoding.Trim() == "gzip")
                        {
                            type = CompressionType.GZip;
                        }
                        else if (encoding.Trim() == "deflate")
                        {
                            type = CompressionType.Deflate;
                        }

                        if (type != CompressionType.None)
                        {
                            Log.Trace("CompressionMessageInspector::AfterReceiveRequest enable {0} for this request", type);
                            OperationContext.Current.Extensions.Add(new CompressionContext(type));
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not process request", e);
            }

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            try
            {
                CompressionContext context = OperationContext.Current.Extensions.Find<CompressionContext>();
                if (context != null)
                {
                    var prop = reply.Properties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
                    Log.Trace("CompressionMessageInspector::BeforeSendReply set {0} encoding for {1} response", context.Type, prop.Headers[HttpResponseHeader.ContentType]);
                    if (context.Type == CompressionType.GZip)
                    {
                        prop.Headers[HttpResponseHeader.ContentEncoding] = "gzip";
                    }
                    else if (context.Type == CompressionType.Deflate)
                    {
                        prop.Headers[HttpResponseHeader.ContentEncoding] = "deflate";
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not process request", e);
            }
        }
    }

    internal class CompressionEndpointBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            throw new Exception("Behavior not supported on the client side");
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new CompressionMessageInspector());
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }

    public class CompressionBehavior : BehaviorExtensionElement
    {
        public override Type BehaviorType { get { return typeof(CompressionEndpointBehavior); } }
        protected override object CreateBehavior() { return new CompressionEndpointBehavior(); }
    }
}
