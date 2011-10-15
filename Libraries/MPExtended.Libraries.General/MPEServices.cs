#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using System.ServiceModel;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Libraries.General
{
    public static class MPEServices
    {
        private static IMediaAccessService _mediaService;
        private static ITVAccessService _tvService;
        private static IWebStreamingService _webStreamService;
        private static IStreamingService _streamService;

        public static IMediaAccessService NetPipeMediaAccessService
        {
            get
            {
                if (_mediaService == null || ((ICommunicationObject)_mediaService).State == CommunicationState.Faulted)
                {
                    _mediaService = ChannelFactory<IMediaAccessService>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 }, new EndpointAddress("net.pipe://localhost/MPExtended/MediaAccessService"));
                }

                return _mediaService;
            }
        }

        public static bool HasMediaAccessConnection
        {
            get
            {
                try
                {
                    NetPipeMediaAccessService.GetServiceDescription();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static ITVAccessService NetPipeTVAccessService
        {
            get
            {
                if (_tvService == null || ((ICommunicationObject)_tvService).State == CommunicationState.Faulted)
                {
                    _tvService = ChannelFactory<ITVAccessService>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 }, new EndpointAddress("net.pipe://localhost/MPExtended/TVAccessService"));
                }
                return _tvService;
            }
        }

        public static bool HasTVAccessConnection
        {
            get
            {
                try
                {
                    return NetPipeTVAccessService.TestConnectionToTVService();
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static IWebStreamingService NetPipeWebStreamService
        {
            get
            {
                if (_webStreamService == null || ((ICommunicationObject)_webStreamService).State == CommunicationState.Faulted)
                {
                    _webStreamService = ChannelFactory<IWebStreamingService>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 }, new EndpointAddress("net.pipe://localhost/MPExtended/StreamingService"));
                }

                return _webStreamService;
            }
        }

        public static IStreamingService NetPipeStreams
        {
            get
            {
                if (_streamService == null || ((ICommunicationObject)_streamService).State == CommunicationState.Faulted)
                {
                    _streamService = ChannelFactory<IStreamingService>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 }, new EndpointAddress("net.pipe://localhost/MPExtended/StreamingService"));
                }

                return _streamService;
            }
        }

        public static bool HasStreamConnection
        {
            get
            {
                try
                {
                    NetPipeWebStreamService.GetServiceDescription();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }


        /////// ======================================================================= \\\\\\\
        private static IMediaAccessService MASConnection;
        private static IWebStreamingService WSSForMAS;
        private static IStreamingService StreamForMAS;
        private static ITVAccessService TASConnection;
        private static IWebStreamingService WSSForTAS;
        private static IStreamingService StreamForTAS;

        public static IMediaAccessService MAS
        {
            get
            {
                if (MASConnection == null || ((ICommunicationObject)MASConnection).State == CommunicationState.Faulted)
                {
                    MASConnection = CreateConnection<IMediaAccessService>(Installation.MASAddress, 4322, "MediaAccessService");
                }

                return MASConnection;
            }
        }

        public static IWebStreamingService MASStreamControl
        {
            get
            {
                if (WSSForMAS == null || ((ICommunicationObject)WSSForMAS).State == CommunicationState.Faulted)
                {
                    WSSForMAS = CreateConnection<IWebStreamingService>(Installation.MASAddress, 4322, "StreamingService");
                }

                return WSSForMAS;
            }
        }

        public static IStreamingService MASStream
        {
            get
            {
                if (StreamForMAS == null || ((ICommunicationObject)StreamForMAS).State == CommunicationState.Faulted)
                {
                    StreamForMAS = CreateConnection<IStreamingService>(Installation.MASAddress, 4322, "StreamingService");
                }

                return StreamForMAS;
            }
        }

        public static ITVAccessService TAS
        {
            get
            {
                if (TASConnection == null || ((ICommunicationObject)TASConnection).State == CommunicationState.Faulted)
                {
                    TASConnection = CreateConnection<ITVAccessService>(Installation.TASAddress, 4321, "TVAccessService");
                }

                return TASConnection;
            }
        }

        public static IWebStreamingService TASStreamControl
        {
            get
            {
                if (WSSForTAS == null || ((ICommunicationObject)WSSForTAS).State == CommunicationState.Faulted)
                {
                    WSSForTAS = CreateConnection<IWebStreamingService>(Installation.TASAddress, 4321, "StreamingService");
                }

                return WSSForTAS;
            }
        }

        public static IStreamingService TASStream
        {
            get
            {
                if (StreamForTAS == null || ((ICommunicationObject)StreamForTAS).State == CommunicationState.Faulted)
                {
                    StreamForTAS = CreateConnection<IStreamingService>(Installation.TASAddress, 4321, "StreamingService");
                }

                return StreamForTAS;
            }
        }


        private static T CreateConnection<T>(string address, int port, string service)
        {
            // TODO: Use System.Uri
            if (address == "127.0.0.1" || address == "localhost")
            {
                return ChannelFactory<T>.CreateChannel(
                    new NetNamedPipeBinding() { MaxReceivedMessageSize = 100000000 },
                    new EndpointAddress(String.Format("net.pipe://{0}/MPExtended/{1}", address, service))
                );
            }
            else
            {
                return ChannelFactory<T>.CreateChannel(
                    new BasicHttpBinding() { MaxReceivedMessageSize = 100000000 },
                    new EndpointAddress(String.Format("http://{0}:{1}/MPExtended/{2}", address, port, service))
                );
            }
        }
    }
}