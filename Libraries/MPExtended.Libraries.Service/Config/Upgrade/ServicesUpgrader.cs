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

            model.AccessRequestEnabled = true;
            model.ExternalAddress.Autodetect = true;

            model.BonjourEnabled = file.Element("bonjour").Element("enabled").Value == "true";
            model.BonjourName = file.Element("bonjour").Element("pcname").Value;

            model.Port = Int32.Parse(file.Element("port").Value);
            model.EnableIPv6 = file.Element("enableIPv6").Value == "true";

            // Do not upgrade these, as the password has been encrypted with a different key in MPExtended 0.4.3
            model.NetworkImpersonation = new NetworkImpersonation()
            {
                Domain = String.Empty,
                Username = String.Empty,
                EncryptedPassword = String.Empty,
                ReadInStreamingService = true
            };

            return model;
        }
    }
}
