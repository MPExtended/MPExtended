#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Hosting;
using MPExtended.Libraries.Service.Shared;
using MPExtended.Libraries.Service.Network;
using MPExtended.Services.MetaService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.TVAccessService
{
    public class LogoDownloader
    {
        private static Timer backgroundTimer;
        private static LogoDownloader logoDownloader;

        public static void Setup()
        {
            // start the thread when the service has been start up
            ServiceState.Started += delegate()
            {
                logoDownloader = new LogoDownloader();
                Task.Factory.StartNew(logoDownloader.Init);
            };
        }

        private ChannelLogos logos;
        private List<string> alreadyHandledClients = new List<string>();
        private List<WebChannelBasic> channelLogosRequired = new List<WebChannelBasic>();

        private void Init()
        {
            // don't do anything if we don't have a TVE connection
            if (!TVAccessService.Instance.TestConnectionToTVService())
                return;

            // exit if we already got all logos
            logos = new ChannelLogos();
            ScanForRequiredLogos();
            if (channelLogosRequired.Count == 0)
            {
                Log.Trace("All channel logos already available, not downloading any...");
                return;
            }

            // try downloading them on starting and exit if successful
            if (PerformCheck())
            {
                return;
            }

            // setup timer
            backgroundTimer = new Timer()
            {
                AutoReset = true,
                Interval = 60 * 60 * 1000,
            };
            backgroundTimer.Elapsed += new ElapsedEventHandler(TimerElapsed);
            backgroundTimer.Start();
        }

        private void ScanForRequiredLogos()
        {
            ITVAccessService tas = TVAccessService.Instance;
            if (!tas.TestConnectionToTVService())
                return;
            channelLogosRequired = tas.GetChannelsBasic()
                .Where(ch => logos.FindLocation(ch.Title) == null)
                .ToList();
        }

        private void TimerElapsed(object source, ElapsedEventArgs args)
        {
            try
            {
                if (PerformCheck())
                {
                    backgroundTimer.Stop();
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to check for channel logos", ex);
            }
        }

        private bool PerformCheck()
        {
            IServiceDiscoverer discoverer = new ServiceDiscoverer();
            foreach (IServiceAddressSet set in discoverer.DiscoverSets(TimeSpan.FromSeconds(15)))
            {
                if (set.MASStream == null)
                {
                    Log.Trace("Found service set {0} without MAS stream, ignoring...");
                    continue;
                }

                Log.Trace("Found service set {0} with MAS streams at {1}", set.GetSetIdentifier(), set.MASStream);
                string ipAddress = set.MASStream.Substring(0, set.MASStream.IndexOf(':'));
                if (!alreadyHandledClients.Contains(set.MASStream) && !NetworkInformation.IsLocalAddress(ipAddress))
                {
                    Log.Debug("Going to download channel logos from MAS installation at {0}", set.MASStream);
                    if(!DownloadChannelLogos(logos, set.Connect()))
                    {
                        Log.Debug("Failed to download them without authorization, trying all our local accounts");
                        foreach (var user in Configuration.Authentication.Users)
                        {
                            if (DownloadChannelLogos(logos, set.Connect(user.Username, user.GetPassword())))
                            {
                                Log.Debug("Downloaded channel logos with account {0}", user.Username);
                                break;
                            }
                        }
                    }
                    alreadyHandledClients.Add(set.MASStream);
                }
            }

            // exit if we got all the logos
            ScanForRequiredLogos();
            if (channelLogosRequired.Count == 0)
            {
                Log.Trace("Yes, got all channel logos now!");
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool DownloadChannelLogos(ChannelLogos logos, IServiceSet serviceSet)
        {
            try
            {
                foreach (WebChannelBasic ch in channelLogosRequired)
                {
                    try
                    {
                        Stream logoStream = serviceSet.MASStream.GetImage(WebMediaType.TV, null, ch.Id.ToString());
                        logos.WriteToCacheDirectory(ch.Title, "png", logoStream);
                        Log.Debug("Downloaded logo for channel {0}", ch.Title);
                    }
                    catch (EndpointNotFoundException)
                    {
                        Log.Trace("Logo for channel {0} not available on server", ch.Title);
                    }
                }

                return true;
            }
            catch (MessageSecurityException)
            {
                return false;
            }
        }
    }
}
