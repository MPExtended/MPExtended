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
using System.Xml.Linq;

namespace MPExtended.Libraries.Service.ConfigurationContracts
{
    public class WebMediaPortalHosting
    {
        public int Port { get; set; }

        public bool EnableTLS { get; set; }
        public int PortTLS { get; set; }

        public WebMediaPortalHosting(string path, string defaultPath)
        {
            // default settings
            Port = 8080;
            EnableTLS = false;
            PortTLS = 44300;

            // load available settings from file (only port can be required as it was present in the first release)
            try
            {
                XElement file = XElement.Load(path);

                Port = Int32.Parse(file.Element("port").Value);
                if (file.Element("tls") != null)
                {
                    EnableTLS = file.Element("tls").Attribute("enable").Value == "yes";
                    PortTLS = Int32.Parse(file.Element("tls").Value);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to open WebMediaPortalHosting.xml", ex);
            }

            // validate port tls 
            if (EnableTLS && (PortTLS < 44300 || PortTLS > 44399))
            {
                Log.Error("TLS port {0} not valid, only 44300-44399 allowed by IIS Express, using port 44300", PortTLS);
                PortTLS = 44300;
            }
        }

        public bool Save()
        {
            try
            {
                XElement file = XElement.Load(Configuration.GetPath("WebMediaPortalHosting.xml"));

                file.Element("port").Value = Port.ToString();

                // remove already existing <tls> nodes, if it is there
                file.Elements("tls").Remove();

                // add new <tls> node
                file.Add(new XElement("tls",
                    new XAttribute("enable", EnableTLS ? "yes" : "no"),
                    PortTLS)
                );

                file.Save(Configuration.GetPath("WebMediaPortalHosting.xml"));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to write WebMediaPortalHosting.xml", ex);
                return false;
            }
        }
    }
}
