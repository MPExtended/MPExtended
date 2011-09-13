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
        public WebStreamMediaType SourceType { get; set; }
        public string SourceId { get; set; }
        public string ClientDescription { get; set; }
        public WebTranscodingInfo TranscodingInfo { get; set; }
    }
}
