using System;
using System.Collections.Generic;
using System.Linq;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Util;

namespace MPExtended.Services.StreamingService.Code
{
    internal static class EncodingInfoExtensionMethods
    {
        public static WebTranscodingInfo ToWebTranscodingInfo(this EncodingInfo info)
        {
            return new WebTranscodingInfo() {
                CurrentBitrate = info.CurrentBitrate,
                CurrentTime = info.CurrentTime,
                EncodedFrames = info.EncodedFrames,
                EncodingFPS = info.EncodingFPS,
            };
        }
    }

    internal static class TranscoderProfileExtensionMethods
    {
        public static WebTranscoderProfile ToWebTranscoderProfile(this TranscoderProfile profile)
        {
            return new WebTranscoderProfile()
            {
                Name = profile.Name,
                Description = profile.Description,
                MIME = profile.MIME,
                MaxOutputWidth = profile.MaxOutputWidth,
                MaxOutputHeight = profile.MaxOutputHeight,
                UseTranscoding = profile.UseTranscoding,
                Target = profile.Target,
                Bandwidth = profile.Bandwidth
            };
        }
    }

    internal static class ResolutionExtensionMethods
    {
        public static WebResolution ToWebResolution(this Resolution res)
        {
            return new WebResolution()
            {
                Width = res.Width,
                Height = res.Height
            };
        }
    }
}
