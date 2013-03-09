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
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Network;
using MPExtended.Services.MetaService.Interfaces;
using ZeroconfService;

namespace MPExtended.Services.MetaService
{
    internal class ZeroconfDiscoverer : INetworkDiscoverer
    {
// Yeah, I know it isn't used, but this is here for the future.
#pragma warning disable 67
        public event EventHandler<ServiceEventArgs> ServiceFound;
        public event EventHandler<ServiceEventArgs> ServiceDisappeared;
#pragma warning restore 67

        internal const int TIMEOUT = 10;
        internal const string DOMAIN = "";

        internal static Dictionary<WebService, string> serviceTypes = new Dictionary<WebService, string>()
        {
            { WebService.MediaAccessService, "_mpextended-mas._tcp." },
            { WebService.TVAccessService, "_mpextended-tas._tcp." },
            { WebService.UserSessionService, "_mpextended-uss._tcp." },
            { WebService.StreamingService, "_mpextended-wss._tcp." }
        };

        private bool? isEnabled = null;
        private NetServiceBrowser browser;

        public bool IsAvailable()
        {
            if (!isEnabled.HasValue)
            {
                isEnabled = CheckBonjourEnabled();
            }
            return isEnabled.Value;
        }

        private bool CheckBonjourEnabled()
        {
            if (!Configuration.Services.BonjourEnabled)
            {
                return false;
            }

            try
            {
                Version ver = NetService.DaemonVersion;
                Log.Debug("Bonjour version {0} installed", ver.ToString());
                return true;
            }
            catch (Exception)
            {
                Log.Trace("Bonjour not installed");
                return false;
            }
        }

        public IServicePublisher GetPublisher()
        {
            return new ZeroconfPublisher();
        }

        public void StartServiceSearch(params WebService[] service)
        {
            browser = new NetServiceBrowser();
            browser.AllowMultithreadedCallbacks = true;
            browser.DidFindService += new NetServiceBrowser.ServiceFound(DiscoverFoundService);
            browser.DidRemoveService += new NetServiceBrowser.ServiceRemoved(DiscoverRemovedService);
            foreach(var srv in service)
            {
                browser.SearchForService(serviceTypes[srv], DOMAIN);
            }
        }

        private void DiscoverRemovedService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            HandleDiscoverEvent(service, "Disappearing", ServiceDisappeared);
        }

        private void DiscoverFoundService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            service.DidResolveService += new NetService.ServiceResolved(DiscoverResolvedService);
            service.ResolveWithTimeout(TIMEOUT);
        }

        private void DiscoverResolvedService(NetService service)
        {
            HandleDiscoverEvent(service, "Found", ServiceFound);
        }

        private void HandleDiscoverEvent(NetService service, string logText, EventHandler<ServiceEventArgs> eventHandler)
        {
            foreach (var address in service.Addresses)
            {
                IPEndPoint endpoint = (IPEndPoint)address;
                if (NetworkInformation.IsLocalAddress(endpoint.Address) || !NetworkInformation.IsValid(endpoint.Address, Configuration.Services.EnableIPv6))
                {
                    continue;
                }

                if (serviceTypes.ContainsValue(service.Type))
                {
                    WebService webService = serviceTypes.Where(x => x.Value == service.Type).First().Key;
                    Log.Debug("Zeroconf: {0} {1} ({2}) at {3}:{4}", logText, webService, service.Type, endpoint.Address, endpoint.Port);
                    if (eventHandler != null)
                    {
                        eventHandler(this, new ServiceEventArgs()
                        {
                            Service = webService,
                            Endpoint = endpoint
                        });
                    }
                }
            }
        }

        public void StopSearch()
        {
            browser.Stop();
        }
    }
}
