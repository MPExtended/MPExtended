using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.ServiceModel.Dispatcher;

namespace MPExtended.Services.MediaAccessService
{
    public class WebHttpWithCustomExceptionHandling : WebHttpBehavior, IErrorHandler
    {
        protected override void AddServerErrorHandlers(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Clear();
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(this);
        }

        public bool HandleError(Exception error)
        {
            return true;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            JsonError errorObject = new JsonError { ExceptionType = error.GetType().ToString(), Message = error.Message, StackTrace = error.StackTrace };

            // format the exception/message with JSON
            fault = Message.CreateMessage(version, "", errorObject, new DataContractJsonSerializer(typeof(JsonError)));

            // set http status code to 500 and content type to JSON
            HttpResponseMessageProperty httpResponse = new HttpResponseMessageProperty();
            httpResponse.Headers[HttpResponseHeader.ContentType] = "application/json";
            httpResponse.StatusCode = HttpStatusCode.InternalServerError;

            fault.Properties.Add(WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(WebContentFormat.Json));
            fault.Properties.Add(HttpResponseMessageProperty.Name, httpResponse);
        }
    }

    public class WebHttpWithCustomExceptionHandlingBehavior : BehaviorExtensionElement
    {
        protected override object CreateBehavior() { return new WebHttpWithCustomExceptionHandling(); }

        public override Type BehaviorType { get { return typeof(WebHttpWithCustomExceptionHandling); } }
    }

    public class JsonError
    {
        public string ExceptionType { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}
