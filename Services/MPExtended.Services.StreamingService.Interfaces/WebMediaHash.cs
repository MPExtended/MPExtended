using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.StreamingService.Interfaces
{
    public class WebMediaHash
    {
        public WebMediaHash(bool generated, string error)
        {
            this.Generated = generated;
            this.Error = error;
        }

        public WebMediaHash()
        {
        }

        public String Error { get; set; }

        public String Hash { get; set; }

        public bool Generated { get; set; }
    }
}
