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
using System.Security.Cryptography;
using System.Text;
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.StreamingService.Code;

namespace MPExtended.Services.StreamingService.Trakt
{
    internal class TraktBridge : IWatchSharingService
    {
        private IMediaAccessService service;
        private string username;
        private string password;

        public TraktBridge(IMediaAccessService service, Dictionary<string, string> config)
        {
            this.service = service;
            this.username = config["username"];
            this.password = config["password"];
        }

        public int UpdateInterval
        {
            get
            {
                return 15;
            }
        }

        private string GetPasswordHash()
        {
            // TODO: save password encrypted instead of in plaintext
            byte[] data = Encoding.ASCII.GetBytes(password);
            byte[] hash = SHA1Managed.Create().ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        public bool StartWatchingMovie(WebMovieDetailed movie)
        {
            return true;
        }

        public bool WatchingMovie(WebMovieDetailed movie, int progress)
        {
            return CallMovieAPI(movie, TraktScrobbleState.watching, progress);
        }

        public bool FinishMovie(WebMovieDetailed movie)
        {
            return CallMovieAPI(movie, TraktScrobbleState.scrobble, 100);
        }

        private bool CallMovieAPI(WebMovieBasic movie, TraktScrobbleState state, int progress) 
        {
            TraktMovieScrobble data = new TraktMovieScrobble()
            {
                MediaCenter = TraktConfig.MediaCenter,
                MediaCenterBuildDate = TraktConfig.MediaCenterDate,
                MediaCenterVersion = TraktConfig.MediaCenterVersion,
                PluginVersion = TraktConfig.PluginVersion,
                Password = GetPasswordHash(),
                UserName = username,

                Duration = movie.Runtime.ToString(),
                IMDBID = movie.IMDBId,
                Progress = progress.ToString(),
                Title = movie.Title,
                TMDBID = movie.TMDBId,
                Year = movie.Year.ToString()
            };

            TraktResponse response = TraktAPI.ScrobbleMovie(data, state);

            if (response.Status != "success")
            {
                Log.Warn("Trakt: failed to update watch status of movie '{0}' ({1}): {2}", movie.Title, movie.Id, response.Error);
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool CancelWatchingMovie(WebMovieDetailed movie)
        {
            TraktResponse response = TraktAPI.ScrobbleMovie(null, TraktScrobbleState.cancelwatching);
            if (response.Status != "success")
            {
                Log.Warn("Trakt: failed to cancel watching movie: {0}", response.Error);
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool StartWatchingEpisode(WebTVEpisodeDetailed episode)
        {
            return true;
        }

        public bool WatchingEpisode(WebTVEpisodeDetailed episode, int progress)
        {
            return CallShowAPI(episode, TraktScrobbleState.watching, progress);
        }

        public bool FinishEpisode(WebTVEpisodeDetailed episode)
        {
            return CallShowAPI(episode, TraktScrobbleState.scrobble, 100);
        }

        private bool CallShowAPI(WebTVEpisodeBasic episode, TraktScrobbleState state, int progress)
        {
            WebTVShowDetailed show = service.GetTVShowDetailedById(episode.ShowId);
            WebTVSeasonDetailed season = service.GetTVSeasonDetailedById(episode.SeasonId);

            TraktEpisodeScrobble data = new TraktEpisodeScrobble()
            {
                MediaCenter = TraktConfig.MediaCenter,
                MediaCenterBuildDate = TraktConfig.MediaCenterDate,
                MediaCenterVersion = TraktConfig.MediaCenterVersion,
                PluginVersion = TraktConfig.PluginVersion,
                Password = GetPasswordHash(),
                UserName = username,

                Duration = show.Runtime.ToString(),
                Episode = episode.EpisodeNumber.ToString(),
                IMDBID = show.IMDBId,
                Progress = progress.ToString(),
                Season = season.SeasonNumber.ToString(),
                Title = show.Title,
                TVDBID = show.TVDBId,
                Year = show.Year.ToString(),
            };

            TraktResponse response = TraktAPI.ScrobbleEpisodeState(data, state);

            if (response.Status != "success")
            {
                Log.Warn("Trakt: failed to update watch status of episode '{0}' ({1}): {2}", episode.Title, episode.Id, response.Error);
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool CancelWatchingEpisode(WebTVEpisodeDetailed episode)
        {
            TraktResponse response = TraktAPI.ScrobbleEpisodeState(null, TraktScrobbleState.cancelwatching);
            if (response.Status != "success")
            {
                Log.Warn("Trakt: failed to cancel watching episode: {0}", response.Error);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
