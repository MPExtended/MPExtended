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
    public class ServiceDescription
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Port { get; set; }
        [DataMember]
        public string User { get; set; }
        [DataMember]
        public string Password { get; set; }
    }

    public class ServerDescription
    {
        [DataMember]
        public int QRVersion { get; set; }
        [DataMember]
        public string Addresses { get; set; }
        [DataMember]
        public string HardwareAddresses { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<ServiceDescription> Services { get; set; }
    }
}
