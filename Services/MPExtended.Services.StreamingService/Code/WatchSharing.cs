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
using System.Xml.Linq;
using System.Threading;
using MPExtended.Libraries.General;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Libraries.Social;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.Services.StreamingService.Code
{
    internal class WatchSharing
    {
        private const int KEEP_ONLINE_ALIVE = 0; // synchronized cancelwatching events only after a delay between x and x+1 minutes (to support restarting the stream)

        private class StreamState
        {
            public string Id { get; set; }
            public object MediaDescriptor { get; set; } // WebMovieDetailed or WebTVEpisodeDetailed
            public MediaSource Source { get; set; }
            public bool Canceled { get; set; }
            public bool Stale { get; set; }
            public int StartedPosition { get; set; } // in seconds
            public long Runtime { get; set; } // in seconds
            public Thread BackgroundThread { get; set; }
            public Reference<WebTranscodingInfo> TranscodingInfo { get; set; }
        }

        private bool enabled;
        private IWatchSharingService service;
        private Dictionary<string, StreamState> streams = new Dictionary<string, StreamState>();

        public WatchSharing()
        {
            switch (Configuration.Streaming.WatchSharing["type"])
            {
                case "trakt":
                    service = new TraktSharingProvider();
                    break;
                case "follwit":
                    service = new FollwitSharingProvider();
                    break;
                case "debug":
                    service = new WatchSharingDebug();
                    break;
                case "none": // no reason that's explicitely listed here
                default:
                    enabled = false;
                    return;
            }

            enabled = true;
            service.MediaService = MPEServices.MAS;
            service.Configuration = Configuration.Streaming.WatchSharing;
        }

        public void StartStream(StreamContext context, Reference<WebTranscodingInfo> infoRef, int position)
        {
            // ignore when not needed
            if (!enabled || (context.Source.MediaType != WebStreamMediaType.Movie && context.Source.MediaType != WebStreamMediaType.TVEpisode))
            {
                return;
            }

            // do some cleanup
            foreach (string id in streams.Where(x => x.Value.Stale).Select(x => x.Value.Id).ToList())
            {
                streams.Remove(id);
            }

            // generate identifier
            string identifier = Enum.GetName(typeof(WebStreamMediaType), context.Source.MediaType) + "_" + context.Source.Id;

            // start if non-existent            
            if (!streams.ContainsKey(identifier))
            {
                StreamState state = new StreamState()
                {
                    Id = identifier,
                    Source = context.Source,
                    TranscodingInfo = infoRef,
                    StartedPosition = position,
                    Canceled = false,
                    Stale = false
                };

                // get mediadescriptor and rough runtime, and start the watching online
                Log.Debug("WatchSharing: synchronizing start watching event to service");
                if (context.Source.MediaType == WebStreamMediaType.TVEpisode)
                {
                    state.MediaDescriptor = MPEServices.MAS.GetTVEpisodeDetailedById(context.Source.Provider, context.Source.Id);
                    state.Runtime = MPEServices.MAS.GetTVShowDetailedById(context.Source.Provider, ((WebTVEpisodeDetailed)state.MediaDescriptor).ShowId).Runtime * 60;
                }
                else if (context.Source.MediaType == WebStreamMediaType.Movie)
                {
                    state.MediaDescriptor = MPEServices.MAS.GetMovieDetailedById(context.Source.Provider, context.Source.Id);
                    state.Runtime = ((WebMovieDetailed)state.MediaDescriptor).Runtime * 60;
                }

                // get exact runtime if available
                if (context.MediaInfo.Duration > 60)
                {
                    state.Runtime = context.MediaInfo.Duration / 1000;
                }

                streams[identifier] = state;
                state.BackgroundThread = ThreadManager.Start("WatchWorker", new ParameterizedThreadStart(this.BackgroundWorker), identifier);
            }
            else
            {
                // just update the progress which will be send next time
                Log.Info("WatchSharing: Picking up old stream");
                streams[identifier].StartedPosition = position;
                streams[identifier].Canceled = false;
            }
        }

        public void EndStream(MediaSource source)
        {
            // generate identifier
            string identifier = Enum.GetName(typeof(WebStreamMediaType), source.MediaType) + "_" + source.Id;

            // ignore if not registered
            if (!enabled || !streams.ContainsKey(identifier))
            {
                return;
            }

            if (streams[identifier].TranscodingInfo != null && streams[identifier].TranscodingInfo.Value != null)
            {
                int progress = CalculateWatchPosition(identifier);
                if (progress >= 95)
                {
                    Log.Debug("WatchSharing: seeing {0}% as finished for {1}", progress, identifier);

                    // send the finished event in a background thread
                    ThreadManager.Start("FinishWatching", delegate()
                    {
                        if (streams[identifier].Source.MediaType == WebStreamMediaType.TVEpisode)
                        {
                            service.FinishEpisode((WebTVEpisodeDetailed)streams[identifier].MediaDescriptor);
                        }
                        else if (streams[identifier].Source.MediaType == WebStreamMediaType.Movie)
                        {
                            service.FinishMovie((WebMovieDetailed)streams[identifier].MediaDescriptor);
                        }

                        // kill it
                        streams[identifier].BackgroundThread.Abort();
                        ThreadManager.Remove(streams[identifier].BackgroundThread);
                        streams.Remove(identifier);
                        Log.Debug("WatchSharing: finished handling {0}", identifier);
                    });
                    return;
                }
            }

            // cancel it
            Log.Debug("WatchSharing: canceling stream {0}", identifier);
            streams[identifier].Canceled = true;
        }

        private void BackgroundWorker(object identifier)
        {
            string id = (string)identifier;
            int iteration = 0;
            int canceledWaitIterations = -1;

            // start by sending the start watching event (which we do here so that it doesn't block starting the stream)
            if (streams[id].Source.MediaType == WebStreamMediaType.TVEpisode)
            {
                service.StartWatchingEpisode((WebTVEpisodeDetailed)streams[id].MediaDescriptor);
            }
            else if (streams[id].Source.MediaType == WebStreamMediaType.Movie)
            {
                service.StartWatchingMovie((WebMovieDetailed)streams[id].MediaDescriptor);
            }

            // and then we don't have to do anything immediately
            Thread.Sleep(60000);

            while (true)
            {
                try
                {
                    // check if canceled
                    if (streams[id].Canceled)
                    {
                        Log.Debug("WatchSharing: stream {0} is canceled", streams[id].Id);
                        canceledWaitIterations = canceledWaitIterations == -1 ? KEEP_ONLINE_ALIVE : canceledWaitIterations - 1;

                        // user definitely canceled
                        if (canceledWaitIterations == 0)
                        {
                            // notify service
                            Log.Debug("WatchSharing: definitely killing stream {0} with cancelwatching event", streams[id].Id);
                            lock (service)
                            {
                                if (streams[id].Source.MediaType == WebStreamMediaType.TVEpisode)
                                {
                                    service.CancelWatchingEpisode((WebTVEpisodeDetailed)streams[id].MediaDescriptor);
                                }
                                else if (streams[id].Source.MediaType == WebStreamMediaType.Movie)
                                {
                                    service.CancelWatchingMovie((WebMovieDetailed)streams[id].MediaDescriptor);
                                }
                            }

                            // and kill us
                            streams[id].Stale = true;
                            return;
                        }
                    }
                    else
                    {
                        canceledWaitIterations = -1;
                    }

                    // only send every x iterations the status, when we know for sure that we have a value and stream isn't canceled yet
                    if (streams[id].Canceled)
                    {
                        Log.Trace("WatchSharing: stream canceled");
                        // try again next iteration (++iteration is NOT called this time so that we don't create a gap when the stream crashed)
                    } 
                    else if (++iteration % service.UpdateInterval == 0)
                    {
                        // send position
                        Log.Debug("WatchSharing: syncing status for {0}", streams[id].Id);
                        lock (service)
                        {
                            if (streams[id].Source.MediaType == WebStreamMediaType.TVEpisode)
                            {
                                service.WatchingEpisode((WebTVEpisodeDetailed)streams[id].MediaDescriptor, CalculateWatchPosition(id));
                            }
                            else if (streams[id].Source.MediaType == WebStreamMediaType.Movie)
                            {
                                service.WatchingMovie((WebMovieDetailed)streams[id].MediaDescriptor, CalculateWatchPosition(id));
                            }
                        }
                    }

                    // wait till next iteration
                    Thread.Sleep(60000); // run each minute
                }
                catch (ThreadAbortException) 
                {
                    // just exit
                    return;
                }
                catch (Exception e)
                {
                    Log.Warn("Failed in watch sharing", e);
                }
            }
        }

        private int CalculateWatchPosition(string id)
        {
            // calculate progress
            int transcodedValue = 0;
            if (streams[id].TranscodingInfo != null && streams[id].TranscodingInfo.Value != null)
            {
                transcodedValue = streams[id].TranscodingInfo.Value.CurrentTime;
            }
            else
            {
                Log.Info("WatchSharing: transcoded time of stream {0} not known", id);
            }
            int watchPosition = transcodedValue / 1000;
            int progress = (int)Math.Round((watchPosition * 1.0 / streams[id].Runtime) * 100);
            Log.Debug("WatchSharing: start position {0} seconds, transcoding position {1} ms, position {2} seconds, runtime {3} min, progress {4}%",
                streams[id].StartedPosition, transcodedValue, watchPosition, streams[id].Runtime, progress);

            return progress;
        }
    }
}
