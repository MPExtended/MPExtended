#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using System.Linq;
using System.Text;
using MPExtended.Libraries.General;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class FFMpeg : ITranscoder
    {
        public string Identifier { get; set; }

        public string GetStreamURL()
        {
            return WCFUtil.GetCurrentRoot() + "StreamingService/stream/RetrieveStream?identifier=" + Identifier;
        }

        public void BuildPipeline(StreamContext context)
        {
            // add input
            bool doInputReader = context.Source.NeedsInputReaderUnit;
            if (doInputReader)
            {
                context.Pipeline.AddDataUnit(context.Source.GetInputReaderUnit(), 1);
            }

            // calculate stream mappings (no way I'm going to add subtitle support; it's just broken)
            string mappings = "";
            if (context.AudioTrackId != null)
            {
                mappings = String.Format("-map v:0 -map a:{0}", context.MediaInfo.AudioStreams.First(x => x.ID == context.AudioTrackId).Index);
            }

            // calculate full argument string
            string arguments;
            if (context.Profile.HasVideoStream)
            {
                arguments = String.Format(
                    "-y {0} -i \"#IN#\" -s {1} -aspect {2}:{3} {4} {5} \"#OUT#\"",
                    context.StartPosition != 0 ? "-ss " + (context.StartPosition / 1000) : "",
                    context.OutputSize, context.OutputSize.Width, context.OutputSize.Height,
                    mappings, context.Profile.CodecParameters["codecParameters"]
                );
            }
            else
            {
                arguments = String.Format(
                    "-y {0} -i \"#IN#\" {1} {2} \"#OUT#\"",
                    context.StartPosition != 0 ? "-ss " + (context.StartPosition / 1000) : "",
                    mappings, context.Profile.CodecParameters["codecParameters"]
                );
            }

            // fix input thing
            if (!doInputReader)
                arguments = arguments.Replace("#IN#", context.Source.GetPath());

            // add unit
            EncoderUnit.TransportMethod input = doInputReader ? EncoderUnit.TransportMethod.NamedPipe : EncoderUnit.TransportMethod.Other;
            EncoderUnit unit = new EncoderUnit(Configuration.Streaming.FFMpegPath, arguments, input, EncoderUnit.TransportMethod.NamedPipe, EncoderUnit.LogStream.StandardError);
            unit.DebugOutput = false; // change this for debugging
            context.Pipeline.AddDataUnit(unit, 5);

            // setup output parsing
            var einfo = new Reference<WebTranscodingInfo>(() => context.TranscodingInfo, x => { context.TranscodingInfo = x; });
            FFMpegLogParsingUnit logunit = new FFMpegLogParsingUnit(einfo, context.StartPosition);
            logunit.LogMessages = true;
            logunit.LogProgress = true;
            context.Pipeline.AddLogUnit(logunit, 6);
        }
    }
}
