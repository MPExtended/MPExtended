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
using System.Text;
using System.Threading.Tasks;
using ZeroconfService;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.MetaService.Interfaces;

namespace MPExtended.Services.MetaService
{
    internal class ZeroconfPublisher : IServicePublisher
    {
        private List<NetService> publishedServices = new List<NetService>();
        private const string SET_SERVICE_TYPE = "_mpextended-set._tcp.";

        public bool Publish(IServiceDetector detector)
        {
            // old style services
            foreach (var srv in Installation.GetInstalledServices())
            {
                if (!ZeroconfDiscoverer.serviceTypes.ContainsKey(srv.ToWebService()))
                    continue;

                Dictionary<string, string> additionalData = new Dictionary<string, string>();
                additionalData["hwAddr"] = String.Join(";", NetworkInformation.GetMACAddresses());
                additionalData["netbios-name"] = System.Environment.MachineName;
                additionalData["external-ip"] = IPAddressUtils.GetExternalIpAddress();

                NetService net = new NetService(ZeroconfDiscoverer.DOMAIN, ZeroconfDiscoverer.serviceTypes[srv.ToWebService()], Configuration.Services.GetServiceName(), srv.Port);
                net.AllowMultithreadedCallbacks = true;
                net.TXTRecordData = NetService.DataFromTXTRecordDictionary(additionalData);
                net.DidPublishService += new NetService.ServicePublished(PublishedService);
                net.DidNotPublishService += new NetService.ServiceNotPublished(FailedToPublishService);
                net.Publish();
                publishedServices.Add(net);
            }
    
            // new style service sets
            foreach (WebServiceSet set in detector.CreateSetComposer().ComposeUnique())
            {
                Log.Debug("Publishing service set {0}", set);
                Dictionary<string, string> additionalData = new Dictionary<string, string>();
                additionalData["mac"] = String.Join(";", NetworkInformation.GetMACAddresses());
                additionalData["netbios-name"] = System.Environment.MachineName;
                additionalData["external-ip"] = IPAddressUtils.GetExternalIpAddress();
                additionalData["mas"] = set.MAS != null ? set.MAS : String.Empty;
                additionalData["masstream"] = set.MASStream != null ? set.MASStream : String.Empty;
                additionalData["tas"] = set.TAS != null ? set.TAS : String.Empty;
                additionalData["tasstream"] = set.TASStream != null ? set.TASStream : String.Empty;
                additionalData["ui"] = set.UI != null ? set.UI : String.Empty;

                NetService net = new NetService(ZeroconfDiscoverer.DOMAIN, SET_SERVICE_TYPE, Configuration.Services.GetServiceName(), Configuration.Services.Port);
                net.AllowMultithreadedCallbacks = true;
                net.TXTRecordData = NetService.DataFromTXTRecordDictionary(additionalData);
                net.DidPublishService += new NetService.ServicePublished(PublishedService);
                net.DidNotPublishService += new NetService.ServiceNotPublished(FailedToPublishService);
                net.Publish();
                publishedServices.Add(net);
            }

            return true;
        }

        public void PublishAsync(IServiceDetector detector)
        {
            Task.Factory.StartNew(() => Publish(detector));
        }

        public void Unpublish()
        {
            // bonjour cleans up automatically
        }

        private void FailedToPublishService(NetService service, DNSServiceException error)
        {
            string msg = String.Format("Failed to publish service {0}", service.Name);
            Log.Warn(msg, error);
        }

        private void PublishedService(NetService service)
        {
            Log.Trace("Published service {0}", service.Type);
        }
    }
}
