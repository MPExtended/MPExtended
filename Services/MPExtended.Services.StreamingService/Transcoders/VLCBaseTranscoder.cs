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
    abstract class VLCBaseTranscoder : ITranscoder
    {
        public string Identifier { get; set; }

        protected class VLCParameters
        {
            public string Sout { get; set; }
            public string Input { get; set; }
            public string[] Arguments { get; set; }
        }

        public virtual string GetStreamURL()
        {
            return WCFUtil.GetCurrentRoot() + "StreamingService/stream/RetrieveStream?identifier=" + Identifier;
        }

        protected VLCParameters GenerateVLCParameters(StreamContext context)
        {
            List<string> arguments = context.Profile.CodecParameters.ContainsKey("options") ?
                context.Profile.CodecParameters["options"].Split(' ').Where(x => x.Length > 0).ToList() :
                new List<string>();

            // input
            string inURL = "";
            if (context.Source.NeedsInputReaderUnit)
            {
                inURL = "stream://#IN#";
            }
            else
            {
                inURL = context.Source.GetPath();
            }

            // add tv options if specified
            if ( (context.IsTv || context.MediaInfo.Container == "MPEG-TS") &&
                  context.Profile.CodecParameters.ContainsKey("tsOptions") && context.Profile.CodecParameters["tsOptions"].Length > 0)
            {
                arguments.AddRange(context.Profile.CodecParameters["tsOptions"].Split(' ').Where(x => x.Length > 0));
            }

            // position
            if (context.StartPosition > 0)
            {
                // disabling this is probably a bit idea as some things (watch sharing, transcoding info) fail then, which results in faulty clients.
                if (!context.Profile.CodecParameters.ContainsKey("disableSeeking") || context.Profile.CodecParameters["disableSeeking"] == "no")
                {
                    arguments.Add("--start-time=" + (context.StartPosition / 1000));
                }
            }

            // audio track 
            if (context.AudioTrackId != null)
            {
                arguments.Add("--audio-track=" + context.MediaInfo.AudioStreams.Where(x => x.ID == context.AudioTrackId).First().Index);
            }
            else
            {
                Log.Warn("VLC streaming without audio track is not implemented yet");
            }

            // subtitle selection
            string subtitleTranscoder;
            if (context.SubtitleTrackId == null)
            {
                subtitleTranscoder = "scodec=none";
            } 
            else
            {
                WebSubtitleStream stream = context.MediaInfo.SubtitleStreams.First(x => x.ID == context.SubtitleTrackId);
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
                "#transcode{" + context.Profile.CodecParameters["encoder"] + 
                ",width=" + context.OutputSize.Width + ",height=" + context.OutputSize.Height + "," + subtitleTranscoder + "}" +
                context.Profile.CodecParameters["muxer"];

            // return
            return new VLCParameters()
            {
                Sout = sout,
                Arguments = arguments.ToArray(),
                Input = inURL
            };
        }

        public abstract void BuildPipeline(StreamContext context);
    }
}
