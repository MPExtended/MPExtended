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
using System.Web;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.Service;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class TVShowViewModel
    {
        public WebTVShowDetailed Show { get; set; }
        public IEnumerable<WebTVSeasonDetailed> Seasons { get; set; }

        public TVShowViewModel(WebTVShowDetailed show, IEnumerable<WebTVSeasonDetailed> seasons)
        {
            Show = show;
            Seasons = seasons;
        }

        public TVShowViewModel(string showId)
        {
            try
            {
                Show = Connections.Current.MAS.GetTVShowDetailedById(Settings.ActiveSettings.TVShowProvider, showId);
                Seasons = Connections.Current.MAS.GetTVSeasonsDetailedForTVShow(Show.PID, Show.Id, WebSortField.TVSeasonNumber, WebSortOrder.Asc);
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to load show {0}", showId), ex);
            }
        }
    }

    public class TVSeasonViewModel
    {
        public WebTVShowDetailed Show { get; private set; }
        public WebTVSeasonDetailed Season { get; private set; }
        public IEnumerable<WebTVEpisodeDetailed> Episodes { get; private set; }

        public TVSeasonViewModel(WebTVShowDetailed show, WebTVSeasonDetailed season, IEnumerable<WebTVEpisodeDetailed> episodes)
        {
            Show = show;
            Season = season;
            Episodes = episodes;
        }

        public TVSeasonViewModel(string seasonId)
        {
            try
            {
                Season = Connections.Current.MAS.GetTVSeasonDetailedById(Settings.ActiveSettings.TVShowProvider, seasonId);
                Show = Connections.Current.MAS.GetTVShowDetailedById(Season.PID, Season.ShowId);
                Episodes = Connections.Current.MAS.GetTVEpisodesDetailedForSeason(Season.PID, seasonId, WebSortField.TVEpisodeNumber, WebSortOrder.Asc);
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to load season {0}", seasonId), ex);
            }
        }
    }

    public class TVEpisodeViewModel : MediaItemModel
    {
        // TODO: Lazy-load these
        public WebTVShowDetailed Show { get; private set; }
        public WebTVSeasonDetailed Season { get; private set; }
        public WebTVEpisodeDetailed Episode { get; private set; }

        protected override WebMediaItem Item { get { return Episode; } }

        public TVEpisodeViewModel(WebTVShowDetailed show, WebTVSeasonDetailed season, WebTVEpisodeDetailed episode)
        {
            Show = show;
            Season = season;
            Episode = episode;
        }

        public TVEpisodeViewModel(WebTVEpisodeDetailed episode)
        {
            Episode = episode;
            Season = Connections.Current.MAS.GetTVSeasonDetailedById(Episode.PID, Episode.SeasonId);
            Show = Connections.Current.MAS.GetTVShowDetailedById(Episode.PID, Episode.ShowId);
        }

        public TVEpisodeViewModel(string episodeId)
        {
            try
            {
                Episode = Connections.Current.MAS.GetTVEpisodeDetailedById(Settings.ActiveSettings.TVShowProvider, episodeId);
                Season = Connections.Current.MAS.GetTVSeasonDetailedById(Episode.PID, Episode.SeasonId);
                Show = Connections.Current.MAS.GetTVShowDetailedById(Episode.PID, Episode.ShowId);
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to load episode {0}", episodeId), ex);
            }
        }
    }
}