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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Social.Trakt;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.Libraries.Social
{
    public class TraktSharingProvider : IWatchSharingService
    {
        public Dictionary<string,string> Configuration { get; set; }

        public int UpdateInterval
        {
            get
            {
                return 15;
            }
        }

        public string HashPassword(string password)
        {
            byte[] data = Encoding.ASCII.GetBytes(password);
            byte[] hash = SHA1Managed.Create().ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        public bool TestCredentials(string username, string password)
        {
            var data = new TraktAccountTestData()
            {
                UserName = username,
                Password = HashPassword(password)
            };
            var resp = TraktAPI.TestAccount(data);
            return resp.Status == "success";
        }

        public bool StartWatchingMovie(WebMovieDetailed movie)
        {
            return CallMovieAPI(movie, TraktWatchStatus.Watching, 0);
        }

        public bool WatchingMovie(WebMovieDetailed movie, int progress)
        {
            return CallMovieAPI(movie, TraktWatchStatus.Watching, progress);
        }

        public bool FinishMovie(WebMovieDetailed movie)
        {
            return CallMovieAPI(movie, TraktWatchStatus.Scrobble, 100);
        }

        public bool CancelWatchingMovie(WebMovieDetailed movie)
        {
            return CallMovieAPI(movie, TraktWatchStatus.CancelWatching, null);
        }

        private bool CallMovieAPI(WebMovieBasic movie, TraktWatchStatus state, int? progress) 
        {
            var data = new TraktMovieScrobbleData()
            {
                MediaCenter = TraktConfig.MediaCenter,
                MediaCenterBuildDate = TraktConfig.MediaCenterDate,
                MediaCenterVersion = TraktConfig.MediaCenterVersion,
                PluginVersion = TraktConfig.PluginVersion,
                Password = Configuration["passwordHash"],
                UserName = Configuration["username"],

                Duration = movie.Runtime.ToString(),
                Title = movie.Title,
                Year = movie.Year.ToString()
            };

            if (progress != null)
                data.Progress = progress.Value.ToString();
			
			if (movie.ExternalId.Count(x => x.Site == "IMDB") > 0)
                data.IMDBID = movie.ExternalId.First(x => x.Site == "IMDB").Id;
            if (movie.ExternalId.Count(x => x.Site == "TMDB") > 0)
                data.TMDBID = movie.ExternalId.First(x => x.Site == "TMDB").Id;
            if (data.IMDBID == null && data.TMDBID == null)
            {
                Log.Info("Trakt: IMDB and TMDB unknown of movie {0}, not sending", movie.Title);
                return false;
            }

            try
            {
                Log.Debug("Trakt: calling service for movie {0} with progress {1} and state {2}", data.Title, data.Progress, state);
                TraktResponse response = TraktAPI.ScrobbleMovie(data, state);
                if (response.Status != "success")
                {
                    Log.Warn("Trakt: failed to update watch status of movie '{0}' ({1}): {2}", movie.Title, movie.Id, response.Error);
                    return false;
                }
                Log.Trace("Trakt: finished service call");
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn("Trakt: failed to call service", ex);
                return false;
            }
        }

        public bool StartWatchingEpisode(WebTVShowDetailed show, WebTVSeasonDetailed season, WebTVEpisodeDetailed episode)
        {
            return CallShowAPI(show, season, episode, TraktWatchStatus.Watching, 0);
        }

        public bool WatchingEpisode(WebTVShowDetailed show, WebTVSeasonDetailed season, WebTVEpisodeDetailed episode, int progress)
        {
            return CallShowAPI(show, season, episode, TraktWatchStatus.Watching, progress);
        }

        public bool FinishEpisode(WebTVShowDetailed show, WebTVSeasonDetailed season, WebTVEpisodeDetailed episode)
        {
            return CallShowAPI(show, season, episode, TraktWatchStatus.Scrobble, 100);
        }

        public bool CancelWatchingEpisode(WebTVShowDetailed show, WebTVSeasonDetailed season, WebTVEpisodeDetailed episode)
        {
            return CallShowAPI(show, season, episode, TraktWatchStatus.CancelWatching, null);
        }

        private bool CallShowAPI(WebTVShowDetailed show, WebTVSeasonDetailed season, WebTVEpisodeBasic episode, TraktWatchStatus state, int? progress)
        {
            var data = new TraktEpisodeScrobbleData()
            {
                MediaCenter = TraktConfig.MediaCenter,
                MediaCenterBuildDate = TraktConfig.MediaCenterDate,
                MediaCenterVersion = TraktConfig.MediaCenterVersion,
                PluginVersion = TraktConfig.PluginVersion,
                Password = Configuration["passwordHash"],
                UserName = Configuration["username"],

                Duration = show.Runtime.ToString(),
                Episode = episode.EpisodeNumber.ToString(),
                Season = season.SeasonNumber.ToString(),
                Title = show.Title,
                Year = show.Year.ToString(),
            };

            if (progress != null)
                data.Progress = progress.Value.ToString();
			
			if (show.ExternalId.Count(x => x.Site == "IMDB") > 0)
                data.IMDBID = show.ExternalId.First(x => x.Site == "IMDB").Id;
            if (show.ExternalId.Count(x => x.Site == "TVDB") > 0)
                data.TVDBID = show.ExternalId.First(x => x.Site == "TVDB").Id;
            if (data.IMDBID == null && data.TVDBID == null)
            {
                Log.Info("Trakt: IMDB and TVDB unknown of episode {0}, not sending", episode.Title);
                return false;
            }

            try
            {
                Log.Debug("Trakt: calling service for show {0} (episode {1}) with progress {2} and state {3}", data.Title, episode.Title, data.Progress, state.ToString());
                TraktResponse response = TraktAPI.ScrobbleEpisode(data, state);
                if (response.Status != "success")
                {
                    Log.Warn("Trakt: failed to update watch status of episode '{0}' ({1}): {2}", episode.Title, episode.Id, response.Error);
                    return false;
                }
                Log.Trace("Trakt: finished service call");
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn("Trakt: failed to call service", ex);
                return false;
            }
        }
    }
}