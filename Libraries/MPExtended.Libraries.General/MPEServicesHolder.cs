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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Libraries.General
{
    public class MPEServicesHolder
    {
        private const int MAX_ITEMS_IN_OBJECT_GRAPH = Int32.MaxValue;
        private const int MAX_STRING_CONTENT_LENGTH = Int32.MaxValue;
        private const int MAX_ARRAY_LENGTH = Int32.MaxValue;

        private string MASUrl;
        private string TASUrl;

        private IMediaAccessService MASConnection;
        private IWebStreamingService WSSForMAS;
        private IStreamingService StreamForMAS;
        private ITVAccessService TASConnection;
        private IWebStreamingService WSSForTAS;
        private IStreamingService StreamForTAS;

        public MPEServicesHolder(string mas, string tas)
        {
            MASUrl = mas;
            TASUrl = tas;
        }

        public MPEServicesHolder(bool useLocalhostByDefault)
        {
            try
            {
                MASUrl = Configuration.Services.MASConnection;
                TASUrl = Configuration.Services.TASConnection;
            }
            catch (Exception)
            {
                if (useLocalhostByDefault)
                {
                    MASUrl = "auto://127.0.0.1:4322";
                    TASUrl = "auto://127.0.0.1:4322";
                }
                else
                {
                    throw;
                }
            }
        }

        public MPEServicesHolder()
            : this(false)
        {
        }

        ~MPEServicesHolder()
        {
            CloseConnection(MASConnection as ICommunicationObject);
            CloseConnection(WSSForMAS as ICommunicationObject);
            CloseConnection(StreamForMAS as ICommunicationObject);
            CloseConnection(TASConnection as ICommunicationObject);
            CloseConnection(WSSForTAS as ICommunicationObject);
            CloseConnection(StreamForTAS as ICommunicationObject);
        }

        public IMediaAccessService MAS
        {
            get
            {
                if (MASConnection == null || ((ICommunicationObject)MASConnection).State == CommunicationState.Faulted)
                {
                    MASConnection = CreateConnection<IMediaAccessService>(MASUrl, "MediaAccessService");
                }

                return MASConnection;
            }
        }

        public bool HasMASConnection
        {
            get
            {
                try
                {
                    MAS.GetServiceDescription();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool IsMASLocal
        {
            get
            {
                Uri uri = new Uri(MASUrl);
                return uri.IsLoopback;
            }
        }

        public IWebStreamingService MASStreamControl
        {
            get
            {
                if (WSSForMAS == null || ((ICommunicationObject)WSSForMAS).State == CommunicationState.Faulted)
                {
                    WSSForMAS = CreateConnection<IWebStreamingService>(MASUrl, "StreamingService");
                }

                return WSSForMAS;
            }
        }

        public IStreamingService MASStream
        {
            get
            {
                if (StreamForMAS == null || ((ICommunicationObject)StreamForMAS).State == CommunicationState.Faulted)
                {
                    StreamForMAS = CreateConnection<IStreamingService>(MASUrl, "StreamingService");
                }

                return StreamForMAS;
            }
        }

        public bool HasMASStreamConnection
        {
            get
            {
                try
                {
                    MASStreamControl.GetServiceDescription();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool IsMASStreamLocal
        {
            get
            {
                Uri uri = new Uri(MASUrl);
                return uri.IsLoopback;
            }
        }

        public string HttpMASStreamRoot
        {
            get
            {
                Uri uri = new Uri(MASUrl);
                return String.Format("http://{0}:{1}/MPExtended/StreamingService/stream/", uri.Host, uri.Port);
            }
        }

        public ITVAccessService TAS
        {
            get
            {
                if (TASConnection == null || ((ICommunicationObject)TASConnection).State == CommunicationState.Faulted)
                {
                    TASConnection = CreateConnection<ITVAccessService>(TASUrl, "TVAccessService");
                }

                return TASConnection;
            }
        }

        public bool HasTASConnection
        {
            get
            {
                try
                {
                    return TAS.TestConnectionToTVService();
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool IsTASLocal
        {
            get
            {
                Uri uri = new Uri(TASUrl);
                return uri.IsLoopback;
            }
        }

        public IWebStreamingService TASStreamControl
        {
            get
            {
                if (WSSForTAS == null || ((ICommunicationObject)WSSForTAS).State == CommunicationState.Faulted)
                {
                    WSSForTAS = CreateConnection<IWebStreamingService>(TASUrl, "StreamingService");
                }

                return WSSForTAS;
            }
        }

        public IStreamingService TASStream
        {
            get
            {
                if (StreamForTAS == null || ((ICommunicationObject)StreamForTAS).State == CommunicationState.Faulted)
                {
                    StreamForTAS = CreateConnection<IStreamingService>(TASUrl, "StreamingService");
                }

                return StreamForTAS;
            }
        }

        public bool HasTASStreamConnection
        {
            get
            {
                try
                {
                    TASStreamControl.GetServiceDescription();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool IsTASStreamLocal
        {
            get
            {
                Uri uri = new Uri(TASUrl);
                return uri.IsLoopback;
            }
        }

        public string HttpTASStreamRoot
        {
            get
            {
                Uri uri = new Uri(TASUrl);
                return String.Format("http://{0}:{1}/MPExtended/StreamingService/stream/", uri.Host, uri.Port);
            }
        }

        private T CreateConnection<T>(string address, string service)
        {
            Uri addr = new Uri(address);

            // This allows us to introduce for example mas04://127.0.0.1/ in the future to allow communication with an older version of the services. 
            if (addr.Scheme != "auto")
            {
                Log.Error("Encountered unknown protocol while setting up {0} connection to {1}", service, address);
                return default(T);
            }

            // create channel factory
            ChannelFactory<T> factory = null;
            if (addr.IsLoopback && addr.Port == 4322)
            {
                NetNamedPipeBinding binding = new NetNamedPipeBinding()
                {
                    MaxReceivedMessageSize = 100000000
                };
                binding.ReaderQuotas.MaxArrayLength = MAX_ARRAY_LENGTH;
                binding.ReaderQuotas.MaxStringContentLength = MAX_STRING_CONTENT_LENGTH;

                factory = new ChannelFactory<T>(
                    binding,
                    new EndpointAddress(String.Format("net.pipe://127.0.0.1/MPExtended/{0}", service))
                );
            }
            else
            {
                BasicHttpBinding binding = new BasicHttpBinding()
                {
                    MaxReceivedMessageSize = 100000000
                };
                binding.ReaderQuotas.MaxArrayLength = MAX_ARRAY_LENGTH;
                binding.ReaderQuotas.MaxStringContentLength = MAX_STRING_CONTENT_LENGTH;

                if (addr.UserInfo != null && addr.UserInfo.Length > 0 && addr.UserInfo.Contains(':'))
                {
                    binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                    binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
                }

                factory = new ChannelFactory<T>(
                    binding,
                    new EndpointAddress(String.Format("http://{0}:{1}/MPExtended/{2}", addr.Host, addr.Port, service))
                );

                if (addr.UserInfo != null && addr.UserInfo.Length > 0 && addr.UserInfo.Contains(':'))
                {
                    factory.Credentials.UserName.UserName = addr.UserInfo.Substring(0, addr.UserInfo.IndexOf(':'));
                    factory.Credentials.UserName.Password = addr.UserInfo.Substring(addr.UserInfo.IndexOf(':') + 1);
                }
            }

            // set MaxItemsInObjectGraph parameter for all operations
            foreach(OperationDescription operation in factory.Endpoint.Contract.Operations)
            {
                operation.Behaviors.Find<DataContractSerializerOperationBehavior>().MaxItemsInObjectGraph = MAX_ITEMS_IN_OBJECT_GRAPH;
            }

            // return
            return factory.CreateChannel();
        }

        private void CloseConnection(ICommunicationObject channel)
        {
            if (channel == null)
                return;

            if (channel.State == CommunicationState.Opening || channel.State == CommunicationState.Opened || channel.State == CommunicationState.Created)
            {
                try
                {
                    channel.Close();
                }
                catch (Exception)
                {
                }
            }
        }
    }
}