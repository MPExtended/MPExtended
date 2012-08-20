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
        public StreamContext Context { get; set; }

        private string streamUrl;

        public VLCBaseTranscoder()
        {
            streamUrl = WCFUtil.GetCurrentRoot() + "StreamingService/stream/RetrieveStream?identifier={0}";
        }

        public virtual string GetStreamURL()
        {
            if (Context.Profile.TranscoderParameters.ContainsKey("rtspOutput") && Context.Profile.TranscoderParameters["rtspOutput"] == "yes")
            {
                Thread.Sleep(2500); // FIXME
                return "rtsp://" + WCFUtil.GetCurrentHostname() + ":5544/" + Identifier + ".sdp";
            }
            else
            {
                return WCFUtil.GetCurrentRoot() + "StreamingService/stream/RetrieveStream?identifier=" + Identifier;
            }
        }

        public virtual void BuildPipeline()
        {
            // input
            bool doInputReader = Context.Source.NeedsInputReaderUnit;
            if (doInputReader)
            {
                Context.Pipeline.AddDataUnit(Context.Source.GetInputReaderUnit(), 1);
            }

            // then add the encoder (this should happen in position 5)
            AddEncoderToPipeline(doInputReader);

            // add the FLV metadata injection unit, when option is set
            if (Context.Profile.TranscoderParameters.ContainsKey("flvMetadataInjection") && Context.Profile.TranscoderParameters["flvMetadataInjection"] == "yes")
            {
                Context.Pipeline.AddDataUnit(new FLVMetadataInjector(Context), 15);
            }
        }

        protected abstract void AddEncoderToPipeline(bool hasInputReader);

        protected virtual VLCParameters GenerateVLCParameters()
        {
            string muxer = Context.Profile.TranscoderParameters["muxer"];
            if (Context.Profile.TranscoderParameters.ContainsKey("rtspOutput") && Context.Profile.TranscoderParameters["rtspOutput"] == "yes")
            {
                muxer = muxer.Replace("#ADDRESS#", "rtsp://:5544/" + Identifier + ".sdp");
            }

            return GenerateVLCParameters(
                Context.Profile.TranscoderParameters.ContainsKey("options") ? Context.Profile.TranscoderParameters["options"] : String.Empty,
                Context.Profile.TranscoderParameters.ContainsKey("tsOptions") ? Context.Profile.TranscoderParameters["tsOptions"] : String.Empty,
                Context.Profile.TranscoderParameters.ContainsKey("disableSeeking") && Context.Profile.TranscoderParameters["disableSeeking"] == "yes",
                Context.Profile.TranscoderParameters.ContainsKey("encoder") ? Context.Profile.TranscoderParameters["encoder"] : String.Empty,
                Context.Profile.TranscoderParameters["muxer"]
            );
        }

        protected virtual VLCParameters GenerateVLCParameters(string options, string tsOptions, bool disableSeeking, string encoderOptions, string muxerOptions)
        {
            List<string> arguments = options.Split(' ').Where(x => x.Length > 0).ToList();

            // input
            string inURL = "";
            if (Context.Source.NeedsInputReaderUnit)
            {
                inURL = @"stream://\#IN#";
            }
            else
            {
                inURL = Context.Source.GetPath();
            }

            // add tv options if specified
            if ((Context.IsTv || Context.MediaInfo.Container == "MPEG-TS") && tsOptions.Length > 0)
            {
                arguments.AddRange(tsOptions.Split(' ').Where(x => x.Length > 0));
            }

            // position (disabling this is probably a bad idea as some things (watch sharing, transcoding info) fail then, which results in faulty clients.)
            if (Context.StartPosition > 0 && !disableSeeking)
            {
                arguments.Add("--start-time=" + (Context.StartPosition / 1000));
            }

            // audio track 
            if (Context.AudioTrackId != null)
            {
                arguments.Add("--audio-track=" + Context.MediaInfo.AudioStreams.Where(x => x.ID == Context.AudioTrackId).First().Index);
            }
            else
            {
                Log.Warn("VLC streaming without audio track is not implemented yet");
            }

            // subtitle selection
            string subtitleTranscoder;
            if (Context.SubtitleTrackId == null)
            {
                subtitleTranscoder = "scodec=none";
            } 
            else
            {
                WebSubtitleStream stream = Context.MediaInfo.SubtitleStreams.First(x => x.ID == Context.SubtitleTrackId);
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
            string sout;
            if (!String.IsNullOrEmpty(encoderOptions))
            {
			    sout = "#transcode{" + encoderOptions + "," + subtitleTranscoder;
				if (!Context.Profile.TranscoderParameters.ContainsKey("noResize") || Context.Profile.TranscoderParameters["noResize"] != "true")
					sout += ",width=" + Context.OutputSize.Width + ",height=" + Context.OutputSize.Height;
				sout += "}" + muxerOptions;
            }
            else
            {
                sout = "#" + muxerOptions.Substring(1);
            }

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
