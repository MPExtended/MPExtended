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
    internal abstract class VLCBaseTranscoder : ITranscoder
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
            var encmuxparam = GetEncoderMuxerParameters(context);
            return GenerateVLCParameters(
                context,
                context.Profile.CodecParameters.ContainsKey("options") ? context.Profile.CodecParameters["options"] : "",
                context.Profile.CodecParameters.ContainsKey("tsOptions") ? context.Profile.CodecParameters["tsOptions"] : "",
                context.Profile.CodecParameters.ContainsKey("disableSeeking") && context.Profile.CodecParameters["disableSeeking"] == "yes",
                encmuxparam.Item1,
                encmuxparam.Item2
            );
        }

        protected VLCParameters GenerateVLCParameters(StreamContext context, string options, string tsOptions, bool disableSeeking, string encoderOptions, string muxerOptions)
        {
            List<string> arguments = options.Split(' ').Where(x => x.Length > 0).ToList();

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
            if ((context.IsTv || context.MediaInfo.Container == "MPEG-TS") && tsOptions.Length > 0)
            {
                arguments.AddRange(tsOptions.Split(' ').Where(x => x.Length > 0));
            }

            // position (disabling this is probably a bad idea as some things (watch sharing, transcoding info) fail then, which results in faulty clients.)
            if (context.StartPosition > 0 && !disableSeeking)
            {
                arguments.Add("--start-time=" + (context.StartPosition / 1000));
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
            string sout = "#transcode{" + encoderOptions + 
                ",width=" + context.OutputSize.Width + ",height=" + context.OutputSize.Height + "," + subtitleTranscoder + "}" +
                muxerOptions;

            // return
            return new VLCParameters()
            {
                Sout = sout,
                Arguments = arguments.ToArray(),
                Input = inURL
            };
        }

        protected virtual Tuple<string, string> GetEncoderMuxerParameters(StreamContext context)
        {
            return new Tuple<string, string>(
                context.Profile.CodecParameters["encoder"],
                context.Profile.CodecParameters["muxer"]
            );
        }

        public abstract void BuildPipeline(StreamContext context);
    }
}
