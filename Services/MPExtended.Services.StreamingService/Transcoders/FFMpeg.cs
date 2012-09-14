#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class FFMpeg : ITranscoder
    {
        public string Identifier { get; set; }
        public StreamContext Context { get; set; }

        public string GetStreamURL()
        {
            return WCFUtil.GetCurrentRoot() + "StreamingService/stream/RetrieveStream?identifier=" + Identifier;
        }

        public void BuildPipeline()
        {
            // add input
            bool doInputReader = Context.Source.NeedsInputReaderUnit;
            if (doInputReader)
            {
                Context.Pipeline.AddDataUnit(Context.Source.GetInputReaderUnit(), 1);
            }

            // calculate stream mappings (no way I'm going to add subtitle support; it's just broken)
            string mappings = "";
            if (Context.AudioTrackId != null)
            {
                mappings = String.Format("-map v:0 -map a:{0}", Context.MediaInfo.AudioStreams.First(x => x.ID == Context.AudioTrackId).Index);
            }

            // calculate full argument string
            string arguments;
            bool doResize = !Context.Profile.TranscoderParameters.ContainsKey("noResize") || Context.Profile.TranscoderParameters["noResize"] != "true";
            if (Context.Profile.HasVideoStream && doResize)
            {
                arguments = String.Format(
                    "-y {0} -i \"#IN#\" -s {1} -aspect {2}:{3} {4} {5} \"#OUT#\"",
                    Context.StartPosition != 0 ? "-ss " + (Context.StartPosition / 1000) : "",
                    Context.OutputSize, Context.OutputSize.Width, Context.OutputSize.Height,
                    mappings, Context.Profile.TranscoderParameters["codecParameters"]
                );
            }
            else
            {
                arguments = String.Format(
                    "-y {0} -i \"#IN#\" {1} {2} \"#OUT#\"",
                    Context.StartPosition != 0 ? "-ss " + (Context.StartPosition / 1000) : "",
                    mappings, Context.Profile.TranscoderParameters["codecParameters"]
                );
            }

            // fix input thing
            if (!doInputReader)
                arguments = arguments.Replace("#IN#", Context.Source.GetPath());

            // add unit
            EncoderUnit.TransportMethod input = doInputReader ? EncoderUnit.TransportMethod.NamedPipe : EncoderUnit.TransportMethod.Other;
            EncoderUnit unit = new EncoderUnit(Configuration.StreamingProfiles.FFMpegPath, arguments, input, EncoderUnit.TransportMethod.NamedPipe, EncoderUnit.LogStream.StandardError, Context);
            unit.DebugOutput = false; // change this for debugging
            Context.Pipeline.AddDataUnit(unit, 5);

            // setup output parsing
            var einfo = new Reference<WebTranscodingInfo>(() => Context.TranscodingInfo, x => { Context.TranscodingInfo = x; });
            FFMpegLogParsingUnit logunit = new FFMpegLogParsingUnit(einfo, Context.StartPosition);
            logunit.LogMessages = true;
            logunit.LogProgress = true;
            Context.Pipeline.AddLogUnit(logunit, 6);
        }
    }
}
