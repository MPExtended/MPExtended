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
            try
            {
                if (Current.Addresses.MAS != null)
                {
                    Log.Debug("MAS: Connected to version {0}", Current.MAS.GetServiceDescription().ServiceVersion);
                }
                else
                {
                    Log.Debug("MAS: No service configured");
                }
            }
            catch (Exception ex)
            {
                Log.Debug("MAS: connection failed", ex);
            }

            try
            {
                if (Current.Addresses.TAS != null)
                {
                    Log.Debug("TAS: Connected to version {0}", Current.TAS.GetServiceDescription().ServiceVersion);
                }
                else
                {
                    Log.Debug("TAS: No service configured");
                }
            }
            catch (Exception ex)
            {
                Log.Debug("TAS connection failed", ex);
            }
        }
    }
}