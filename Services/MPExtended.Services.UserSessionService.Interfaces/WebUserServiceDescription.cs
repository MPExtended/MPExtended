using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.UserSessionService.Interfaces
{
    public class WebUserServiceDescription
    {
        public int ApiVersion { get; set; }
        public string ServiceVersion { get; set; }

        public bool IsAvailable { get; set; }
    }
}
