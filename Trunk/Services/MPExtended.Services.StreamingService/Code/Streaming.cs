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
using System.Threading;
using MPExtended.Libraries.General;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Services.StreamingService.Transcoders;

namespace MPExtended.Services.StreamingService.Code
{
    internal class Streaming
    {
#if DEBUG
        private const int ALLOW_STREAM_IDLE_TIME = 30 * 1000; // in milliseconds, use shorter time for debugging
#else
        private const int ALLOW_STREAM_IDLE_TIME = 2 * 60 * 1000; // in milliseconds, 2 minutes seams reasonable
#endif

        private WatchSharing sharing;
        private Thread timeoutWorker;
        private static Dictionary<string, ActiveStream> Streams = new Dictionary<string, ActiveStream>();

        private class ActiveStream
        {
            public string Identifier { get; set; }
            public string ClientDescription { get; set; }
            public TranscoderProfile Profile { get; set; }

            public MediaSource Source { get; set; }
            public Resolution OutputSize { get; set; }

            public ITranscoder Transcoder { get; set; }
            public Pipeline Pipeline { get; set; }
            public WebTranscodingInfo TranscodingInfo { get; set; }

            public ReadTrackingStreamWrapper OutputStream { get; set; }
        }

        public Streaming()
        {
            sharing = new WatchSharing();
            timeoutWorker = new Thread(TimeoutStreamsWorker);
            timeoutWorker.Start();
        }

        ~Streaming()
        {
            timeoutWorker.Abort();
        }

        private void TimeoutStreamsWorker()
        {
            while (true)
            {
                try
                {
                    lock (Streams)
                    {
                        var toDelete = Streams
                            .Where(x => x.Value.OutputStream != null && x.Value.OutputStream.MillisecondsSinceLastRead > ALLOW_STREAM_IDLE_TIME)
                            .Select(x => x.Value.Identifier)
                            .ToList();

                        foreach (string key in toDelete)
                        {
                            Log.Info("Stream {0} has been idle for {1} milliseconds, so cancel it", key, Streams[key].OutputStream.MillisecondsSinceLastRead);
                            KillStream(key);
                        }
                    }

                    Thread.Sleep(ALLOW_STREAM_IDLE_TIME);
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log.Warn("Error in timeout stream worker", ex);
                }
            }
        }

        public bool InitStream(string identifier, string clientDescription, MediaSource source)
        {
            ActiveStream stream = new ActiveStream();
            stream.Identifier = identifier;
            stream.ClientDescription = clientDescription;
            stream.Source = source;

            lock (Streams)
            {
                Streams[identifier] = stream;
            }
            return true;
        }

