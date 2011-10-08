using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Meta
{
    public class WebBackendProvider
    {
        public string Name { get; set; }
        public string Assembly { get; set; }
        public string Version { get; set; }
    }
}
