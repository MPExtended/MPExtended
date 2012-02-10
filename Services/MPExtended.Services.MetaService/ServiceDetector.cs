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
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.MetaService.Interfaces;

namespace MPExtended.Services.MetaService
{
    internal class ServiceDetector
    {
        public bool HasActiveMAS
        {
            get
            {
                try
                {
                    if (!Installation.IsServiceInstalled(MPExtendedService.MediaAccessService))
                        return false;
                    var msd = ServiceClientFactory.CreateLocalMAS().GetServiceDescription();
                    return msd.AvailableMovieLibraries.Count > 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool HasActiveTAS
        {
            get
            {
                try
                {
                    if (!Installation.IsServiceInstalled(MPExtendedService.TVAccessService))
                        return false;
                    var tsd = ServiceClientFactory.CreateLocalTAS().GetServiceDescription();
                    return tsd.HasConnectionToTVServer;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool HasActiveWSS
        {
            get
            {
                try
                {
                    if (!Installation.IsServiceInstalled(MPExtendedService.StreamingService))
                        return false;
                    var wsd = ServiceClientFactory.CreateLocalWSS().GetServiceDescription();
                    return wsd.SupportsMedia || wsd.SupportsRecordings || wsd.SupportsTV;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool HasUI
        {
            get
            {
                return Installation.IsServiceInstalled(MPExtendedService.UserSessionService);
            }
        }

        private CompositionHinter hinter;

        public ServiceDetector()
        {
            hinter = new CompositionHinter();
            hinter.StartDiscovery();
        }

        public ServiceSetComposer CreateSetComposer()
        {
            ServiceSetComposer composer = new ServiceSetComposer(hinter);
            composer.OurAddress = NetworkInformation.GetIPAddresses().First();

            composer.HasActiveMAS = HasActiveMAS;
            composer.HasActiveTAS = HasActiveTAS;
            composer.HasActiveWSS = HasActiveWSS;
            composer.HasActiveUI = HasUI;

            return composer;
        }

        public List<WebService> GetInstalledServices()
        {
            return Installation.GetInstalledServices().Select(x => x.ToWebService()).ToList();
        }

        public List<WebService> GetActiveServices()
        {
            List<WebService> list = new List<WebService>()
            {
                WebService.MediaAccessService
            };

            if (HasActiveMAS) list.Add(WebService.MediaAccessService);
            if (HasActiveTAS) list.Add(WebService.TVAccessService);
            if (HasActiveWSS) list.Add(WebService.StreamingService);
            if (HasUI) list.Add(WebService.UserSessionService);
            return list;
        }
    }
}
