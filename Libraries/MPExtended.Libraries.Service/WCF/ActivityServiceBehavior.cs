#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace MPExtended.Libraries.Service.WCF
{
    public class ActivityTracer : IDispatchMessageInspector
    {
        public static DateTime LastMessageReceived { get; private set; }

        static ActivityTracer()
        {
            LastMessageReceived = DateTime.Now;
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            LastMessageReceived = DateTime.Now;
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            return;
        }
    }

    public class ActivityTracingServiceBehavior : IServiceBehavior
    {
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            return;
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            ActivityTracer instance = new ActivityTracer();
            foreach (ChannelDispatcher chDisp in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher epDisp in chDisp.Endpoints)
                {
                    epDisp.DispatchRuntime.MessageInspectors.Add(instance);
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            return;
        }
    }

    public class ActivityServiceBehavior : BehaviorExtensionElement
    {
        public override Type BehaviorType { get { return typeof(ActivityTracingServiceBehavior); } }
        protected override object CreateBehavior() { return new ActivityTracingServiceBehavior(); }
    }
}
