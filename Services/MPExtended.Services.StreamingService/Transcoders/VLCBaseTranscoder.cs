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
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Transcoders
{
    abstract class VLCBaseTranscoder : ITranscoder
    {
        public TranscoderProfile Profile { get; set; }
        public MediaSource Source { get; set; }
        public WebMediaInfo MediaInfo { get; set; }
        public string Identifier { get; set; }

        protected class VLCParameters
        {
            public string Sout { get; set; }
            public string Input { get; set; }
            public string[] Arguments { get; set; }
        }

        protected VLCParameters GenerateVLCParameters(Pipeline pipeline, WebResolution outputSize, int position, int? audioId, int? subtitleId)
        {
            List<string> arguments = Profile.CodecParameters["options"].Split(' ').ToList();

            // input
            string inURL = "";
            if (pipeline.GetDataUnit(1) != null && pipeline.GetDataUnit(1) is InputUnit)
            {
                inURL = "stream://#IN#";
            }
            else
            {
                inURL = Source.GetPath();
            }

            // position
            if (position > 0)
            {
                // TODO: this breaks the android player
                //arguments.Add("--input-fast-seek");
                //arguments.Add("--start-time=" + position);
            }

            // audio track 
            if (audioId != null)
            {
                arguments.Add("--audio-track=" + MediaInfo.AudioStreams.Where(x => x.ID == audioId).First().Index);
            }
            else
            {
                Log.Warn("VLC streaming without audio track is not implemented yet");
            }

            // subtitle selection
            string subtitleTranscoder;
            if (subtitleId == null)
            {
                subtitleTranscoder = "scodec=none";
            } 
            else
            {
                WebSubtitleStream stream = MediaInfo.SubtitleStreams.First(x => x.ID == subtitleId);
                if (stream.Filename != null)
                {
                    arguments.Add("--sub-file=" + stream.Filename);
                }
                else
                {
                    arguments.Add("--sub-track=" + stream.Index);
                }
                subtitleTranscoder = "soverlay";
            }

            // create parameters
            string sout =
                "#transcode{" + Profile.CodecParameters["encoder"] + ",width=" + outputSize.Width + ",height=" + outputSize.Height + "," + subtitleTranscoder + "}"
                + Profile.CodecParameters["muxer"];

            // return
            return new VLCParameters()
            {
                Sout = sout,
                Arguments = arguments.ToArray(),
                Input = inURL
            };
        }


        public abstract void AlterPipeline(Pipeline pipeline, WebResolution outputSize, Reference<WebTranscodingInfo> einfo, int position, int? audioId, int? subtitleId);
        public abstract string GetStreamURL();
    }
}
