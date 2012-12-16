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
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Timers;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.Social;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal class WatchSharing : IDisposable
    {
        // you can change the timings here to make debugging less boring
#if false
        private const int CYCLE_TIME = 5000;
        private const int KEEP_ONLINE_ALIVE_CYCLES = 6; // number of cycles to wait before synchronizing cancelwatching events
#else
        private const int CYCLE_TIME = 60000;
        private const int KEEP_ONLINE_ALIVE_CYCLES = 5;
#endif

        private class WatchSharingTimer : Timer
        {
            public string Identifier { get; set; }
            public IWatchSharingService Service { get; set; }
            public int Iteration { get; set; }
            public int CanceledWaitIterations { get; set; }

            public WatchSharingTimer(string identifier, IWatchSharingService service)
                : base()
            {
                AutoReset = true;
                Interval = CYCLE_TIME;

                Identifier = identifier;
                Service = service;
                Iteration = 0;
                CanceledWaitIterations = -1;
            }
        }

        private class StreamState
        {
            public string Id { get; set; }
            public object MediaDescriptor { get; set; } // WebMovieDetailed or WebTVEpisodeDetailed
            public StreamContext Context { get; set; }
            public Reference<WebTranscodingInfo> TranscodingInfo { get; set; }
            public int Runtime { get; set; } // in milliseconds
            public bool Canceled { get; set; }
            public bool Stale { get; set; }
            public List<WatchSharingTimer> BackgroundTimers { get; set; }
        }

        private class WatchSharingServiceList : List<IWatchSharingService>
        {
            public void ExecuteForAll(Action<IWatchSharingService> action)
            {
                foreach (IWatchSharingService service in this)
                {
                    lock (service)
                    {
                        action(service);
                    }
                }
            }
        }

        private bool enabled;
        private WatchSharingServiceList services;
        private Dictionary<string, StreamState> streams = new Dictionary<string, StreamState>();

        public WatchSharing()
        {
            services = new WatchSharingServiceList();
            if (Configuration.Streaming.WatchSharing.DebugEnabled)
                services.Add(new WatchSharingDebug());
            if (Configuration.Streaming.WatchSharing.FollwitEnabled)
                services.Add(new FollwitSharingProvider() { Configuration = Configuration.Streaming.WatchSharing.FollwitConfiguration });
            if (Configuration.Streaming.WatchSharing.TraktEnabled)
                services.Add(new TraktSharingProvider() { Configuration = Configuration.Streaming.WatchSharing.TraktConfiguration });
            enabled = services.Any();
        }

        public void Dispose()
        {
            // cancel all streams
            foreach (var kvp in streams)
            {
                EndStream(kvp.Value.Context.Source, true);
            }
        }

        public void StartStream(StreamContext context, Reference<WebTranscodingInfo> infoRef)
        {
            // ignore when not needed
            if (!enabled || (context.Source.MediaType != WebMediaType.Movie && context.Source.MediaType != WebMediaType.TVEpisode))
            {
                return;
            }

            // do some cleanup
            foreach (string id in streams.Where(x => x.Value.Stale).Select(x => x.Value.Id).ToList())
            {
                streams.Remove(id);
            }

            // generate identifier
            string identifier = GetIdentifierFromMediaSource(context.Source);

            // start if non-existent            
            if (!streams.ContainsKey(identifier))
            {
                StreamState state = new StreamState()
                {
                    Id = identifier,
                    Context = context,
                    TranscodingInfo = infoRef,
                    Canceled = false,
                    Stale = false
                };

                // get mediadescriptor and rough runtime
                Log.Debug("WatchSharing: synchronizing start watching event to service");
                if (context.Source.MediaType == WebMediaType.TVEpisode)
                {
                    state.MediaDescriptor = Connections.MAS.GetTVEpisodeDetailedById(context.Source.Provider, context.Source.Id);
                    state.Runtime = Connections.MAS.GetTVShowDetailedById(context.Source.Provider, ((WebTVEpisodeDetailed)state.MediaDescriptor).ShowId).Runtime * 60000;
                }
                else if (context.Source.MediaType == WebMediaType.Movie)
                {
                    state.MediaDescriptor = Connections.MAS.GetMovieDetailedById(context.Source.Provider, context.Source.Id);
                    state.Runtime = ((WebMovieDetailed)state.MediaDescriptor).Runtime * 60000;
                }

                // get exact runtime if available
                if (context.MediaInfo.Duration > 60)
                {
                    state.Runtime = (int)context.MediaInfo.Duration;
                }

                // send start watching event
                Task.Factory.StartNew(delegate()
                {
                    if (state.Context.Source.MediaType == WebMediaType.TVEpisode)
                    {
                        services.ExecuteForAll(s => CallForEpisode(identifier, s.StartWatchingEpisode));
                    }
                    else if (state.Context.Source.MediaType == WebMediaType.Movie)
                    {
                        services.ExecuteForAll(s => s.StartWatchingMovie((WebMovieDetailed)state.MediaDescriptor));
                    }
                });

                // and start the background timer
                streams[identifier] = state;
                state.BackgroundTimers = new List<WatchSharingTimer>();
                foreach (IWatchSharingService service in services)
                {
                    var thisTimer = new WatchSharingTimer(identifier, service);
                    thisTimer.Elapsed += TimerElapsed;
                    thisTimer.Start();
                    state.BackgroundTimers.Add(thisTimer);
                }
            }
            else
            {
                // just update the progress which will be send next time
                Log.Info("WatchSharing: Picking up old stream");
                streams[identifier].Canceled = false;
                streams[identifier].Context = context;
            }
        }

        public void EndStream(MediaSource source, bool force = false)
        {
            // generate identifier
            string identifier = GetIdentifierFromMediaSource(source);

            // ignore if not registered or already stale
            if (!enabled || !streams.ContainsKey(identifier) || streams[identifier].Stale)
            {
                return;
            }

            if (streams[identifier].TranscodingInfo != null && streams[identifier].TranscodingInfo.Value != null)
            {
                int progress = CalculateWatchPosition(identifier);
                if (progress >= 95)
                {
                    Log.Debug("WatchSharing: seeing {0}% as finished for {1}", progress, identifier);

                    // send the finished event in the background thread
                    Task.Factory.StartNew(delegate ()
                    {
                        // Stop the timers. Do this before sending the FinishEpisode() events as we could get a race condition with the service otherwise.
                        foreach (var timer in streams[identifier].BackgroundTimers)
                        {
                            timer.Enabled = false;
                        }

                        // Send the FinishEpisode event
                        if (streams[identifier].Context.Source.MediaType == WebMediaType.TVEpisode)
                        {
                            services.ExecuteForAll(s => CallForEpisode(identifier, s.FinishEpisode));
                        }
                        else if (streams[identifier].Context.Source.MediaType == WebMediaType.Movie)
                        {
                            services.ExecuteForAll(s => s.FinishMovie((WebMovieDetailed)streams[identifier].MediaDescriptor));
                        }

                        // And definitely stop the stream
                        streams.Remove(identifier);
                        Log.Debug("WatchSharing: finished handling {0}", identifier);
                    });
                    return;
                }
            }

            // cancel it
            if (!force)
            {
                Log.Debug("WatchSharing: canceling stream {0}", identifier);
                streams[identifier].Canceled = true;
            }
            else
            {
                // definitely cancel it
                Log.Debug("WatchSharing: killing stream {0} because of forced EndStream", identifier);
                if (streams[identifier].Context.Source.MediaType == WebMediaType.TVEpisode)
                {
                    services.ExecuteForAll(s => CallForEpisode(identifier, s.CancelWatchingEpisode));
                }
                else if (streams[identifier].Context.Source.MediaType == WebMediaType.Movie)
                {
                    services.ExecuteForAll(s => s.CancelWatchingMovie((WebMovieDetailed)streams[identifier].MediaDescriptor));
                }
            }
        }

        private void TimerElapsed(object source, ElapsedEventArgs args)
        {
            try
            {
                WatchSharingTimer wst = (WatchSharingTimer)source;

                // check if canceled
                if (streams[wst.Identifier].Canceled)
                {
                    Log.Debug("WatchSharing: stream {0} is canceled", streams[wst.Identifier].Id);
                    wst.CanceledWaitIterations = wst.CanceledWaitIterations == -1 ? KEEP_ONLINE_ALIVE_CYCLES : wst.CanceledWaitIterations - 1;

                    // user definitely canceled
                    if (wst.CanceledWaitIterations == 0)
                    {
                        // notify service
                        Log.Debug("WatchSharing: definitely killing stream {0} with cancelwatching event", streams[wst.Identifier].Id);
                        lock (wst.Service)
                        {
                            if (streams[wst.Identifier].Context.Source.MediaType == WebMediaType.TVEpisode)
                            {
                                CallForEpisode(wst.Identifier, wst.Service.CancelWatchingEpisode);
                            }
                            else if (streams[wst.Identifier].Context.Source.MediaType == WebMediaType.Movie)
                            {
                                wst.Service.CancelWatchingMovie((WebMovieDetailed)streams[wst.Identifier].MediaDescriptor);
                            }
                        }

                        // and kill us
                        streams[wst.Identifier].Stale = true;
                        wst.Enabled = false;
                        return;
                    }
                }
                else
                {
                    wst.CanceledWaitIterations = -1;
                }

                // only send every x iterations the status, when we know for sure that we have a value and stream isn't canceled yet
                if (streams[wst.Identifier].Canceled)
                {
                    Log.Trace("WatchSharing: stream canceled");
                    // try again next iteration (++iteration is NOT called this time so that we don't create a gap when the stream crashed)
                } 
                else if (++wst.Iteration % wst.Service.UpdateInterval == 0)
                {
                    // send position
                    Log.Debug("WatchSharing: syncing status for {0}", streams[wst.Identifier].Id);
                    lock (wst.Service)
                    {
                        if (streams[wst.Identifier].Context.Source.MediaType == WebMediaType.TVEpisode)
                        {
                            CallForEpisode(wst.Identifier, (show, season, episode) => wst.Service.WatchingEpisode(show, season, episode, CalculateWatchPosition(wst.Identifier)));
                        }
                        else if (streams[wst.Identifier].Context.Source.MediaType == WebMediaType.Movie)
                        {
                            wst.Service.WatchingMovie((WebMovieDetailed)streams[wst.Identifier].MediaDescriptor, CalculateWatchPosition(wst.Identifier));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn("Failed in watch sharing", e);
            }
        }

        private int CalculateWatchPosition(string id)
        {
            long watchPosition = streams[id].Context.GetPlayerPosition();
            int progress = (int)Math.Round((watchPosition * 1.0 / streams[id].Runtime) * 100);
            Log.Debug("WatchSharing: watch position {0}ms, runtime {1}ms, progress {2}%", watchPosition, streams[id].Runtime, progress);
            return progress;
        }

        private string GetIdentifierFromMediaSource(MediaSource source)
        {
            return Enum.GetName(typeof(WebMediaType), source.MediaType) + "_" + source.Id;
        }

        private TReturn CallForEpisode<TReturn>(string identifier, Func<WebTVShowDetailed, WebTVSeasonDetailed, WebTVEpisodeDetailed, TReturn> action)
        {
            var episode = (WebTVEpisodeDetailed)streams[identifier].MediaDescriptor;
            var show = Connections.MAS.GetTVShowDetailedById(episode.PID, episode.ShowId);
            var season = Connections.MAS.GetTVSeasonDetailedById(episode.PID, episode.SeasonId);
            return action.Invoke(show, season, episode);
        }
    }
}
