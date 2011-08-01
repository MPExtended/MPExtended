using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.StreamingService.Interfaces
{
    public class WebTranscodingInfo
    {
        /// <summary>
        /// Bitrate (bandwidth needed) for the currently running transcoding
        /// </summary>
        public decimal CurrentBitrate { get; set; }

        /// <summary>
        /// Current time in ms of the currently running transcoding
        /// </summary>
        public int CurrentTime { get; set; }

        /// <summary>
        /// FPS (Frames per second -> speed) of the currently running transcoding
        /// </summary>
        public int EncodingFPS { get; set; }

        /// <summary>
        /// Frames encoded on the currently running transcoding
        /// </summary>
        public int EncodedFrames { get; set; }
    }
}