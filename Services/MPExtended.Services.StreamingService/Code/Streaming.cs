#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using System.Timers;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.Service.Config;
using MPExtended.Libraries.Service.Hosting;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Services.StreamingService.Transcoders;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal class Streaming : IDisposable
    {
        public const int STREAM_NONE = -2;
        public const int STREAM_DEFAULT = -1;

        private WatchSharing sharing;
        private StreamingService service;
        private static IDictionary<string, ActiveStream> Streams = new ConcurrentDictionary<string, ActiveStream>();
        private Timer timeoutTimer;

        private class ActiveStream
        {
            public string Identifier { get; set; }
            public string ClientDescription { get; set; }
            public string ClientIP { get; set; }
            public DateTime StartTime { get; set; } // date the stream was initialized (no use)

            public int Timeout { get; set; } // in seconds!
            public DateTime LastActivity { get; set; }
            public bool UseActivityForTimeout { get; set; }

            public ITranscoder Transcoder { get; set; }
            public ReadTrackingStreamWrapper OutputStream { get; set; }

            public StreamContext Context { get; set; }
        }

        public Streaming(StreamingService serviceInstance)
        {
            service = serviceInstance;
            sharing = new WatchSharing();

            timeoutTimer = new Timer()
            {
                AutoReset = true,
                Interval = 1000,
            };
            timeoutTimer.Elapsed += new ElapsedEventHandler(TimeoutStreamsTick);
            timeoutTimer.Start();

            ServiceState.Stopping += delegate()
            {
                foreach (var identifier in Streams.Select(x => x.Value.Identifier).ToList())
                {
                    StreamLog.Warn(identifier, "Killing stream because of service stop");
                    KillStream(identifier);
                }
            };
        }

        public void Dispose()
        {
            try
            {
                sharing.Dispose();
                timeoutTimer.Stop();
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

        private void TimeoutStreamsTick(object source, ElapsedEventArgs args)
        {
            try
            {
                var toDelete = Streams
                    .Where(x =>
                        (x.Value.UseActivityForTimeout ||
                                (x.Value.OutputStream != null && x.Value.OutputStream.TimeSinceLastRead > (x.Value.Timeout * 1000))) &&
                        (x.Value.LastActivity.Add(TimeSpan.FromSeconds(x.Value.Timeout)) < DateTime.Now)
                    )
                    .Select(x => x.Value.Identifier)
                    .ToList();

                if (toDelete.Count > 0)
                {
                    foreach (string key in toDelete)
                    {
                        // The stream could've been terminated between the moment we decide it should be terminated and we get here. 
                        if (!Streams.ContainsKey(key))
                            continue;

                        if (Streams[key].UseActivityForTimeout)
                        {
                            StreamLog.Info(key, "Stream had last service activity at {0}, so cancel it", Streams[key].LastActivity);
                        }
                        else
                        {
                            StreamLog.Info(key, "Stream had last read {0} milliseconds ago (read {1} bytes in total) and last service activity at {2}, so cancel it", 
                                Streams[key].OutputStream.TimeSinceLastRead, Streams[key].OutputStream.ReadBytes, Streams[key].LastActivity);
                        }
                        service.FinishStream(key);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Error in TimeoutStreamsTick", ex);
            }
        }

        public bool InitStream(string identifier, string clientDescription, MediaSource source, int timeout)
        {
            if (!source.Exists)
            {
                StreamLog.Warn(identifier, "Tried to start stream for non-existing file {0}", source.GetDebugName());
                return false;
            }

            ActiveStream stream = new ActiveStream();
            stream.Identifier = identifier;
            stream.ClientDescription = clientDescription;
            stream.StartTime = DateTime.Now;
            stream.Timeout = timeout;
            stream.LastActivity = DateTime.Now;
            stream.UseActivityForTimeout = false;
            stream.Context = new StreamContext();
            stream.Context.Identifier = identifier;
            stream.Context.Source = source;
            stream.Context.IsTv = source.MediaType == WebMediaType.TV;

            // Some clients such as WebMP proxy the streams before relying it to the client. We should give these clients the option to
            // forward the real IP address, so that we can show that one in the configurator too. However, to avoid abuse, we should show
            // the real IP of the client too, so make up some nice text string. 
            string realIp = WCFUtil.GetHeaderValue("forwardedFor", "X-Forwarded-For");
            stream.ClientIP = realIp == null ? WCFUtil.GetClientIPAddress() : String.Format("{0} (via {1})", realIp, WCFUtil.GetClientIPAddress());

            Streams[identifier] = stream;
            return true;
        }

        public string StartStream(string identifier, TranscoderProfile profile, long position = 0, int audioId = STREAM_DEFAULT, int subtitleId = STREAM_DEFAULT)
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
                StreamLog.Warn(identifier, "Stream requested for non-existent profile");
                return null;
            }

            try
            {
                lock (Streams[identifier])
                {
                    StreamLog.Debug(identifier, "StartStream for file {0}", Streams[identifier].Context.Source.GetDebugName());

                    // initialize stream and context
                    ActiveStream stream = Streams[identifier];
                    stream.Context.StartPosition = position;
                    stream.Context.Profile = profile;
                    stream.UseActivityForTimeout = profile.Transport == "httplive";
                    stream.Context.MediaInfo = MediaInfoHelper.LoadMediaInfoOrSurrogate(stream.Context.Source);
                    stream.Context.OutputSize = CalculateSize(stream.Context);
                    StreamLog.Debug(identifier, "Using {0} as output size for stream {1}", stream.Context.OutputSize, identifier);
                    Reference<WebTranscodingInfo> infoRef = new Reference<WebTranscodingInfo>(() => stream.Context.TranscodingInfo, x => { stream.Context.TranscodingInfo = x; });
                    sharing.StartStream(stream.Context, infoRef);

                    // get transcoder
                    stream.Transcoder = (ITranscoder)Activator.CreateInstance(Type.GetType(profile.Transcoder));
                    stream.Transcoder.Identifier = identifier;
                    stream.Transcoder.Context = stream.Context;

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
                    StreamLog.Debug(identifier, "Final stream selection: audioId={0}, subtitleId={1}", stream.Context.AudioTrackId, stream.Context.SubtitleTrackId);

                    // build the pipeline
                    stream.Context.Pipeline = new Pipeline(identifier);
                    stream.Context.TranscodingInfo = new WebTranscodingInfo();
                    stream.Transcoder.BuildPipeline();

                    // start the pipeline, but imm
                    bool assembleResult = stream.Context.Pipeline.Assemble();
                    bool startResult = assembleResult ? stream.Context.Pipeline.Start() : true;
                    if (!assembleResult || !startResult)
                    {
                        StreamLog.Warn(identifier, "Starting pipeline failed");
                        return null;
                    }

                    // get the final stream and return it
                    Stream finalStream = Streams[identifier].Context.Pipeline.GetFinalStream();
                    if (finalStream != null)
                    {
                        Streams[identifier].OutputStream = new ReadTrackingStreamWrapper(finalStream);
                    }

                    StreamLog.Info(identifier, "Started stream");
                    Streams[identifier].LastActivity = DateTime.Now;
                    return stream.Transcoder.GetStreamURL();
                }
            }
            catch (Exception ex)
            {
                StreamLog.Error(identifier, "Failed to start stream", ex);
                return null;
            }
        }

        public Stream RetrieveStream(string identifier)
        {
            if (!Streams.ContainsKey(identifier))
            {
                Log.Warn("Client called RetrieveStream() for non-existing identifier", identifier);
                WCFUtil.SetResponseCode(HttpStatusCode.NotFound);
                return Stream.Null;
            }

            lock (Streams[identifier])
            {
                WCFUtil.SetContentType(Streams[identifier].Context.Profile.MIME);
                if (Streams[identifier].OutputStream == null)
                {
                    StreamLog.Warn(identifier, "Encountered null stream in RetrieveStream");
                    WCFUtil.SetResponseCode(HttpStatusCode.NotFound);
                    return Stream.Null;
                }

                WCFUtil.SetContentType(Streams[identifier].Context.Profile.MIME);

                Streams[identifier].LastActivity = DateTime.Now;
                if (Streams[identifier].Transcoder is IRetrieveHookTranscoder)
                {
                    (Streams[identifier].Transcoder as IRetrieveHookTranscoder).RetrieveStreamCalled();
                }

                return Streams[identifier].OutputStream;
            }
        }

        public Stream CustomTranscoderData(string identifier, string action, string parameters)
        {
            if (!Streams.ContainsKey(identifier))
            {
                Log.Warn("Client called CustomTranscoderData() for non-existing identifier {0}", identifier);
                WCFUtil.SetResponseCode(HttpStatusCode.NotFound);
                return Stream.Null;
            }

            lock (Streams[identifier])
            {
                if (!(Streams[identifier].Transcoder is ICustomActionTranscoder))
                    return Stream.Null;

                Streams[identifier].LastActivity = DateTime.Now;
                return ((ICustomActionTranscoder)Streams[identifier].Transcoder).CustomActionData(action, parameters);
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
                    StreamLog.Debug(identifier, "Stopping stream");
                    sharing.EndStream(Streams[identifier].Context.Source);
                    Streams[identifier].Context.Pipeline.Stop();
                    Streams[identifier].Context.Pipeline = null;
                }
            }
            catch (Exception ex)
            {
                StreamLog.Error(identifier, "Failed to stop stream", ex);
            }
        }

        public void KillStream(string identifier)
        {
            EndStream(identifier);
            Streams.Remove(identifier);
            StreamLog.Debug(identifier, "Killed stream");
        }

        public List<WebStreamingSession> GetStreamingSessions()
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
                PercentageProgress = s.Context.MediaInfo == null ? 0 :
                                        (int)Math.Round(100.0 * s.Context.GetPlayerPosition() / s.Context.MediaInfo.Duration),
                TranscodingInfo = s.Context.TranscodingInfo != null ? s.Context.TranscodingInfo : null,
            }).ToList();
        }

        public void SetPlayerPosition(string identifier, long playerPosition)
        {
            if (Streams.ContainsKey(identifier) && Streams[identifier] != null)
            {
                Streams[identifier].Context.SyncedPlayerPosition = playerPosition;
                Streams[identifier].Context.LastPlayerPositionSync = DateTime.Now;
            }
        }

        public WebTranscodingInfo GetEncodingInfo(string identifier)
        {
            Streams[identifier].LastActivity = DateTime.Now;
            if (Streams.ContainsKey(identifier) && Streams[identifier] != null)
                return Streams[identifier].Context.TranscodingInfo;

            Log.Warn("Requested transcoding info for unknown identifier {0}", identifier);
            return null;
        }

        public Resolution CalculateSize(TranscoderProfile profile, decimal displayAspectRatio)
        {
            return Resolution.Calculate(displayAspectRatio, profile.MaxOutputWidth, profile.MaxOutputHeight, 2);
        }

        public Resolution CalculateSize(StreamContext context)
        {
            return CalculateSize(context.Profile, context.Source, context.MediaInfo);
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
                    var res = Resolution.Calculate(info.VideoStreams.First().DisplayAspectRatio, profile.MaxOutputWidth, profile.MaxOutputHeight, 2);
                    if (res.Width == 0 && res.Height == 0)
                        return new Resolution(info.VideoStreams.First().Width, info.VideoStreams.First().Height);
                    return res;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to calculate size of output stream", ex);
            }

            // default
            return Resolution.Calculate(MediaInfoHelper.DEFAULT_ASPECT_RATIO, profile.MaxOutputWidth, profile.MaxOutputHeight, 2);
        }
    }
}
