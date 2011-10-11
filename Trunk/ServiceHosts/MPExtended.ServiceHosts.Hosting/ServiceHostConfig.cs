#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
    internal class ServiceHostConfig
    {
        public static bool BonjourEnabled
        {
            get 
            {
                XElement file = XElement.Load(Configuration.GetPath("Services.xml"));
                return file.Element("bonjour") != null && 
                    file.Element("bonjour").Element("enabled") != null && 
                    file.Element("bonjour").Element("enabled").Value == "true";
            }
        }

        public static string BonjourServiceName
        {
            get
            {
                return XElement.Load(Configuration.GetPath("Services.xml")).Element("bonjour").Element("pcname").Value;
            }
        }

        public static int Port
        {
            get
            {
                XElement file = XElement.Load(Configuration.GetPath("Services.xml"));
                int port;
                if (file.Element("port") != null && Int32.TryParse(file.Element("port").Value, out port))
                {
                    return port;
                }

                return 4322;
            }
        }

        public static bool IPv6Enabled
        {
            get
            {
                XElement file = XElement.Load(Configuration.GetPath("Services.xml"));
                return file.Elements("enableIPv6") != null && file.Element("enableIPv6").Value == "true";
            }
        }
    }
}
