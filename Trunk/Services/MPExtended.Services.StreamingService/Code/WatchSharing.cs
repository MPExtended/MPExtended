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
        private const int STATUS_INTERVAL = 15; // in minutes

        private class StreamState
        {
            public string Id { get; set; }
            public object MediaDescriptor { get; set; } // WebMovieDetailed or WebTVEpisodeDetailed
            public MediaSource Source { get; set; }
            public bool OverrideProgress { get; set; }
            public bool Canceled { get; set; }
            public bool Stale { get; set; }
            public int Progress { get; set; }
            public int Runtime { get; set; }
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

        public void StartStream(string identifier, MediaSource source, Reference<WebTranscodingInfo> infoRef, int position)
        {
            // ignore when not needed
            if (!enabled || (source.MediaType != WebStreamMediaType.Movie && source.MediaType != WebStreamMediaType.TVEpisode))
            {
                return;
            }

            // do some cleanup
            foreach (string id in streams.Where(x => x.Value.Stale).Select(x => x.Value.Id))
            {
                streams.Remove(id);
            }

            // calculate progress
            int progress = (int)Math.Round((position * 1.0 / 60) / streams[identifier].Runtime);

            // start if non-existent            
            if (!streams.ContainsKey(identifier))
            {
                StreamState state = new StreamState()
                {
                    Id = identifier,
                    Source = source,
                    TranscodingInfo = infoRef,
                    Progress = progress,
                    OverrideProgress = true,
                    Canceled = false,
                    Stale = false
                };

                if (source.MediaType == WebStreamMediaType.TVEpisode)
                {
                    state.MediaDescriptor = mas.GetTVEpisodeDetailedById(source.Id);
                    service.StartWatchingEpisode((WebTVEpisodeDetailed)state.MediaDescriptor);
                    state.Runtime = mas.GetTVShowDetailedById(((WebTVEpisodeDetailed)state.MediaDescriptor).ShowId).Runtime;
                }
                else if (source.MediaType == WebStreamMediaType.Movie)
                {
                    state.MediaDescriptor = mas.GetMovieDetailedById(source.Id);
                    service.StartWatchingMovie((WebMovieDetailed)state.MediaDescriptor);
                    state.Runtime = ((WebMovieDetailed)state.MediaDescriptor).Runtime;
                }

                state.BackgroundThread = new Thread(new ParameterizedThreadStart(this.BackgroundWorker));
                state.BackgroundThread.Start();
                streams[identifier] = state;
            }
            else
            {
                // just update the progress which will be send next time
                streams[identifier].Progress = progress;
            }
        }

        public void EndStream(string identifier)
        {
            // ignore if not registered
            if (!enabled || !streams.ContainsKey(identifier))
            {
                return;
            }

            // talk to backend
            int minutes = streams[identifier].TranscodingInfo.Value.CurrentTime / 60;
            if (minutes > streams[identifier].Runtime * 0.95)
            {
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
            }
            else
            {
                // cancel it
                streams[identifier].Canceled = true;
            }
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
                            streams[id].Stale = true;
                            return;
                        }
                    }
                    else
                    {
                        canceledWaitIterations = -1;
                    }

                    // only send every x iterations the status
                    if (iteration++ % STATUS_INTERVAL == 0)
                    {
                        // calculate progress
                        int minutes = streams[id].TranscodingInfo.Value.CurrentTime / 60;
                        int progress = (int)Math.Round(minutes * 1.0 / streams[id].Runtime);

                        // but allow to override it in the beginning
                        if (streams[id].OverrideProgress)
                        {
                            progress = streams[id].Progress;
                            streams[id].OverrideProgress = false;
                        }

                        // and send it
                        lock (service)
                        {
                            if (streams[id].Source.MediaType == WebStreamMediaType.TVEpisode)
                            {
                                service.WatchingEpisode((WebTVEpisodeDetailed)streams[id].MediaDescriptor, progress);
                            }
                            else if (streams[id].Source.MediaType == WebStreamMediaType.Movie)
                            {
                                service.WatchingMovie((WebMovieDetailed)streams[id].MediaDescriptor, progress);
                            }
                        }
                    }

                    // wait till next iteration
                    Thread.Sleep(60000);
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
    }
}
