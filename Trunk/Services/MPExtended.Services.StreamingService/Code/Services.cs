#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers
// http://mpextended.codeplex.com/
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
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
#endregion

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
