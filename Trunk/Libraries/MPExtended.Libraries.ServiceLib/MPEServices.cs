#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;
using System.ServiceModel;

namespace MPExtended.Libraries.ServiceLib
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
    }
}