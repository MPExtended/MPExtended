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
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Text;
using MPExtended.Libraries.General;

namespace MPExtended.Libraries.ServiceLib
{
    public class WcfUsernameValidator : UserNamePasswordValidator
    {
        public static String UserName { get; set; }
        public static String Password { get; set; }

        public static void Init()
        {
            string username;
            string password;
            Configuration.GetCredentials(out username, out password, false);
            UserName = username;
            Password = password;
        }

        public override void Validate(string userName, string password)
        {
            // This isn't secure, though
            if ((WcfUsernameValidator.UserName != null && WcfUsernameValidator.Password != null) &&
                (userName != WcfUsernameValidator.UserName || password != WcfUsernameValidator.Password))
            {
                SecurityTokenException ex = new SecurityTokenException("Validation Failed!");
                //needs ms fix -> http://blogs.msdn.com/b/drnick/archive/2010/02/02/fix-to-allow-customizing-the-status-code-when-validation-fails.aspx
                ex.Data["HttpStatusCode"] = HttpStatusCode.Unauthorized;
                throw ex;
            }
        }
    }
}
