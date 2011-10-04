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
using MPExtended.Services.MediaAccessService.Trakt;

namespace MPExtended.Services.MediaAccessService
{
    internal class TraktBridge
    {
        private IMediaAccessService service;
        private string username;
        private string password;

        public TraktBridge(IMediaAccessService service, string username, string password)
        {
            this.service = service;
            this.username = username;
            this.password = password;
        }

        private string GetPasswordHash()
        {
            // TODO: save password encrypted instead of in plaintext
            byte[] data = Encoding.ASCII.GetBytes(password);
            byte[] hash = SHA1Managed.Create().ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        public bool UpdateMovie(WebMovieBasic movie, int minutesWatched)
        {
            return CallMovieAPI(movie, TraktScrobbleState.watching, Math.Round(minutesWatched * 1.0 / movie.Runtime * 100).ToString());
        }

        public bool FinishMovie(WebMovieBasic movie)
        {
            return CallMovieAPI(movie, TraktScrobbleState.scrobble, "100");
        }

        private bool CallMovieAPI(WebMovieBasic movie, TraktScrobbleState state, string progress) 
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
                Progress = progress,
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

        public bool CancelMovie()
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

        public bool UpdateEpisode(WebTVEpisodeBasic episode, int minutesWatched)
        {
            return CallShowAPI(episode, TraktScrobbleState.watching, minutesWatched);
        }

        public bool FinishEpisode(WebTVEpisodeBasic episode)
        {
            return CallShowAPI(episode, TraktScrobbleState.scrobble, 0);
        }

        private bool CallShowAPI(WebTVEpisodeBasic episode, TraktScrobbleState state, int minutesWatched)
        {
            WebTVShowDetailed show = service.GetTVShowDetailedById(episode.ShowId);
            WebTVSeasonDetailed season = service.GetTVSeasonDetailedById(episode.SeasonId);

            string progress = state == TraktScrobbleState.watching ?
                Math.Round(minutesWatched * 1.0 / show.Runtime * 100).ToString() :
                "100";

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
                Progress = progress,
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

        public bool CancelEpisode()
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
