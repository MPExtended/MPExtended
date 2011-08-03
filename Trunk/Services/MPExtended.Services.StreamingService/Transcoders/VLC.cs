#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers
// http://mpextended.codeplex.com/
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
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using MPExtended.Services.StreamingService.Units;
using MPExtended.Services.StreamingService.Util;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Code;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class VLC : ITranscoder
    {
        public TranscoderProfile Profile { get; set; }
        public string Input { get; set; }

        public VLC(TranscoderProfile profile)
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
            return Profile.CodecParameters["path"];
        }

        public string GenerateArguments(WebMediaInfo info, Resolution outputSize, int position, int? audioId, int? subtitleId)
        {
            // audio language selection
            string audioTrack = "";
            if (audioId != null)
                audioTrack = "--audio-track " + info.AudioStreams.Where(x => x.ID == audioId).First().Index;

            // subtitle language selection
            // TODO: support external subtitle files
            string subtitleTranscoder = "";
            string subtitleArguments = "";
            if (subtitleId != null)
            {
                WebSubtitleStream stream = info.SubtitleStreams.Where(x => x.ID == subtitleId).First();
                if (stream is WebExternalSubtitleStream)
                {
                    subtitleArguments = "--sub-file=" + ((WebExternalSubtitleStream)stream).Filename;
                }
                else
                {
                    subtitleArguments = "--sub-track " + stream.Index;
                }
                subtitleTranscoder += ",soverlay";
            }

            // prepare output path (some trickying for VLC)
            string path = @"\#OUT#";
            string muxer = Profile.CodecParameters["muxer"].Replace("#OUT#", path);

            // arguments
            string arguments = "-I dummy -vvv \"#IN#\" " + subtitleArguments + " " + audioTrack + " --sout ";
            arguments += "\"#transcode{" + Profile.CodecParameters["encoder"] + ",width=" + outputSize.Width + ",height=" + outputSize.Height + subtitleTranscoder + "}";
            arguments += muxer + "\"";

            if (!InputReaderWanted())
            {
                arguments = arguments.Replace("#IN#", Input);
            }

            return arguments;
        }

        public ILogProcessingUnit GetLogParsingUnit(Reference<Util.EncodingInfo> save)
        {
            return null;
        }
    }
}
