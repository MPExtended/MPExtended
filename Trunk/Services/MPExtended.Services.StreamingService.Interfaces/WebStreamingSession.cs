using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.StreamingService.Interfaces
{
    public class WebStreamingSession
    {
        public string Profile { get; set; }
        public string Identifier { get; set; }
        public string SourceFile { get; set; }
        public string ClientDescription { get; set; }
        public WebTranscodingInfo TranscodingInfo { get; set; }
    }
}
