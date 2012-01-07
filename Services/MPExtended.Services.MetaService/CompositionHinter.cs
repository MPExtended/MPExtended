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
using System.Net;
using System.Text;
using MPExtended.Libraries.General;

namespace MPExtended.Services.MetaService
{
    internal class CompositionHinter
    {
        private string tveAddress;

        public void StartDiscovery()
        {
            tveAddress = ReadConfiguredTVServerAddress();
        }

        public string GetConfiguredTVServerAddress()
        {
            return tveAddress;
        }

        protected string ReadConfiguredTVServerAddress()
        {
            // Try to read the TV server IP address from MediaPortal's configuration
            if (!Mediaportal.HasValidConfigFile())
            {
                return null;
            }

            // read the hostname
            Dictionary<string, string> tvSection = Mediaportal.ReadSectionFromConfigFile("tvservice");
            if (tvSection == null || !tvSection.ContainsKey("hostname") || string.IsNullOrWhiteSpace(tvSection["hostname"]))
            {
                return null;
            }
            string hostname = tvSection["hostname"];

            // Return as IP addresses
            return Dns.GetHostAddresses(hostname).Select(x => x.ToString()).First();
        }
    }
}
