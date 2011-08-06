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
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;
using MPExtended.Services.StreamingService.Util;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class FFMpeg : ITranscoder
    {
        public TranscoderProfile Profile { get; set; }
        public string Input { get; set; }

        public FFMpeg(TranscoderProfile profile)
        {
            this.Profile = profile;
        }

        public bool InputReaderWanted()
        {
            return Input.EndsWith(".ts.tsbuffer");
        }

        public EncoderUnit.TransportMethod GetInputMethod()
        {
            if (InputReaderWanted())
                return EncoderUnit.TransportMethod.NamedPipe;
            return EncoderUnit.TransportMethod.Other;
        }

        public EncoderUnit.TransportMethod GetOutputMethod()
        {
            return EncoderUnit.TransportMethod.NamedPipe;
        }

        public string GetTranscoderPath()
        {
            return Config.GetFFMpegPath();
        }

        public string GenerateArguments(WebMediaInfo info, Resolution outputSize, int position, int? audioId, int? subtitleId)
        {
            // no way I'm going to add subtitle support for ffmpeg. It's just broken. 

            // calculate stream mappings
            string mappings = "";
            if(audioId != null)
            {
                mappings = String.Format("-map 0:{0} ", info.VideoStreams.First().Index);
                mappings += String.Format("-map 0:{0} ", info.AudioStreams.Where(x => x.ID == audioId).First().Index);
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
            if (!InputReaderWanted())
                arguments = arguments.Replace("#IN#", Input);

            return arguments;
        }

        public ILogProcessingUnit GetLogParsingUnit(Reference<EncodingInfo> save)
        {
            FFMpegLogParsing unit = new FFMpegLogParsing(save);
            unit.LogMessages = true;
            unit.LogProgress = true;
            return unit;
        }
    }
}
