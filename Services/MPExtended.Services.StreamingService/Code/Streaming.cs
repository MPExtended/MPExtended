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
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using System.Threading;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.ConfigurationContracts;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Services.StreamingService.Transcoders;

namespace MPExtended.Services.StreamingService.Code
{
    internal class Streaming : IDisposable
    {
#if DEBUG
        private const int ALLOW_STREAM_IDLE_TIME = 30 * 1000; // use shorter time for debugging
#else
        private const int ALLOW_STREAM_IDLE_TIME = 5 * 1000; // two minutes was doomed to long by a lot of people for WebMP, so let's try 5 seconds
#endif
        public const int STREAM_NONE = -2;
        public const int STREAM_DEFAULT = -1;

        private WatchSharing sharing;
        private StreamingService service;
        private static Dictionary<string, ActiveStream> Streams = new Dictionary<string, ActiveStream>();

        private class ActiveStream
        {
            public string Identifier { get; set; }
            public string ClientDescription { get; set; }
            public string ClientIP { get; set; }
            public DateTime StartTime { get; set; } // date the stream was initialized (no use)

            public ITranscoder Transcoder { get; set; }
            public ReadTrackingStreamWrapper OutputStream { get; set; }

            public StreamContext Context { get; set; }
        }

        public Streaming(StreamingService serviceInstance)
        {
            sharing = new WatchSharing();
            service = serviceInstance;
            ThreadManager.Start("StreamTimeout", TimeoutStreamsWorker);
        }

        public void Dispose()
        {
            try
            {
                sharing.Dispose();
                ThreadManager.Abort("StreamTimeout");
                foreach (string identifier in Streams.Keys)
                {
                    EndStream(identifier);
                }
            }
            catch (Exception ex)
            {
                Log.Info("Failed to dispose WSS", ex);
            }
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
                            .Where(x => x.Value.OutputStream != null && x.Value.OutputStream.TimeSinceLastRead > ALLOW_STREAM_IDLE_TIME)
                            .Select(x => x.Value.Identifier)
                            .ToList();

                        foreach (string key in toDelete)
                        {
                            Log.Info("Stream {0} has been idle for {1} milliseconds, so cancel it", key, Streams[key].OutputStream.TimeSinceLastRead);
                            service.FinishStream(key);
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
            if (!source.Exists)
            {
                Log.Warn("Tried to start stream for non-existing file {0}", source.GetDebugName());
                return false;
            }

            ActiveStream stream = new ActiveStream();
            stream.Identifier = identifier;
            stream.ClientDescription = clientDescription;
            stream.ClientIP = WCFUtil.GetClientIPAddress();
            stream.StartTime = DateTime.Now;
            stream.Context = new StreamContext();
            stream.Context.Source = source;
            stream.Context.IsTv = source.MediaType == WebStreamMediaType.TV;

            lock (Streams)
            {
                Streams[identifier] = stream;
            }
            return true;
        }

        public string StartStream(string identifier, TranscoderProfile profile, int position = 0, int audioId = STREAM_DEFAULT, int subtitleId = STREAM_DEFAULT)
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
                    Log.Trace("StartStream called with identifier {0}", identifier);

                    // initialize stream and context
                    ActiveStream stream = Streams[identifier];
                    stream.Context.StartPosition = position;
                    stream.Context.Profile = profile;
                    stream.Context.MediaInfo = MediaInfoHelper.LoadMediaInfoOrSurrogate(stream.Context.Source);
                    stream.Context.OutputSize = CalculateSize(stream.Context);
                    Reference<WebTranscodingInfo> infoRef = new Reference<WebTranscodingInfo>(() => stream.Context.TranscodingInfo, x => { stream.Context.TranscodingInfo = x; });
                    Log.Trace("Using {0} as output size for stream {1}", stream.Context.OutputSize, identifier);
                    sharing.StartStream(stream.Context, infoRef);
                
                    // get transcoder
                    stream.Transcoder = (ITranscoder)Activator.CreateInstance(Type.GetType(profile.TranscoderImplementationClass));
                    stream.Transcoder.Identifier = identifier;

                    // get audio and subtitle id
                    if (stream.Context.MediaInfo.AudioStreams.Where(x => x.ID == audioId).Count() > 0)
                    {
                        stream.Context.AudioTrackId = stream.Context.MediaInfo.AudioStreams.Where(x => x.ID == audioId).First().ID;
                    }
                    else if (audioId == STREAM_DEFAULT)
                    {
                        string preferredLanguage = Configuration.Streaming.DefaultAudioStream;
                        if (stream.Context.MediaInfo.AudioStreams.Count(x => x.Language == preferredLanguage) > 0)
                        {
                            stream.Context.AudioTrackId = stream.Context.MediaInfo.AudioStreams.First(x => x.Language == preferredLanguage).ID;
                        }
                        else if (preferredLanguage != "none" && stream.Context.MediaInfo.AudioStreams.Count() > 0)
                        {
                            stream.Context.AudioTrackId = stream.Context.MediaInfo.AudioStreams.First().ID;
                        }
                    }

                    if (stream.Context.MediaInfo.SubtitleStreams.Where(x => x.ID == subtitleId).Count() > 0)
                    {
                        stream.Context.SubtitleTrackId = stream.Context.MediaInfo.SubtitleStreams.Where(x => x.ID == subtitleId).First().ID;
                    }
                    else if (subtitleId == STREAM_DEFAULT)
                    {
                        string preferredLanguage = Configuration.Streaming.DefaultSubtitleStream;
                        if (stream.Context.MediaInfo.SubtitleStreams.Count(x => x.Language == preferredLanguage) > 0)
                        {
                            stream.Context.SubtitleTrackId = stream.Context.MediaInfo.SubtitleStreams.First(x => x.Language == preferredLanguage).ID;
                        }
                        else if (preferredLanguage == "external" && stream.Context.MediaInfo.SubtitleStreams.Count(x => x.Filename != null) > 0)
                        {
                            stream.Context.SubtitleTrackId = stream.Context.MediaInfo.SubtitleStreams.First(x => x.Filename != null).ID;
                        }
                        else if (preferredLanguage == "first" && stream.Context.MediaInfo.SubtitleStreams.Count() > 0)
                        {
                            stream.Context.SubtitleTrackId = stream.Context.MediaInfo.SubtitleStreams.First().ID;
                        }
                    }
                    Log.Debug("Final stream selection: audioId={0}, subtitleId={1}", stream.Context.AudioTrackId, stream.Context.SubtitleTrackId);

                    // build the pipeline
                    stream.Context.Pipeline = new Pipeline();
                    stream.Context.TranscodingInfo = new WebTranscodingInfo();
                    stream.Transcoder.BuildPipeline(stream.Context);

                    // start the processes and retrieve output stream
                    stream.Context.Pipeline.Assemble();
                    stream.Context.Pipeline.Start();

                    Stream finalStream = Streams[identifier].Context.Pipeline.GetFinalStream();
                    if (finalStream != null)
                    {
                        Streams[identifier].OutputStream = new ReadTrackingStreamWrapper(finalStream);
                    }

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
                WebOperationContext.Current.OutgoingResponse.ContentType = Streams[identifier].Context.Profile.MIME;
                if(Streams[identifier].OutputStream == null)
                {
                    Log.Warn("Encountered null stream in RetrieveStream for identifier {0}", identifier);
                    WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                    return Stream.Null;
                }

                WCFUtil.SetContentType(Streams[identifier].Context.Profile.MIME);

                if (Streams[identifier].Transcoder is IRetrieveHookTranscoder)
                {
                    (Streams[identifier].Transcoder as IRetrieveHookTranscoder).RetrieveStreamCalled(Streams[identifier].Context);
                }

                return Streams[identifier].OutputStream;
            }
        }

