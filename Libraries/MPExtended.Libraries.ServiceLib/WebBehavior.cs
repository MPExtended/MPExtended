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
using System.Runtime.Serialization.Json;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using MPExtended.Libraries.General;

namespace MPExtended.Libraries.ServiceLib
{
    public class WebExceptionHandling : WebHttpBehavior, IErrorHandler
    {
        protected override void AddServerErrorHandlers(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Clear();
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(this);
        }

        protected override QueryStringConverter GetQueryStringConverter(OperationDescription operationDescription)
        {
            return new CustomQueryStringConverter();
        }

        public bool HandleError(Exception error)
        {
            return true;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            Log.Error("Unhandled exception in service (JSON interface)", error);
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

    public class JsonError
    {
        public string ExceptionType { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }

    public class WebBehavior : BehaviorExtensionElement
    {
        protected override object CreateBehavior() { return new WebExceptionHandling(); }
        public override Type BehaviorType { get { return typeof(WebExceptionHandling); } }
    }
}