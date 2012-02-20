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
using System.Threading;
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal abstract class VLCBaseTranscoder : ITranscoder
    {
        protected class VLCParameters
        {
            public string Sout { get; set; }
            public string Input { get; set; }
            public string[] Arguments { get; set; }
        }

        public string Identifier { get; set; }

        private string streamUrl;

        public VLCBaseTranscoder()
        {
            streamUrl = WCFUtil.GetCurrentRoot() + "StreamingService/stream/RetrieveStream?identifier={0}";
        }

        public virtual string GetStreamURL(StreamContext context)
        {
            if (context.Profile.CodecParameters.ContainsKey("rtspOutput") && context.Profile.CodecParameters["rtspOutput"] == "yes")
            {
                Thread.Sleep(2500); // FIXME
                return "rtsp://" + WCFUtil.GetCurrentHostname() + ":5544/" + Identifier + ".sdp";
            }
            else
            {
                return WCFUtil.GetCurrentRoot() + "StreamingService/stream/RetrieveStream?identifier=" + Identifier;
            }
        }

        public virtual void BuildPipeline(StreamContext context)
        {
            // input
            bool doInputReader = context.Source.NeedsInputReaderUnit;
            if (doInputReader)
            {
                context.Pipeline.AddDataUnit(context.Source.GetInputReaderUnit(), 1);
            }

            // then add the encoder (this should happen in position 5)
            AddEncoderToPipeline(context, doInputReader);

            // add the FLV metadata injection unit, when option is set
            if (context.Profile.CodecParameters.ContainsKey("flvMetadataInjection") && context.Profile.CodecParameters["flvMetadataInjection"] == "yes")
            {
                context.Pipeline.AddDataUnit(new FLVMetadataInjector(context), 15);
            }
        }

        protected abstract void AddEncoderToPipeline(StreamContext context, bool hasInputReader);

        protected VLCParameters GenerateVLCParameters(StreamContext context)
        {
            string muxer = context.Profile.CodecParameters["muxer"];
            if (context.Profile.CodecParameters.ContainsKey("rtspOutput") && context.Profile.CodecParameters["rtspOutput"] == "yes")
            {
                muxer = muxer.Replace("#ADDRESS#", "rtsp://:5544/" + Identifier + ".sdp");
            }

            return GenerateVLCParameters(
                context,
                context.Profile.CodecParameters.ContainsKey("options") ? context.Profile.CodecParameters["options"] : "",
                context.Profile.CodecParameters.ContainsKey("tsOptions") ? context.Profile.CodecParameters["tsOptions"] : "",
                context.Profile.CodecParameters.ContainsKey("disableSeeking") && context.Profile.CodecParameters["disableSeeking"] == "yes",
                context.Profile.CodecParameters["encoder"],
                context.Profile.CodecParameters["muxer"]
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
            string sout = "#transcode{" + encoderOptions + "," + subtitleTranscoder;
            if (!context.Profile.CodecParameters.ContainsKey("noResize") || context.Profile.CodecParameters["noResize"] != "true")
                sout += ",width=" + context.OutputSize.Width + ",height=" + context.OutputSize.Height;
            sout += "}" + muxerOptions;

            // return
            return new VLCParameters()
            {
                Sout = sout,
                Arguments = arguments.ToArray(),
                Input = inURL
            };
        }
    }
}
