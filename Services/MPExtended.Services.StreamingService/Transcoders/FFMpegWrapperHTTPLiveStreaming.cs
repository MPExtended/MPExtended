#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class FFMpegWrapperHTTPLiveStreaming: FFMpeg, ICustomActionTranscoder
    {
        protected FFMpegHTTPLiveStreamer httpLive;

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

        public Stream CustomActionData(string action, string parameters)
        {
            return httpLive.ProvideCustomActionFile(action, parameters);
        }
    }
}
