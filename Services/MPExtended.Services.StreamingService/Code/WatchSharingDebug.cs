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
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.Services.StreamingService.Code
{
    internal class WatchSharingDebug : IWatchSharingService
    {
        public IMediaAccessService MAS { get; set; }

        public int UpdateInterval
        {
            get
            {
                return 1;
            }
        }

        public bool StartWatchingMovie(int provider, WebMovieDetailed movie)
        {
            Log.Debug("WSD: Start watching movie {0}", movie.Title);
            return true;
        }

        public bool WatchingMovie(int provider, WebMovieDetailed movie, int progress)
        {
            Log.Debug("WSD: Watching movie {0} ({1}%)", movie.Title, progress);
            return true;
        }

        public bool FinishMovie(int provider, WebMovieDetailed movie)
        {
            Log.Debug("WSD: Finished movie {0}", movie.Title);
            return true;
        }

        public bool CancelWatchingMovie(int provider, WebMovieDetailed movie)
        {
            Log.Debug("WSD: Canceled movie {0}", movie.Title);
            return true;
        }

        public bool StartWatchingEpisode(int provider, WebTVEpisodeDetailed episode)
        {
            Log.Debug("WSD: Start watching episode {0}, season {1}, show {2}", episode.Title, episode.SeasonId, episode.ShowId);
            return true;
        }

        public bool WatchingEpisode(int provider, WebTVEpisodeDetailed episode, int progress)
        {
            Log.Debug("WSD: Watching episode {0} ({1}%)", episode.Title, progress);
            return true;
        }

        public bool FinishEpisode(int provider, WebTVEpisodeDetailed episode)
        {
            Log.Debug("WSD: Finished episode {0}", episode.Title);
            return true;
        }

        public bool CancelWatchingEpisode(int provider, WebTVEpisodeDetailed episode)
        {
            Log.Debug("WSD: Canceled episode {0}", episode.Title);
            return true;
        }
    }
}
