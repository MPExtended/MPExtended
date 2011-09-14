using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebTVServiceDescription
    {
        public int ApiVersion { get; set; }
        public string ServiceVersion { get; set; }

        public bool HasConnectionToTVServer { get; set; }
    }
}
