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
using System.Runtime.Serialization;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    /// <summary>
    /// Class for holding basic information about this htpc
    /// </summary>
    public class ServerDescription
    {
        [DataMember(Name = "ServiceType")]
        public String ServiceType { get; set; }

        [DataMember(Name = "GeneratorApp")]
        public String GeneratorApp { get; set; }

        [DataMember(Name = "Addresses")]
        public String Addresses { get; set; }

        [DataMember(Name = "Port")]
        public int Port { get; set; }

        [DataMember(Name = "Hostname")]
        public String Hostname { get; set; }

        [DataMember(Name = "HardwareAddresses")]
        public String HardwareAddresses { get; set; }

        [DataMember(Name = "Name")]
        public String Name { get; set; }

        [DataMember(Name = "User")]
        public String User { get; set; }

        [DataMember(Name = "Password")]
        public String Password { get; set; }

        [DataMember(Name = "Passcode")]
        public String Passcode { get; set; }

        [DataMember(Name = "AuthOptions")]
        public int AuthOptions { get; set; }

    }
}
