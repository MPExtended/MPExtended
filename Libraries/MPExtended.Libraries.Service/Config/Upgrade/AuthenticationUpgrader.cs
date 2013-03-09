#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
    internal class AuthenticationUpgrader : AttemptConfigUpgrader<Authentication>
    {
        protected override Authentication DoUpgrade()
        {
            var file = XElement.Load(OldPath);
            var model = new Authentication();

            model.Enabled = file.Element("users").Attribute("authenticationEnabled").Value == "true";

            model.Users = file.Element("users").Elements("user").Select(x => new User()
            {
                Username = x.Element("username").Value,
                EncryptedPassword = x.Element("password").Value
            }).ToList();

            return model;
        }
    }
}
