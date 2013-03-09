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
using System.Net;
using System.Web;
using MPExtended.Libraries.Client;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class FoundServicesModel
    {
        public string MAS { get; set; }
        public string MASHost { get; set; }
        public string MASStream { get; set; }

        public string TAS { get; set; }
        public string TASHost { get; set; }
        public string TASStream { get; set; }

        public FoundServicesModel(IServiceAddressSet sourceSet)
        {
            MAS = sourceSet.MAS;
            MASStream = sourceSet.MASStream;
            if (MAS != null)
                MASHost = GetHostName(MAS);

            TAS = sourceSet.TAS;
            TASStream = sourceSet.TASStream;
            if (TAS != null)
                TASHost = GetHostName(TAS);
        }

        private string GetHostName(string address)
        {
            if (address.Contains(':'))
                address = address.Substring(0, address.IndexOf(':'));

            var entry = Dns.GetHostEntry(address);
            if (entry != null && entry.HostName != null)
                return entry.HostName;
            if (entry != null && entry.Aliases.Count() > 0)
                return entry.Aliases.First();

            return address;
        }
    }
}