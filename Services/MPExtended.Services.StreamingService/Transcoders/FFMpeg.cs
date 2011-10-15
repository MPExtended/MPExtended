#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class FFMpeg : ITranscoder
    {
        public TranscoderProfile Profile { get; set; }
        public MediaSource Source { get; set; }
        public WebMediaInfo MediaInfo { get; set; }
        public string Identifier { get; set; }

        public string GetStreamURL()
        {
            return WCFUtil.GetCurrentRoot() + "StreamingService/stream/RetrieveStream?identifier=" + Identifier;
        }

        public void AlterPipeline(Pipeline pipeline, WebResolution outputSize, Reference<WebTranscodingInfo> einfo, int position, int? audioId, int? subtitleId)
        {
            // add input
            bool doInputReader = !Source.IsLocalFile;
            if (doInputReader)
            {
                pipeline.AddDataUnit(Source.GetInputReaderUnit(), 1);
            }

            // calculate stream mappings (no way I'm going to add subtitle support; it's just broken)
            string mappings = "";
            if (audioId != null)
            {
                mappings = String.Format("-map 0:{0} ", MediaInfo.VideoStreams.First().Index);
                mappings += String.Format("-map 0:{0} ", MediaInfo.AudioStreams.Where(x => x.ID == audioId).First().Index);
            }

            // calculate full argument string
            string arguments;
            if (Profile.HasVideoStream)
            {
                arguments = String.Format(
                    "-y {0} -i \"#IN#\" -s {1} -aspect {2}:{3} {4} {5} \"#OUT#\"",
                    position != 0 ? "-ss " + position : "",
                    outputSize, outputSize.Width, outputSize.Height,
                    mappings, Profile.CodecParameters["codecParameters"]
                );
            }
            else
            {
                arguments = String.Format(
                    "-y {0} -i \"#IN#\" {1} {2} \"#OUT#\"",
                    position != 0 ? "-ss " + position : "",
                    mappings, Profile.CodecParameters["codecParameters"]
                );
            }

            // fix input thing
            if (!doInputReader)
                arguments = arguments.Replace("#IN#", Source.GetPath());

            // add unit
            EncoderUnit.TransportMethod input = doInputReader ? EncoderUnit.TransportMethod.NamedPipe : EncoderUnit.TransportMethod.Other;
            EncoderUnit unit = new EncoderUnit(Config.GetFFMpegPath(), arguments, input, EncoderUnit.TransportMethod.NamedPipe, EncoderUnit.LogStream.StandardError);
            unit.DebugOutput = false; // change this for debugging
            pipeline.AddDataUnit(unit, 5);

            // setup output parsing
            FFMpegLogParsingUnit logunit = new FFMpegLogParsingUnit(einfo);
            logunit.LogMessages = true;
            logunit.LogProgress = true;
            pipeline.AddLogUnit(logunit, 6);
        }
    }
}
