// Based on http://www.frenk.com/2009/12/gzip-compression-wcfsilverlight/

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
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext) {
            try
            {
                var prop = request.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
                var accept = prop.Headers[HttpRequestHeader.AcceptEncoding];
                if (!string.IsNullOrEmpty(accept))
                {
                    foreach (string encoding in accept.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (encoding.Trim().Equals("gzip"))
                        {
                            Log.Trace(GetType().FullName + "::AfterReceiveRequest enable gzip for this request");
                            OperationContext.Current.Extensions.Add(new CompressionContext(CompressionType.GZIP));
                            break;
                        }
                        else if (encoding.Trim().Equals("deflate"))
                        {
                            Log.Trace(GetType().FullName + "::AfterReceiveRequest enable deflate for this request");
                            OperationContext.Current.Extensions.Add(new CompressionContext(CompressionType.DEFLATE));
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

        public void BeforeSendReply(ref Message reply, object correlationState) {
            try
            {
                CompressionContext context = OperationContext.Current.Extensions.Find<CompressionContext>();
                if (context != null)
                {
                    var prop = reply.Properties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
                    if (context.Type == CompressionType.GZIP)
                    {
                        Log.Trace(GetType().FullName + "::BeforeSendReply set gzip for this response");
                        prop.Headers[HttpResponseHeader.ContentEncoding] = "gzip";
                    }
                    else if (context.Type == CompressionType.DEFLATE)
                    {
                        Log.Trace(GetType().FullName + "::BeforeSendReply set deflate for this response");
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
