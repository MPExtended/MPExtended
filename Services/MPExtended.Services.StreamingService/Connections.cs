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
using System.Linq;
using System.Text;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Services.StreamingService
{
    internal static class Connections
    {
        public static ITVAccessService TAS
        {
            get
            {
                return GetServiceSet().TAS;
            }
        }

        public static bool IsTASLocal
        {
            get
            {
                return NetworkInformation.IsLocalAddress(GetServiceSet().Addresses.TAS);
            }
        }

        public static IMediaAccessService MAS
        {
            get
            {
                return GetServiceSet().MAS;
            }
        }

        public static bool IsMASLocal
        {
            get
            {
                return NetworkInformation.IsLocalAddress(GetServiceSet().Addresses.MAS);
            }
        }

        public static IServiceSet Current
        {
            get
            {
                return GetServiceSet();
            }
        }

        private static IServiceSet _serviceSet;

        private static IServiceSet GetServiceSet()
        {
            if (_serviceSet == null)
            {
                var addr = new ServiceAddressSet(Configuration.Services.MASConnection, Configuration.Services.TASConnection);
                _serviceSet = addr.Connect();
            }

            return _serviceSet;
        }
    }
}
