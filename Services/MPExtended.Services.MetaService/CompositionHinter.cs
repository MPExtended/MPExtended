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
using MPExtended.Services.MetaService.Interfaces;

namespace MPExtended.Services.MetaService
{
    internal class CompositionHinter
    {
        private IPEndPoint tveAddress;
        private List<IPEndPoint> tvServerAddresses;
        private List<INetworkDiscoverer> validDiscoverers;

        public CompositionHinter(params INetworkDiscoverer[] discoverers)
        {
            validDiscoverers = discoverers.Where(x => x.IsAvailable()).ToList();
        }

        public void StartDiscovery()
        {
            tveAddress = ReadConfiguredTVServerAddress();
            tvServerAddresses = new List<IPEndPoint>();

            foreach (var discoverer in validDiscoverers)
            {
                discoverer.ServiceFound += new EventHandler<ServiceEventArgs>(FoundService);
                discoverer.ServiceDisappeared += new EventHandler<ServiceEventArgs>(RemovedService);
                discoverer.StartServiceSearch(WebService.TVAccessService);
            }
        }

        public IPEndPoint GetConfiguredTVServerAddress()
        {
            return tveAddress;
        }

        public IList<IPEndPoint> GetTVServersInLocalNetwork()
        {
            return tvServerAddresses;
        }

        public IEnumerable<IServicePublisher> GetAvailablePublishers()
        {
            return validDiscoverers
                .Select(x => x.GetPublisher())
                .Where(x => x != null);
        }

        protected IPEndPoint ReadConfiguredTVServerAddress()
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

            try
            {
                // Return as IP addresses
                var address = Dns.GetHostAddresses(hostname).First();
                return new IPEndPoint(address, Configuration.DEFAULT_PORT);
            }
            catch (SocketException)
            {
                Log.Info("Failed to resolve hostname {0} configured as default TV Server", hostname);
                return null;
            }
        }

        private void RemovedService(object sender, ServiceEventArgs e)
        {
            Log.Trace("TAS at {0} disappeared", e.Endpoint);
            lock (tvServerAddresses)
            {
                tvServerAddresses.Remove(e.Endpoint);
            }
        }

        private void FoundService(object sender, ServiceEventArgs e)
        {
            Log.Trace("Discovered TAS at {0}", e.Endpoint);
            lock (tvServerAddresses)
            {
                tvServerAddresses.Add(e.Endpoint);
            }
        }
    }
}
