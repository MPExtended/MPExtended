using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MetaService.Interfaces
{
    public class WebServiceVersion
    {
        public string Version { get; set; }
        public string Build { get; set; }
        public int ApiVersion { get; set; }
    }
}
