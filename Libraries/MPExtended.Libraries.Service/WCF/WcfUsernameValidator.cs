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
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using MPExtended.Libraries.Service;

namespace MPExtended.Libraries.Service.WCF
{
    public class WcfUsernameValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            if (!Configuration.Services.AuthenticationEnabled)
                return;

            if(!Configuration.Services.Users.Any(x => x.Username == userName && x.ValidatePassword(password)))
            {
                Log.Info("Access denied for request with username {0}", userName);

                SecurityTokenException ex = new SecurityTokenException("Validation Failed!");

                // Doesn't always work: http://blogs.msdn.com/b/drnick/archive/2010/02/02/fix-to-allow-customizing-the-status-code-when-validation-fails.aspx
                ex.Data["HttpStatusCode"] = HttpStatusCode.Unauthorized;

                throw ex;
            }
        }
    }
}
