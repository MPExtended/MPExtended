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
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Services.StreamingService.Units;
using MPExtended.Services.StreamingService.Util;
using MPExtended.Services.StreamingService.Transcoders;

namespace MPExtended.Services.StreamingService.Code
{
    internal class Streaming
    {
        private static Dictionary<string, ActiveStream> Streams = new Dictionary<string, ActiveStream>();

        private class ActiveStream
        {
            public string Identifier { get; set; }
            public string ClientDescription { get; set; }
            public TranscoderProfile Profile { get; set; }

            public string Source { get; set; }
            public bool IsTsBuffer { get; set; }
            public Resolution OutputSize { get; set; }

            public ITranscoder Transcoder { get; set; }
            public Pipeline Pipeline { get; set; }
            public EncodingInfo EncodingInfo { get; set; }
        }

        public bool InitStream(string identifier, string clientDescription, string source)
        {
            ActiveStream stream = new ActiveStream();
            stream.Identifier = identifier;
            stream.ClientDescription = clientDescription;
            stream.Source = source;
            stream.IsTsBuffer = source.EndsWith(".ts.tsbuffer");

            Streams[identifier] = stream;
            return true;
        }

        public bool StartStream(string identifier, TranscoderProfile profile, int position = 0, int? audioId = null, int? subtitleId = null)
        {
            if (!Streams.ContainsKey(identifier) || Streams[identifier] == null)
            {
                Log.Warn("Stream requested for invalid identifier {0}", identifier);
                return false;
            }

            if (profile == null)
            {
                Log.Warn("Stream requested for non-existent profile");
                return false;
            }

            try
            {
                ActiveStream stream = Streams[identifier];
                stream.Profile = profile;
                stream.OutputSize = CalculateSize(stream.Profile, stream.Source, stream.IsTsBuffer);
                Log.Trace("Using {0} as output size for stream {1}", stream.OutputSize, identifier);

                // get media info
                WebMediaInfo info;
                if (stream.IsTsBuffer)
                {
                    info = MediaInfoWrapper.GetMediaInfo(new TsBuffer(stream.Source));
                }
                else
                {
                    info = MediaInfoWrapper.GetMediaInfo(stream.Source);
                }
                
                // get transcoder
                stream.Transcoder = profile.GetTranscoder();
                stream.Transcoder.Input = stream.Source;
                stream.Transcoder.MediaInfo = info;
                stream.Transcoder.Identifier = identifier;

                // check for validness of ids
                if (info.AudioStreams.Where(x => x.ID == audioId).Count() == 0) 
                    audioId = null;
                if (info.SubtitleStreams.Where(x => x.ID == subtitleId).Count() == 0)
                    subtitleId = null;

                // build the pipeline
                stream.Pipeline = new Pipeline();
                stream.EncodingInfo = new EncodingInfo();
                Reference<EncodingInfo> eref = new Reference<EncodingInfo>(() => stream.EncodingInfo, x => { stream.EncodingInfo = x; });
                stream.Transcoder.AlterPipeline(stream.Pipeline, stream.OutputSize, eref, position, audioId, subtitleId);

                // start the processes
                stream.Pipeline.Assemble();
                stream.Pipeline.Start();

                Log.Info("Started stream with identifier " + identifier);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to start stream " + identifier, ex);
                return false;
            }
        }

        public Stream RetrieveStream(string identifier)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = Streams[identifier].Profile.MIME;
            return Streams[identifier].Pipeline.GetFinalStream();
        }

        public Stream HttpLiveStreaming(string identifier, string action, string parameters)
        {
            return ((HTTPLiveTranscoderWrapper)Streams[identifier].Transcoder).DoAction(action, parameters);
        }

        public void EndStream(string identifier)
        {
            if (!Streams.ContainsKey(identifier) || Streams[identifier] == null || Streams[identifier].Pipeline == null || !Streams[identifier].Pipeline.IsStarted)
                return;

            try
            {
                Log.Debug("Stopping stream with identifier " + identifier);
                Streams[identifier].Pipeline.Stop();
                Streams[identifier].Pipeline = null;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to stop stream " + identifier, ex);
            }
        }

        public void KillStream(string identifier)
        {
            EndStream(identifier);
            Streams.Remove(identifier);
            Log.Debug("Killed stream with identifier {0}", identifier);
        }

        public List<WebStreamingSession> GetStreamingSessions()
        {
            return Streams.Select(s => s.Value).Select(s => new WebStreamingSession()
            {
                ClientDescription = s.ClientDescription,
                Identifier = s.Identifier,
                SourceFile = s.Source,
                Profile = s.Profile != null ? s.Profile.Name : null,
                TranscodingInfo = s.EncodingInfo != null ? s.EncodingInfo.ToWebTranscodingInfo() : null
            }).ToList();
        }

        public EncodingInfo GetEncodingInfo(string identifier) 
        {
            if (Streams.ContainsKey(identifier) && Streams[identifier] != null)
                return Streams[identifier].EncodingInfo;

            return null;
        }

        public Resolution CalculateSize(TranscoderProfile profile, string file, bool isTsBuffer = false)
        {
            if (!profile.HasVideoStream)
                return new Resolution(0, 0);

            decimal aspect;
            if (isTsBuffer)
            {
                // FIXME: we might want to support TV with other aspect ratios
                aspect = (decimal)16 / 9;
            }
            else
            {
                WebMediaInfo info = MediaInfoWrapper.GetMediaInfo(file);
                aspect = info.VideoStreams.First().DisplayAspectRatio;
            }
            return Resolution.Calculate(aspect, new Resolution(profile.MaxOutputWidth, profile.MaxOutputHeight), 2);
        }
    }
}
