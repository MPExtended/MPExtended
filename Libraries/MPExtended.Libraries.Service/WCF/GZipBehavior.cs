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
using Microsoft.ServiceModel.Samples;
using System.Text;
using System.Xml;

namespace MPExtended.Libraries.Service.WCF
{
    internal class GzipMessageInspector : IDispatchMessageInspector
	{
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext) {
            //Log.Debug("Got a request " + request.GetType().FullName);

            try
            {
                var prop = request.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
                var accept = prop.Headers[HttpRequestHeader.AcceptEncoding];
                if (!string.IsNullOrEmpty(accept) && accept.Contains("gzip"))
                {
                    Log.Debug(GetType().FullName + "::AfterReceiveRequest enable gzip for this request");
                    OperationContext.Current.Extensions.Add(new GzipContext());
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not process request", e);
            }

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState) {
            //Log.Debug("Got a response " + reply.GetType().FullName);

            try
            {
                if (OperationContext.Current.Extensions.Find<GzipContext>() != null)
                {
                    Log.Debug(GetType().FullName + "::BeforeSendReply set gzip for this response");
                    var prop = reply.Properties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
                    prop.Headers[HttpResponseHeader.ContentEncoding] = "gzip";
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not process request", e);
            }
        }
    }

    internal class GzipEndpointBehavior : IEndpointBehavior
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
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new GzipMessageInspector());
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }

    public class GzipBehavior : BehaviorExtensionElement
	{
        public override Type BehaviorType { get { return typeof(GzipEndpointBehavior); } }
        protected override object CreateBehavior() { return new GzipEndpointBehavior(); }
    }
}
