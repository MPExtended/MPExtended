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

// Inspired by the zeroconf code in the WifiRemote plugin, thanks!

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ZeroconfService;
using MPExtended.Libraries.General;
using System.Security.Cryptography;

namespace MPExtended.ServiceHosts.Hosting
{
    internal class Zeroconf
    {
        private const string DOMAIN = ""; // whole network

        private List<NetService> publishServices = new List<NetService>();
        private string serviceName = "";

        public void PublishServices(List<Service> services)
        {
            if (!Configuration.Services.BonjourEnabled || !CheckBonjourInstallation())
            {
                return;
            }

            serviceName = GetServiceName();
            foreach (Service srv in services)
            {
                // We also send a list of usernames and password hashes, so that clients can detect if they match across MPExtended
                // installations.
                // Note: this is specifically introduced for aMPdroid. It will probably be changed to something more advanced in the
                // next release where we don't keep backwards-compatibility for this part. Please do not depend on the presence of
                // this property.
                HashAlgorithm hashAlg = MD5.Create();
                Dictionary<string, string> sendUsers = new Dictionary<string,string>();
                foreach(var user in Configuration.Services.Users)
                {
                    byte[] hash = hashAlg.ComputeHash(Encoding.UTF8.GetBytes(user.EncryptedPassword));
                    for (int i = 1; i < 1000; i++)
                    {
                        hash = hashAlg.ComputeHash(hash);
                    }
                    sendUsers.Add(user.Username, Convert.ToBase64String(hash, 0, 12));
                }

                // also publish IP address and username list
                Dictionary<string, string> additionalData = new Dictionary<string, string>();
                additionalData["hwAddr"] = String.Join(";", NetworkInformation.GetMACAddresses());
                additionalData["users"] = String.Join(";", sendUsers.Select(x => x.Key + ":" + x.Value));

                NetService net = new NetService(DOMAIN, srv.ZeroconfServiceType, serviceName, srv.Port);
                net.AllowMultithreadedCallbacks = true;
                net.TXTRecordData = NetService.DataFromTXTRecordDictionary(additionalData);
                net.DidNotPublishService += new NetService.ServiceNotPublished(FailedToPublishService);
                net.DidPublishService += new NetService.ServicePublished(PublishedService);
                net.Publish();
            }
            Log.Info("Published bonjour services");
        }

        private bool CheckBonjourInstallation()
        {
            try
            {
                Version ver = NetService.DaemonVersion;
                Log.Trace("Bonjour version {0} installed", ver.ToString());
                return true;
            }
            catch (Exception)
            {
                Log.Trace("Bonjour not installed");
                return false;
            }
        }

        private string GetServiceName()
        {
            string value = Configuration.Services.BonjourName;
            if (!String.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            try
            {
                return System.Environment.MachineName;
            }
            catch (Exception)
            {
                return "MPExtended Services";
            }
        }

        private void FailedToPublishService(NetService service, DNSServiceException error)
        {
            string msg = String.Format("Failed to publish service {0}", service.Name);
            Log.Warn(msg, error);
        }

        private void PublishedService(NetService service)
        {
            Log.Debug("Published service {0}", service.Type);
        }
    }
}
