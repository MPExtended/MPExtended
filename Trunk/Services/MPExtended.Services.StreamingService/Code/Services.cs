using System;
using System.Collections.Generic;
using System.ServiceModel;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    class WebServices
    {
        private static IMediaAccessService _media;
        private static ITVAccessService _tv;
        private static Dictionary<string, WebVirtualCard> _timeshiftings;

        static WebServices()
        {
            _timeshiftings = new Dictionary<string, WebVirtualCard>();
        }

        public static IMediaAccessService Media
        {
            get
            {
                if (_media == null)
                    _media = ChannelFactory<IMediaAccessService>.CreateChannel(
                        new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 },
                        new EndpointAddress("net.pipe://localhost/MPExtended/MediaAccessService")
                    );
                return _media;
            }
        }

        public static ITVAccessService TV
        {
            get
            {
                if (_tv == null)
                    _tv = ChannelFactory<ITVAccessService>.CreateChannel(
                        new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 },
                        new EndpointAddress("net.pipe://localhost/MPExtended/TVAccessService")
                    );
                return _tv;
            }
        }

        public static WebVirtualCard GetTimeshifting(string id)
        {
            if (_timeshiftings.ContainsKey(id))
                return _timeshiftings[id];
            return null;
        }

        public static void SaveTimeshifting(string id, WebVirtualCard card)
        {
            if (card == null)
            {
                _timeshiftings.Remove(id);
            }
            else
            {
               _timeshiftings[id] = card;
            }
        }
    }
}
