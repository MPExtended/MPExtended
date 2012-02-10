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
using System.Net.Sockets;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using ZeroconfService;

namespace MPExtended.Services.MetaService
{
    internal class CompositionHinter : ICompositionHinter
    {
        private string tveAddress;
        private List<IPEndPoint> tvServerAddresses;

        public void StartDiscovery()
        {
            tveAddress = ReadConfiguredTVServerAddress();
            tvServerAddresses = new List<IPEndPoint>();

            if (Zeroconf.IsEnabled)
            {
                NetServiceBrowser browser = new NetServiceBrowser();
                browser.AllowMultithreadedCallbacks = true;
                browser.DidFindService += new NetServiceBrowser.ServiceFound(ZeroconfDiscoverFoundService);
                browser.DidRemoveService += new NetServiceBrowser.ServiceRemoved(ZeroconfDiscoverRemovedService);
                browser.SearchForService(Zeroconf.TAS_SERVICE_TYPE, Zeroconf.DOMAIN);
            }
        }

        public string GetConfiguredTVServerAddress()
        {
            return tveAddress;
        }

        public IList<IPEndPoint> GetTVServersInLocalNetwork()
        {
            return tvServerAddresses;
        }

        protected string ReadConfiguredTVServerAddress()
        {
            // Try to read the TV server IP address from MediaPortal's configuration
            if (!Mediaportal.HasValidConfigFile())
            {
                return null;
            }

            // read the hostname
            Dictionary<string, string> tvSection = Mediaportal.ReadSectionFromConfigFile("tvservice");
            if (tvSection == null || !tvSection.ContainsKey("hostname") || string.IsNullOrWhiteSpace(tvSection["hostname"]))
            {
                return null;
            }
            string hostname = tvSection["hostname"];

            // Return as IP addresses
            return Dns.GetHostAddresses(hostname).Select(x => x.ToString()).First();
        }

        private void ZeroconfDiscoverRemovedService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            foreach (var address in service.Addresses)
            {
                IPEndPoint endpoint = (IPEndPoint)address;
                if (service.Type == Zeroconf.TAS_SERVICE_TYPE && tvServerAddresses.Contains(endpoint))
                {
                    Log.Trace("TAS at {0}:{1} disappeared", endpoint.Address, endpoint.Port);
                    lock (tvServerAddresses)
                    {
                        tvServerAddresses.Remove(endpoint);
                    }
                }
            }
        }

        private void ZeroconfDiscoverFoundService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            service.DidResolveService += new NetService.ServiceResolved(ZeroconfDiscoverResolvedService);
            service.ResolveWithTimeout(Zeroconf.TIMEOUT);
        }

        private void ZeroconfDiscoverResolvedService(NetService service)
        {

            foreach (var address in service.Addresses)
            {
                IPEndPoint endpoint = (IPEndPoint)address;
                if (NetworkInformation.IsLocalAddress(endpoint.Address) || !NetworkInformation.IsValid(endpoint.Address, Configuration.Services.EnableIPv6))
                    continue;

                if (service.Type == Zeroconf.TAS_SERVICE_TYPE)
                {
                    Log.Trace("Discovered TAS at {0}:{1}", endpoint.Address, endpoint.Port);
                    lock (tvServerAddresses)
                    {
                        tvServerAddresses.Add((IPEndPoint)address);
                    }
                }
            }
        }
    }
}
