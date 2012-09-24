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
using System.Web;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public static class Connections
    {
        private static IServiceSet serviceSet = null;

        public static IServiceSet Current
        {
            get
            {
                return serviceSet;
            }
        }

        internal static void SetUrls(string mas, string tas)
        {
            var addressSet = new ServiceAddressSet(mas, tas, null);
            serviceSet = addressSet.Connect(Configuration.WebMediaPortal.ServiceUsername, Configuration.WebMediaPortal.ServicePassword);
        }

        internal static void LogServiceVersions()
        {
            string masVersion = Current.HasMASConnection ? Current.MAS.GetServiceDescription().ServiceVersion : "<unconnected>";
            string tasVersion = Current.HasTASConnection ? Current.TAS.GetServiceDescription().ServiceVersion : "<unconnected>";
            Log.Debug("Connected to MAS version {0}, TAS version {1}", masVersion, tasVersion);
        }
    }
}