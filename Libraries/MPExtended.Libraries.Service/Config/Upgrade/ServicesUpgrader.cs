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

namespace MPExtended.Libraries.Service.Config.Upgrade
{
    internal class ServicesUpgrader : AttemptConfigUpgrader<Services>
    {
        protected override Services DoUpgrade()
        {
            var file = XElement.Load(OldPath);
            var model = new Services();

            model.AuthenticationEnabled = file.Element("users").Attribute("authenticationEnabled").Value == "true";

            model.BonjourEnabled = file.Element("bonjour").Element("enabled").Value == "true";
            model.BonjourName = file.Element("bonjour").Element("pcname").Value;

            model.Port = Int32.Parse(file.Element("port").Value);
            model.EnableIPv6 = file.Element("enableIPv6").Value == "true";

            model.NetworkImpersonation = new NetworkImpersonation()
            {
                Domain = file.Element("networkImpersonation").Element("domain").Value,
                Username = file.Element("networkImpersonation").Element("username").Value,
                EncryptedPassword = file.Element("networkImpersonation").Element("password").Value,
                ReadInStreamingService = file.Element("networkImpersonation").Element("readInStreamingService").Value == "true"
            };

            model.Users = file.Element("users").Elements("user").Select(x => new User()
            {
                Username = x.Element("username").Value,
                EncryptedPassword = x.Element("password").Value
            }).ToList();

            return model;
        }
    }
}
