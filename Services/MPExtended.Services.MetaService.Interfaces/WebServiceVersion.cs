using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MetaService.Interfaces
{
    public class WebServiceVersion
    {
        public string Version { get; set; }
        public int ApiVersion { get; set; }

        public int? TASApiVersion { get; set; }
        public int? MASApiVersion { get; set; }
        public int? StreamApiVersion { get; set; }
        public int? USSApiVersion { get; set; }
    }
}
