using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Net;

namespace MPExtended.Services.StreamingService.Util
{
    public class WcfUsernameValidator : UserNamePasswordValidator
    {
        public static String UserName { get; set; }
        public static String Password { get; set; }

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
