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
using MPExtended.Libraries.General;

namespace MPExtended.Services.MetaService
{
    internal class ServiceDetector
    {
        public static bool HasActiveMAS
        {
            get
            {
                try
                {
                    var msd = ServiceClientFactory.CreateLocalMAS().GetServiceDescription();
                    return msd.AvailableMovieLibraries.Count > 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool HasActiveTAS
        {
            get
            {
                try
                {
                    var tsd = ServiceClientFactory.CreateLocalTAS().GetServiceDescription();
                    return tsd.HasConnectionToTVServer;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool HasActiveWSS
        {
            get
            {
                try
                {
                    var wsd = ServiceClientFactory.CreateLocalWSS().GetServiceDescription();
                    return wsd.SupportsMedia || wsd.SupportsRecordings || wsd.SupportsTV;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool HasUI
        {
            get
            {
                return true;
            }
        }

        public static ServiceSetComposer GetSetComposer()
        {
            ServiceSetComposer composer = new ServiceSetComposer();
            composer.OurAddress = NetworkInformation.GetIPAddresses().First();

            composer.HasActiveMAS = HasActiveMAS;
            composer.HasActiveTAS = HasActiveTAS;
            composer.HasActiveWSS = HasActiveWSS;
            composer.HasActiveUI = HasUI;

            return composer;
        }
    }
}
