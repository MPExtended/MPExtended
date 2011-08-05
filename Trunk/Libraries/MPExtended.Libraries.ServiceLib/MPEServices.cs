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
                    try
                    {
                        _mediaService = ChannelFactory<IMediaAccessService>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 }, new EndpointAddress("net.pipe://localhost/MPExtended/MediaAccessService"));

                    }
                    catch (Exception ex)
                    {

                        Log.Error("Exception in ServiceInstance", ex);
                    }
                }
                return _mediaService;
            }
        }

        public static ITVAccessService NetPipeTVService
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
    }
}
