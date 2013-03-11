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

namespace MPExtended.Libraries.Client
{
    public class ConnectionFactory<TService>
    {
        public bool UsePipeForLocalhost { get; set; }
        public bool CreateStreamBindings { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public TService CreateConnection(string address, string path)
        {
            ChannelFactory<TService> factory;
            factory = UsePipeForLocalhost && UsePipeBinding(address) ?
                CreateLocalChannelFactory(address, path) :
                CreateRemoteChannelFactory(address, path);

            foreach (OperationDescription operation in factory.Endpoint.Contract.Operations)
            {
                operation.Behaviors.Find<DataContractSerializerOperationBehavior>().MaxItemsInObjectGraph = Int32.MaxValue;
            }

            return factory.CreateChannel();
        }

        protected virtual bool UsePipeBinding(string address)
        {
            return address == "127.0.0.1" || address == "localhost" || address.StartsWith("127.0.0.1:") || address.StartsWith("localhost:");
        }

        protected virtual ChannelFactory<TService> CreateLocalChannelFactory(string address, string path)
        {
            var binding = new NetNamedPipeBinding()
            {
                MaxReceivedMessageSize = Int32.MaxValue
            };

            if (CreateStreamBindings)
                binding.TransferMode = TransferMode.StreamedResponse;
            binding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
            binding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;

            var endpointAddress = new EndpointAddress(String.Format("net.pipe://127.0.0.1/{0}", path));
            return new ChannelFactory<TService>(binding, endpointAddress);
        }

        protected virtual ChannelFactory<TService> CreateRemoteChannelFactory(string address, string path)
        {
            var binding = new BasicHttpBinding()
            {
                MaxReceivedMessageSize = Int32.MaxValue
            };

            if (CreateStreamBindings)
                binding.TransferMode = TransferMode.StreamedResponse;
            binding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
            binding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;

            if (!String.IsNullOrEmpty(Username) && !String.IsNullOrEmpty(Password))
            {
                binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            }

            var endpointAddress = new EndpointAddress(String.Format("http://{0}/{1}", address, path));
            var factory = new ChannelFactory<TService>(binding, endpointAddress);

            if (!String.IsNullOrEmpty(Username) && !String.IsNullOrEmpty(Password))
            {
                factory.Credentials.UserName.UserName = Username;
                factory.Credentials.UserName.Password = Password;
            }

            return factory;
        }

        public void DisposeConnection(TService connection)
        {
            ConnectionFactory.DisposeConnection(connection);
        }
    }
}
