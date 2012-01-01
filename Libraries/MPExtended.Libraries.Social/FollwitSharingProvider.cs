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
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Libraries.Social.Follwit;
using MPExtended.Libraries.General;

namespace MPExtended.Libraries.Social
{
    public class FollwitSharingProvider : IWatchSharingService
    {
        public IMediaAccessService MediaService { get; set; }
        public Dictionary<string, string> Configuration { get; set; }

        public int UpdateInterval
        {
            get 
            { 
                return 1; 
            }
        }

        public bool TestCredentials(string username, string password)
        {
            var data = new FollwitAccountTestData()
            {
                UserName = username,
                Password = FollwitAPI.GeneratePasswordHash(password)
            };
            var resp = FollwitAPI.TestAccount(data);
            return resp.Response == "success";
        }

        public string HashPassword(string password)
        {
            return FollwitAPI.GeneratePasswordHash(password);
        }

        public bool StartWatchingMovie(WebMovieDetailed movie)
        {
            return CallFollwitMovie(movie, FollwitWatchStatus.Watching);
        }

        public bool WatchingMovie(WebMovieDetailed movie, int progress)
        {
            // Follw.it doesn't require to send a status each X minutes or something
            return true;
        }

        public bool FinishMovie(WebMovieDetailed movie)
        {
            return CallFollwitMovie(movie, FollwitWatchStatus.Watched);
        }

        public bool CancelWatchingMovie(WebMovieDetailed movie)
        {
            return CallFollwitMovie(movie, FollwitWatchStatus.CancelWatching);
        }

        private bool CallFollwitMovie(WebMovieDetailed movie, FollwitWatchStatus state)
        {
            try
            {
                // create movie
                if (movie.ExternalId.Count(x => x.Site == "IMDB") == 0)
                {
                    Log.Info("Follwit: IMDB id of movie {0} unknown, not sending", movie.Title);
                    return false;
                }
            
                // and send it
                var fm = new FollwitMovie()
                {
                    Username = Configuration["username"],
                    Password = Configuration["passwordHash"],
                    IMDBId = movie.ExternalId.First(x => x.Site == "IMDB").Id
                };

                var ret = FollwitAPI.UpdateMovieState(fm, state);
                if (ret.Response != "success")
                {
                    Log.Warn("Follwit: failed to update watch status of movie '{0}' ({1})", movie.Title, movie.Id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Follwit: failed to update movie watch status", ex);
                return false;
            }
            return true;
        }

        public bool StartWatchingEpisode(WebTVEpisodeDetailed episode)
        {
            return CallFollwitEpisode(episode, FollwitWatchStatus.Watching);
        }

        public bool WatchingEpisode(WebTVEpisodeDetailed episode, int progress)
        {
            // Follw.it doesn't require to send a status each X minutes or something
            return true;
        }

        public bool FinishEpisode(WebTVEpisodeDetailed episode)
        {
            return CallFollwitEpisode(episode, FollwitWatchStatus.Watched);
        }

        public bool CancelWatchingEpisode(WebTVEpisodeDetailed episode)
        {
            return CallFollwitEpisode(episode, FollwitWatchStatus.CancelWatching);
        }

        private bool CallFollwitEpisode(WebTVEpisodeDetailed episode, FollwitWatchStatus state)
        {
            try
            {
                if (episode.ExternalId.Count(x => x.Site == "TVDB") == 0)
                {
                    Log.Info("Follwit: TVDB id of episode {0} unknown, not sending", episode.Title);
                    return false;
                }

                var fm = new FollwitEpisode()
                {
                    Username = Configuration["username"],
                    Password = Configuration["passwordHash"],
                    TVDBId = episode.ExternalId.First(x => x.Site == "TVDB").Id
                };

                var ret = FollwitAPI.UpdateEpisodeState(fm, state);
                if (ret.Response != "success")
                {
                    Log.Warn("Follwit: failed to update watch status of episode '{0}' ({1})", episode.Title, episode.Id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Follwit: failed to update episode watch status", ex);
                return false;
            }
            return true;
        }
    }
}
