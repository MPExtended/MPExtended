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
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.Services.StreamingService.Code
{
    public interface IWatchSharingService
    {
        int UpdateInterval { get; } // in minutes

        bool StartWatchingMovie(WebMovieDetailed movie);
        bool WatchingMovie(WebMovieDetailed movie, int progress);
        bool FinishMovie(WebMovieDetailed movie);
        bool CancelWatchingMovie(WebMovieDetailed movie);

        bool StartWatchingEpisode(WebTVEpisodeDetailed episode);
        bool WatchingEpisode(WebTVEpisodeDetailed episode, int progress);
        bool FinishEpisode(WebTVEpisodeDetailed episode);
        bool CancelWatchingEpisode(WebTVEpisodeDetailed episode);
    }

    internal class WatchSharing
    {
        private class StreamState
        {
            public string Id { get; set; }
            public object MediaDescriptor { get; set; } // WebMovieDetailed or WebTVEpisodeDetailed
            public MediaSource Source { get; set; }
            public bool Canceled { get; set; }
            public bool Stale { get; set; }
            public int StartedPosition { get; set; } // in seconds
            public int Runtime { get; set; } // in minutes
            public Thread BackgroundThread { get; set; }
            public Reference<WebTranscodingInfo> TranscodingInfo { get; set; }
        }

        private bool enabled;
        private IWatchSharingService service;
        private IMediaAccessService mas;

        private Dictionary<string, StreamState> streams = new Dictionary<string, StreamState>();

        public WatchSharing()
        {
            mas = MPEServices.NetPipeMediaAccessService;

            XElement config = XElement.Load(Configuration.GetPath("Streaming.xml")).Element("watchsharing");
            string serviceName = config.Element("type").Value;
            Dictionary<string, string> serviceConfig = new Dictionary<string, string>();
            if (config.Element(serviceName) != null)
            {
                serviceConfig = config.Element(serviceName).Elements().ToDictionary(x => x.Name.LocalName, x => x.Value);
            }

            if (serviceName == "trakt")
            {
                enabled = true;
                service = new Trakt.TraktBridge(mas, serviceConfig);
            }
            else if (serviceName == "debug")
            {
                enabled = true;
                service = new WatchSharingDebug();
            }
            else if (serviceName == "none")
            {
                enabled = false;
            }
            else
            {
                Log.Warn("Disabling watch sharing because of unknown service {0}", serviceName);
                enabled = false;
            }
        }

        public void StartStream(MediaSource source, Reference<WebTranscodingInfo> infoRef, int position)
        {
            // ignore when not needed
            if (!enabled || (source.MediaType != WebStreamMediaType.Movie && source.MediaType != WebStreamMediaType.TVEpisode))
            {
                return;
            }

            // do some cleanup
            foreach (string id in streams.Where(x => x.Value.Stale).Select(x => x.Value.Id).ToList())
            {
                streams.Remove(id);
            }

            // generate identifier
            string identifier = Enum.GetName(typeof(WebStreamMediaType), source.MediaType) + "_" + source.Id;

            // start if non-existent            
            if (!streams.ContainsKey(identifier))
            {
                StreamState state = new StreamState()
                {
                    Id = identifier,
                    Source = source,
                    TranscodingInfo = infoRef,
                    StartedPosition = position,
                    Canceled = false,
                    Stale = false
                };

                if (source.MediaType == WebStreamMediaType.TVEpisode)
                {
                    state.MediaDescriptor = mas.GetTVEpisodeDetailedById(source.Provider, source.Id);
                    service.StartWatchingEpisode((WebTVEpisodeDetailed)state.MediaDescriptor);
                    state.Runtime = mas.GetTVShowDetailedById(source.Provider, ((WebTVEpisodeDetailed)state.MediaDescriptor).ShowId).Runtime;
                }
                else if (source.MediaType == WebStreamMediaType.Movie)
                {
                    state.MediaDescriptor = mas.GetMovieDetailedById(source.Provider, source.Id);
                    service.StartWatchingMovie((WebMovieDetailed)state.MediaDescriptor);
                    state.Runtime = ((WebMovieDetailed)state.MediaDescriptor).Runtime;
                }

                streams[identifier] = state;

                state.BackgroundThread = new Thread(new ParameterizedThreadStart(this.BackgroundWorker));
                state.BackgroundThread.Name = "WatchWorker";
                state.BackgroundThread.Start(identifier);
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
                if (progress != null && progress >= 95)
                {
                    Log.Debug("WatchSharing: seeing {0}% as finished for {1}", progress, identifier);

                    // finished
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
                    streams.Remove(identifier);
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

            while (true)
            {
                try
                {
                    // check if canceled
                    if (streams[id].Canceled)
                    {
                        Log.Debug("WatchSharing: stream {0} is canceled", streams[id].Id);
                        canceledWaitIterations = canceledWaitIterations == -1 ? 3 : canceledWaitIterations - 1;

                        // user definitely canceled
                        if (canceledWaitIterations == 0)
                        {
                            // notify service
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
                            Log.Debug("WatchSharing: definitly killing stream {0}", streams[id].Id);
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
                        // try again next iteration (iteration++ is NOT called this time)
                    } 
                    else if (iteration++ % service.UpdateInterval == 0)
                    {
                        // send position
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
            if (streams[id].TranscodingInfo != null && streams[id].TranscodingInfo.Value != null && streams[id].TranscodingInfo.Value.CurrentTime != null)
            {
                transcodedValue = streams[id].TranscodingInfo.Value.CurrentTime;
            }
            else
            {
                Log.Info("WatchSharing: transcoded time of stream {0} not known", id);
            }
            int transcodedSeconds = transcodedValue / 1000 + streams[id].StartedPosition;
            int progress = CalculatePercent(transcodedSeconds / 60, streams[id].Runtime);
            Log.Debug("WatchSharing: transcoded {0} ms, position {1} seconds, runtime {2}, progress {3}%",
                transcodedValue, transcodedSeconds, streams[id].Runtime, progress);

            return progress;
        }

        private static int CalculatePercent(int currentMinutes, int runtime)
        {
            return (int)Math.Round((currentMinutes * 1.0 / runtime) * 100);
        }
    }
}
