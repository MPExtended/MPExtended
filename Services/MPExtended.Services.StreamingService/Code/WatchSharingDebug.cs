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
using MPExtended.Libraries.General;
using MPExtended.Libraries.Social;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.Services.StreamingService.Code
{
    internal class WatchSharingDebug : IWatchSharingService
    {
        public int UpdateInterval
        {
            get
            {
                return 1;
            }
        }

        public IMediaAccessService MediaService { get; set; }
        public Dictionary<string, string> Configuration { get; set; }

        public bool StartWatchingMovie(WebMovieDetailed movie)
        {
            Log.Debug("WSD: Start watching movie {0}", movie.Title);
            return true;
        }

        public bool WatchingMovie(WebMovieDetailed movie, int progress)
        {
            Log.Debug("WSD: Watching movie {0} ({1}%)", movie.Title, progress);
            return true;
        }

        public bool FinishMovie(WebMovieDetailed movie)
        {
            Log.Debug("WSD: Finished movie {0}", movie.Title);
            return true;
        }

        public bool CancelWatchingMovie(WebMovieDetailed movie)
        {
            Log.Debug("WSD: Canceled movie {0}", movie.Title);
            return true;
        }

        public bool StartWatchingEpisode(WebTVEpisodeDetailed episode)
        {
            Log.Debug("WSD: Start watching episode {0}, season {1}, show {2}", episode.Title, episode.SeasonId, episode.ShowId);
            return true;
        }

        public bool WatchingEpisode(WebTVEpisodeDetailed episode, int progress)
        {
            Log.Debug("WSD: Watching episode {0} ({1}%)", episode.Title, progress);
            return true;
        }

        public bool FinishEpisode(WebTVEpisodeDetailed episode)
        {
            Log.Debug("WSD: Finished episode {0}", episode.Title);
            return true;
        }

        public bool CancelWatchingEpisode(WebTVEpisodeDetailed episode)
        {
            Log.Debug("WSD: Canceled episode {0}", episode.Title);
            return true;
        }

        // not needed as this class won't be called from the configurator
        public bool TestCredentials(string username, string password)
        {
            throw new NotImplementedException();
        }

        public string HashPassword(string password)
        {
            throw new NotImplementedException();
        }
    }
}
