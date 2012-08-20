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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Services.UserSessionService.Interfaces;

namespace MPExtended.Libraries.Client
{
    internal class ServiceSet : IServiceSet
    {
        private IServiceAddressSet addressSet;
        private string username;
        private string password;

        private IMediaAccessService MASConnection;
        private IWebStreamingService WSSForMAS;
        private IStreamingService StreamForMAS;
        private ITVAccessService TASConnection;
        private IWebStreamingService WSSForTAS;
        private IStreamingService StreamForTAS;

        public ServiceSet(IServiceAddressSet addrSet, string username, string password)
        {
            addressSet = addrSet;
            this.username = username;
            this.password = password;
        }

        public ServiceSet(IServiceAddressSet addrSet)
            : this (addrSet, null, null)
        {
        }

        ~ServiceSet()
        {
            CloseConnection(MASConnection as ICommunicationObject);
            CloseConnection(WSSForMAS as ICommunicationObject);
            CloseConnection(StreamForMAS as ICommunicationObject);
            CloseConnection(TASConnection as ICommunicationObject);
            CloseConnection(WSSForTAS as ICommunicationObject);
            CloseConnection(StreamForTAS as ICommunicationObject);
        }

        public IServiceAddressSet Addresses
        {
            get
            {
                return addressSet;
            }
        }

        public IMediaAccessService MAS
        {
            get
            {
                if (MASConnection == null || ((ICommunicationObject)MASConnection).State == CommunicationState.Faulted)
                {
                    MASConnection = CreateConnection<IMediaAccessService>(addressSet.MAS, "MediaAccessService", username, password);
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

        public IWebStreamingService MASStreamControl
        {
            get
            {
                if (WSSForMAS == null || ((ICommunicationObject)WSSForMAS).State == CommunicationState.Faulted)
                {
                    WSSForMAS = CreateConnection<IWebStreamingService>(addressSet.MASStream, "StreamingService/soap", username, password);
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
                    StreamForMAS = CreateConnection<IStreamingService>(addressSet.MASStream, "StreamingService/soapstream", username, password, true);
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

        public ITVAccessService TAS
        {
            get
            {
                if (TASConnection == null || ((ICommunicationObject)TASConnection).State == CommunicationState.Faulted)
                {
                    TASConnection = CreateConnection<ITVAccessService>(addressSet.TAS, "TVAccessService", username, password);
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

        public IWebStreamingService TASStreamControl
        {
            get
            {
                if (WSSForTAS == null || ((ICommunicationObject)WSSForTAS).State == CommunicationState.Faulted)
                {
                    WSSForTAS = CreateConnection<IWebStreamingService>(addressSet.TASStream, "StreamingService/soap", username, password);
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
                    StreamForTAS = CreateConnection<IStreamingService>(addressSet.TASStream, "StreamingService/soapstream", username, password, true);
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

        private T CreateConnection<T>(string address, string service)
        {
            return CreateConnection<T>(address, service, null, null, false);
        }

        private T CreateConnection<T>(string address, string service, string username, string password)
        {
            return CreateConnection<T>(address, service, username, password, false);
        }

        private T CreateConnection<T>(string address, string service, string username, string password, bool streamConnection)
        {
            ConnectionFactory<T> factory = new ConnectionFactory<T>()
            {
                Username = username,
                Password = password,
                UsePipeForLocalhost = true,
                CreateStreamBindings = streamConnection
            };

            return factory.CreateConnection(address, String.Format("/MPExtended/{0}", service));
        }

        private void CloseConnection(ICommunicationObject channel)
        {
            ConnectionFactory.DisposeConnection(channel);
        }
    }
}