        public Stream CustomTranscoderData(string identifier, string action, string parameters)
        {
            lock (Streams[identifier])
            {
                if (!(Streams[identifier].Transcoder is ICustomActionTranscoder))
                    return Stream.Null;

                return ((ICustomActionTranscoder)Streams[identifier].Transcoder).DoAction(action, parameters);
            }
        }

        public void EndStream(string identifier)
        {
            if (!Streams.ContainsKey(identifier) || Streams[identifier] == null || Streams[identifier].Context.Pipeline == null || !Streams[identifier].Context.Pipeline.IsStarted)
                return;

            try
            {
                lock (Streams[identifier])
                {
                    Log.Debug("Stopping stream with identifier " + identifier);
                    sharing.EndStream(Streams[identifier].Context.Source);
                    Streams[identifier].Context.Pipeline.Stop();
                    Streams[identifier].Context.Pipeline = null;
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
            lock (Streams)
            {
                return Streams.Select(s => s.Value).Select(s => new WebStreamingSession()
                {
                    ClientDescription = s.ClientDescription,
                    ClientIPAddress = s.ClientIP,
                    Identifier = s.Identifier,
                    SourceType = s.Context.Source.MediaType,
                    SourceId = s.Context.Source.Id,
                    Profile = s.Context.Profile != null ? s.Context.Profile.Name : null,
                    StartTime = s.StartTime,
                    DisplayName = s.Context.Source.GetMediaDisplayName(),

                    StartPosition = s.Context.StartPosition,
                    PlayerPosition = s.Context.GetPlayerPosition(),
                    TranscodingInfo = s.Context.TranscodingInfo != null ? s.Context.TranscodingInfo : null,
                }).ToList();
            }
        }

        public void SetPlayerPosition(string identifier, int playerPosition)
        {
            if (Streams.ContainsKey(identifier) && Streams[identifier] != null)
            {
                Streams[identifier].Context.SyncedPlayerPosition = playerPosition;
                Streams[identifier].Context.LastPlayerPositionSync = DateTime.Now;
            }
        }

        public WebTranscodingInfo GetEncodingInfo(string identifier) 
        {
            if (Streams.ContainsKey(identifier) && Streams[identifier] != null)
                return Streams[identifier].Context.TranscodingInfo;

            Log.Warn("Requested transcoding info for unknown identifier {0}", identifier);
            return null;
        }

        public Resolution CalculateSize(TranscoderProfile profile, MediaSource source, WebMediaInfo info = null)
        {
            try
            {
                if (!profile.HasVideoStream)
                    return new Resolution(0, 0);

                if (info == null)
                {
                    info = MediaInfoHelper.LoadMediaInfoOrSurrogate(source);
                }

                if (info.VideoStreams.Count > 0)
                {
                    return Resolution.Calculate(info.VideoStreams.First().DisplayAspectRatio, profile.MaxOutputWidth, profile.MaxOutputHeight, 2);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to calculate size of output stream", ex);
            }

            // default
            return Resolution.Calculate(16 / 9, profile.MaxOutputWidth, profile.MaxOutputHeight, 2);
        }

        public Resolution CalculateSize(StreamContext context)
        {
            return CalculateSize(context.Profile, context.Source, context.MediaInfo);
        }
    }
}
