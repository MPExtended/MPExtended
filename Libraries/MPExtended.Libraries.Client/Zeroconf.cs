#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using ZeroconfService;
using System.Net;

namespace MPExtended.Libraries.Client
{
    internal class Zeroconf
    {
        public delegate void EntryFoundCallback(string address, int port, string metaService);

        private static int TIMEOUT = 10;
        private static string DOMAIN = "";
        private static string SET_SERVICE_TYPE = "_mpextended-set._tcp.";

        private EntryFoundCallback foundCallback;
        private NetServiceBrowser browser;

        public static bool CheckAvailability()
        {
            try
            {
                Version ver = NetService.DaemonVersion;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void StartDiscovery(EntryFoundCallback callback)
        {
            foundCallback = callback;

            browser = new NetServiceBrowser();
            browser.AllowMultithreadedCallbacks = true;
            browser.DidFindService += new NetServiceBrowser.ServiceFound(ZeroconfDiscoverFoundService);
            browser.SearchForService(SET_SERVICE_TYPE, DOMAIN);
        }

        public void StopDiscovery()
        {
            if (browser != null)
            {
                browser.Stop();
            }
        }

        private void ZeroconfDiscoverFoundService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            service.DidResolveService += new NetService.ServiceResolved(ZeroconfDiscoverResolvedService);
            service.ResolveWithTimeout(Zeroconf.TIMEOUT);
        }

        private void ZeroconfDiscoverResolvedService(NetService service)
        {
            var data = NetService.DictionaryFromTXTRecordData(service.TXTRecordData);

            foreach (var address in service.Addresses)
            {
                IPEndPoint endpoint = (IPEndPoint)address;
                if (service.Type == SET_SERVICE_TYPE)
                {
                    var meta = data.Contains("meta") ? Encoding.ASCII.GetString((byte[])data["meta"]) : 
                                                       String.Format("http://{0}:{1}/", endpoint.Address, endpoint.Port);
                    foundCallback.Invoke(endpoint.Address.ToString(), endpoint.Port, meta);
                }
            }
        }
    }
}
