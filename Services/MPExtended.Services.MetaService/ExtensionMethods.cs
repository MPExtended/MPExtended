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
using MPExtended.Services.MetaService.Interfaces;

namespace MPExtended.Services.MetaService
{
    internal static class ServiceConfigurationExtensionMethods
    {
        private static Dictionary<string, WebService> knownServices = new Dictionary<string, WebService>()
        {
            { "MediaAccessService", WebService.MediaAccessService },
            { "StreamingService", WebService.StreamingService },
            { "TVAccessService", WebService.TVAccessService },
            { "UserSessionService", WebService.UserSessionService },
            { "MetaService", WebService.MetaService },
            { "WifiRemote", WebService.WifiRemote }
        };

        public static bool IsKnownService(this ServiceConfiguration service)
        {
            return knownServices.ContainsKey(service.Service);
        }

        public static WebService ToWebService(this ServiceConfiguration service)
        {
            if (knownServices.ContainsKey(service.Service))
                return knownServices[service.Service];
            throw new ArgumentException();
        }
    }
}
