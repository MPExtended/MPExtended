using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.StreamingService.Interfaces
{
    public class WebMediaHash
    {
        public bool Generated { get; set; }
        public string Error { get; set; }
        public string Hash { get; set; }
    }
}
