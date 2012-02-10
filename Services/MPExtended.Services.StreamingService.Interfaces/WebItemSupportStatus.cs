using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.StreamingService.Interfaces
{
    public class WebItemSupportStatus
    {
        public bool Supported { get; set; }
        public string Reason { get; set; }

        public WebItemSupportStatus()
        {
        }

        public WebItemSupportStatus(bool supported, string reason)
        {
            Supported = supported;
            Reason = reason;
        }
    }
}
