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
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.Libraries.Social
{
    public interface IWatchSharingService
    {
        int UpdateInterval { get; } // in minutes
        Dictionary<string, string> Configuration { get; set;  }

        bool TestCredentials(string username, string password);
        string HashPassword(string password);

        bool StartWatchingMovie(WebMovieDetailed movie);
        bool WatchingMovie(WebMovieDetailed movie, int progress);
        bool FinishMovie(WebMovieDetailed movie);
        bool CancelWatchingMovie(WebMovieDetailed movie);

        bool StartWatchingEpisode(WebTVShowDetailed show, WebTVSeasonDetailed season, WebTVEpisodeDetailed episode);
        bool WatchingEpisode(WebTVShowDetailed show, WebTVSeasonDetailed season, WebTVEpisodeDetailed episode, int progress);
        bool FinishEpisode(WebTVShowDetailed show, WebTVSeasonDetailed season, WebTVEpisodeDetailed episode);
        bool CancelWatchingEpisode(WebTVShowDetailed show, WebTVSeasonDetailed season, WebTVEpisodeDetailed episode);
    }
}
