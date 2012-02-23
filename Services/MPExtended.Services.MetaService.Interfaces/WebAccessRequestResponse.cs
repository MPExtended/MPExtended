using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MetaService.Interfaces
{
    public class WebAccessRequestResponse
    {
        public string Token { get; set; }
        public string ClientName { get; set; }

        public bool UserHasResponded { get; set; }
        public bool IsAllowed { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
    }
}
