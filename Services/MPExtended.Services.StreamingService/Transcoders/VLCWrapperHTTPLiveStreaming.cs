#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Services.StreamingService.Code;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class VLCWrapperHTTPLiveStreaming : VLCWrapper, ICustomActionTranscoder
    {
        private VLCHTTPLiveStreamer httpLiveStreamer;

        public VLCWrapperHTTPLiveStreaming()
            : base ()
        {
            ReadOutputStream = false;
        }

        public override string GetStreamURL()
        {
            return httpLiveStreamer.GetStreamURL();
        }

        public override void BuildPipeline()
        {
            httpLiveStreamer = new VLCHTTPLiveStreamer(Identifier, Context);
            base.BuildPipeline();
            httpLiveStreamer.AppendPipeline();
        }

        protected override VLCParameters GenerateVLCParameters(string options, string tsOptions, bool disableSeeking, string encoderOptions, string muxerOptions)
        {
            return base.GenerateVLCParameters(options, tsOptions, disableSeeking, encoderOptions, httpLiveStreamer.GetFullMuxerString());
        }

        public Stream CustomActionData(string action, string parameters)
        {
            return httpLiveStreamer.ProvideCustomActionFile(action, parameters);
        }
    }
}
