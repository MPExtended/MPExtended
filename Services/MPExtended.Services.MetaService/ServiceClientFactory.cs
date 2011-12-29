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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MetaService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.MetaService
{
    internal static class ServiceClientFactory
    {
        public static IMetaService CreateMeta(string ip)
        {
            try
            {
                IMetaService channel = CreateConnection<IMetaService>(String.Format("http://{0}:4322/MPExtended/MetaService", ip));
                if (!channel.TestConnection())
                {
                    return null;
                }

                return channel;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IMediaAccessService CreateLocalMAS()
        {
            return CreateConnection<IMediaAccessService>(String.Format("http://127.0.0.1:4322/MPExtended/MediaAccessService"));
        }

        public static ITVAccessService CreateLocalTAS()
        {
            return CreateConnection<ITVAccessService>(String.Format("http://127.0.0.1:4322/MPExtended/TVAccessService"));
        }

        public static IWebStreamingService CreateLocalWSS()
        {
            return CreateConnection<IWebStreamingService>(String.Format("http://127.0.0.1:4322/MPExtended/StreamingService"));
        }

        private static T CreateConnection<T>(string url)
        {
            ChannelFactory<T> factory = new ChannelFactory<T>(
                new BasicHttpBinding() { MaxReceivedMessageSize = 100000000 },
                new EndpointAddress(url)
            );

            T channel = factory.CreateChannel();
            return channel;
        }
    }
}
