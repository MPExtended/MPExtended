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
using System.Xml.Linq;
using MPExtended.Libraries.General;

namespace MPExtended.ServiceHosts.Hosting
{
    internal class ServiceList
    {
        public static List<Service> GetAvailableServices()
        {
            List<Service> allServices = new List<Service>()
            {
                new Service("MediaAccessService", "MPExtended.Services.MediaAccessService", "MediaAccessService", "_mpextended-mas._tcp", Installation.CheckInstalled("MAS")),
                new Service("TVAccessService", "MPExtended.Services.TVAccessService", "TVAccessService", "_mpextended-tas._tcp", Installation.CheckInstalled("TAS")),
                new Service("StreamingService", "MPExtended.Services.StreamingService", "StreamingService", "_mpextended-wss._tcp", Installation.CheckInstalled("WSS")),
                new Service("UserSessionService", "MPExtended.Services.UserSessionService", "UserSessionProxyService", "_mpextended-uss._tcp", Installation.CheckInstalled("USS")),
            };

            string[] disabled =
                XElement.Load(Configuration.GetPath("Services.xml"))
                .Element("disabledServices")
                .Elements("service")
                .Select(x => x.Value)
                .ToArray();

            return allServices.Where(x => x.IsInstalled && !disabled.Contains(x.Assembly)).ToList();
        }
    }
}
