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
        public string DisplayName { get; set; }

        public string ClientDescription { get; set; }
        public string ClientIPAddress { get; set; }
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The place where the user started the playback. 
        /// </summary>
        public int StartPosition { get; set; }

        /// <summary>
        /// The current place of the player.
        /// </summary>
        public int PlayerPosition { get; set; }

        public WebTranscodingInfo TranscodingInfo { get; set; }
    }
}
