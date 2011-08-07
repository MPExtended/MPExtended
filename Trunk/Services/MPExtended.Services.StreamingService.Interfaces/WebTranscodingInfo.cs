using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.StreamingService.Interfaces
{
    public class WebTranscodingInfo
    {
        // whether or not getting the transcoding info is supported
        public bool Supported { get; set; }
        // current bitrate in kbit/s
        public decimal CurrentBitrate { get; set; }
        // current time in milliseconds for the transcoding
        public int CurrentTime { get; set; }
        // number of frames that get encoded per second
        public int EncodingFPS { get; set; }
        // number of encoded frames
        public int EncodedFrames { get; set; }
        // is the transcoding finished?
        public bool Finished { get; set; }
        // did the transcoding fail?
        public bool Failed { get; set; }
    }
}