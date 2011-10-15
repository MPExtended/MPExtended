using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.StreamingService.Interfaces
{
    public class WebStreamServiceDescription
    {
        public bool SupportsMedia { get; set; }
        public bool SupportsRecordings { get; set; }
        public bool SupportsTV { get; set; }

        public int ApiVersion { get; set; }
        public string ServiceVersion { get; set; }
    }
}