#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service.Network;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service.WCF
{
    public class ClientFactory<TService> : ConnectionFactory<TService>
    {
        protected override bool UsePipeBinding(string address)
        {
            string host = address.Contains(':') ? address.Substring(0, address.IndexOf(':')) : address;
            return NetworkInformation.IsLocalAddress(host);
        }

        public TService CreateLocalTcpConnection(int port, string path)
        {
            NetTcpBinding binding = new NetTcpBinding()
            {
                MaxReceivedMessageSize = Int32.MaxValue,
                ReceiveTimeout = new TimeSpan(1, 0, 0),
                SendTimeout = new TimeSpan(1, 0, 0),
            };
            binding.ReliableSession.Enabled = true;
            binding.ReliableSession.Ordered = true;
            binding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
            binding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;

            var endpointAddress = new EndpointAddress(String.Format("net.tcp://localhost:{0}/{1}", port, path));
            var factory = new ChannelFactory<TService>(binding, endpointAddress);

            foreach (OperationDescription operation in factory.Endpoint.Contract.Operations)
            {
                operation.Behaviors.Find<DataContractSerializerOperationBehavior>().MaxItemsInObjectGraph = Int32.MaxValue;
            }

            return factory.CreateChannel();
        }
    }
}
