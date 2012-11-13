using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Units;
using MPExtended.Libraries.Service;
using System.IO;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class FFMpegWrapperHTTPLiveStreaming: FFMpeg, ICustomActionTranscoder
    {
        FFMpegHTTPLiveStreamer httpLive;
        public FFMpegWrapperHTTPLiveStreaming()
            : base()
        {
            ReadOutputStream = false;
        }

        public override string GetStreamURL()
        {
            return httpLive.GetStreamURL();
        }

        public override void BuildPipeline()
        {
            httpLive = new FFMpegHTTPLiveStreamer(Identifier, Context); 
            base.BuildPipeline();
            httpLive.AppendPipeline();
        }

        public override string GenerateArguments()
        {
            string arguments = base.GenerateArguments();
            string outputDirectory = httpLive.TemporaryDirectory;
            string playlist = Path.Combine(outputDirectory, "index.m3u8");
            string segment = Path.Combine(outputDirectory, "%06d.ts");
            return string.Format("{0} -segment_list \"{1}\" \"{2}\"", arguments, playlist, segment);            
        }

        #region ICustomActionTranscoder Members

        public System.IO.Stream CustomActionData(string action, string parameters)
        {
            return httpLive.ProvideCustomActionFile(action, parameters);
        }

        #endregion
    }
}
