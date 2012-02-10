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
using ZeroconfService;

namespace MPExtended.Services.MetaService
{
    internal class Zeroconf
    {
        public static int TIMEOUT = 10;
        public static string DOMAIN = "";
        public static string SET_SERVICE_TYPE = "_mpextended-set._tcp.";
        public static string TAS_SERVICE_TYPE = "_mpextended-tas._tcp.";
        public static string MAS_SERVICE_TYPE = "_mpextended-mas._tcp.";
        public static string WSS_SERVICE_TYPE = "_mpextended-wss._tcp.";
        public static string USS_SERVICE_TYPE = "_mpextended-uss._tcp.";

        private static bool? isEnabled = null;

        public static bool IsEnabled
        {
            get
            {
                if (isEnabled.HasValue)
                    return isEnabled.Value;

                if (!Configuration.Services.BonjourEnabled)
                {
                    isEnabled = false;
                    return false;
                }

                try
                {
                    Version ver = NetService.DaemonVersion;
                    Log.Debug("Bonjour version {0} installed", ver.ToString());
                    isEnabled = true;
                    return true;
                }
                catch (Exception)
                {
                    Log.Trace("Bonjour not installed");
                    isEnabled = false;
                    return false;
                }
            }
        }
    }
}
