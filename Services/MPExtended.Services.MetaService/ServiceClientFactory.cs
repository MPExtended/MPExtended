#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Text;
using System.ServiceModel;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.WCF;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MetaService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.MetaService
{
    internal static class ServiceClientFactory
    {
        public static IMetaService CreateMeta(IPEndPoint address)
        {
            return CreateMeta(address.Address.ToString(), address.Port);
        }

        public static IMetaService CreateMeta(string ip)
        {
            return CreateMeta(ip, Configuration.DEFAULT_PORT);
        }

        public static IMetaService CreateMeta(string ip, int port)
        {
            try
            {
                var channel = new ClientFactory<IMetaService>().CreateConnection(String.Format("{0}:{1}", ip, port), "MPExtended/MetaService");
                if (!channel.TestConnection())
                    return null;

                return channel;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static ClientFactory<T> CreateClientFactory<T>()
        {
            var factory = new ClientFactory<T>();
            factory.UsePipeForLocalhost = true;
            return factory;
        }

        public static IMediaAccessService CreateLocalMAS()
        {
            return CreateClientFactory<IMediaAccessService>().CreateConnection("127.0.0.1", "MPExtended/MediaAccessService");
        }

        public static ITVAccessService CreateLocalTAS()
        {
            return CreateClientFactory<ITVAccessService>().CreateConnection("127.0.0.1", "MPExtended/TVAccessService");
        }

        public static IWebStreamingService CreateLocalWSS()
        {
            return CreateClientFactory<IWebStreamingService>().CreateConnection("127.0.0.1", "MPExtended/StreamingService/soap");
        }
    }
}