        public string StartStream(string identifier, TranscoderProfile profile, int position = 0, int? audioId = null, int? subtitleId = null)
        {
            // there's a theoretical race condition here between the insert in InitStream() and this, but the client should really, really
            // always have a positive result from InitStream() before continuing, so their bad that the stream failed. 
            if (!Streams.ContainsKey(identifier) || Streams[identifier] == null)
            {
                Log.Warn("Stream requested for invalid identifier {0}", identifier);
                return null;
            }

            if (profile == null)
            {
                Log.Warn("Stream requested for non-existent profile");
                return null;
            }

            try
            {
                lock (Streams[identifier]) 
                {
                    ActiveStream stream = Streams[identifier];
                    stream.Profile = profile;
                    stream.OutputSize = CalculateSize(stream.Profile, stream.Source);
                    Reference<WebTranscodingInfo> infoRef = new Reference<WebTranscodingInfo>(() => stream.TranscodingInfo, x => { stream.TranscodingInfo = x; });
                    Log.Trace("Using {0} as output size for stream {1}", stream.OutputSize, identifier);

                    // get media info
                    WebMediaInfo info = MediaInfoWrapper.GetMediaInfo(stream.Source);

                    // share it
                    sharing.StartStream(stream.Source, infoRef, position);
                
                    // get transcoder
                    stream.Transcoder = profile.GetTranscoder();
                    stream.Transcoder.Source = stream.Source;
                    stream.Transcoder.MediaInfo = info;
                    stream.Transcoder.Identifier = identifier;

                    // get transcoder
                    stream.Transcoder = profile.GetTranscoder();
                    stream.Transcoder.Source = stream.Source;
                    stream.Transcoder.MediaInfo = info;
                    stream.Transcoder.Identifier = identifier;

                    // check for validness of ids
                    if (info.AudioStreams.Where(x => x.ID == audioId).Count() == 0)
                        audioId = null;
                    if (info.SubtitleStreams.Where(x => x.ID == subtitleId).Count() == 0)
                        subtitleId = null;

                    // build the pipeline
                    stream.Pipeline = new Pipeline();
                    stream.TranscodingInfo = new WebTranscodingInfo();
                    stream.Transcoder.AlterPipeline(stream.Pipeline, stream.OutputSize, infoRef, position, audioId, subtitleId);

                    // start the processes and retrieve output stream
                    stream.Pipeline.Assemble();
                    stream.Pipeline.Start();
                    Streams[identifier].OutputStream = new ReadTrackingStreamWrapper(Streams[identifier].Pipeline.GetFinalStream());

                    Log.Info("Started stream with identifier " + identifier);
                    return stream.Transcoder.GetStreamURL();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to start stream " + identifier, ex);
                return null;
            }
        }

        public Stream RetrieveStream(string identifier)
        {
            lock (Streams[identifier])
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = Streams[identifier].Profile.MIME;
                return Streams[identifier].OutputStream;
            }
        }

        public Stream CustomTranscoderData(string identifier, string action, string parameters)
        {
            lock (Streams[identifier])
            {
                if (!(Streams[identifier].Transcoder is ICustomActionTranscoder))
                    return null;

                return ((ICustomActionTranscoder)Streams[identifier].Transcoder).DoAction(action, parameters);
            }
        }

        public void EndStream(string identifier)
        {
            if (!Streams.ContainsKey(identifier) || Streams[identifier] == null || Streams[identifier].Pipeline == null || !Streams[identifier].Pipeline.IsStarted)
                return;

            try
            {
                lock (Streams[identifier])
                {
                    Log.Debug("Stopping stream with identifier " + identifier);
                    sharing.EndStream(Streams[identifier].Source);
                    Streams[identifier].Pipeline.Stop();
                    Streams[identifier].Pipeline = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to stop stream " + identifier, ex);
            }
        }

        public void KillStream(string identifier)
        {
            EndStream(identifier);
            lock (Streams)
            {
                Streams.Remove(identifier);
            }
            Log.Debug("Killed stream with identifier {0}", identifier);
        }

        public List<WebStreamingSession> GetStreamingSessions()
        {
            return Streams.Select(s => s.Value).Select(s => new WebStreamingSession()
            {
                ClientDescription = s.ClientDescription,
                Identifier = s.Identifier,
                SourceType = s.Source.MediaType,
                SourceId = s.Source.Id,
                Profile = s.Profile != null ? s.Profile.Name : null,
                TranscodingInfo = s.TranscodingInfo != null ? s.TranscodingInfo : null
            }).ToList();
        }

        public WebTranscodingInfo GetEncodingInfo(string identifier) 
        {
            if (Streams.ContainsKey(identifier) && Streams[identifier] != null)
                return Streams[identifier].TranscodingInfo;

            return null;
        }

        public Resolution CalculateSize(TranscoderProfile profile, MediaSource source)
        {
            if (!profile.HasVideoStream)
                return new Resolution(0, 0);

            decimal aspect;
            if (source.MediaType == WebStreamMediaType.TV)
            {
                // FIXME: we might want to support TV with other aspect ratios
                aspect = (decimal)16 / 9;
            }
            else
            {
                WebMediaInfo info = MediaInfoWrapper.GetMediaInfo(source);
                aspect = info.VideoStreams.First().DisplayAspectRatio;
            }
            return Resolution.Calculate(aspect, new Resolution(profile.MaxOutputWidth, profile.MaxOutputHeight), 2);
        }
    }
}
