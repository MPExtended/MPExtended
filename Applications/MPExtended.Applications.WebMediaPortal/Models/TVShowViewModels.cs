#region Copyright (C) 2012-2013 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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

using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.Service;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

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
                Seasons = Connections.Current.MAS.GetTVSeasonsDetailedForTVShow(Show.PID, Show.Id, sort: WebSortField.TVSeasonNumber, order: WebSortOrder.Asc);
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
                Episodes = Connections.Current.MAS.GetTVEpisodesDetailedForSeason(Season.PID, seasonId, sort: WebSortField.TVEpisodeNumber, order: WebSortOrder.Asc)
                    .Where(x => !String.IsNullOrEmpty(x.Title));
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to load season {0}", seasonId), ex);
            }
        }
    }

    public class TVEpisodeViewModel : MediaItemModel
    {
        public WebTVEpisodeDetailed Episode { get; private set; }

        // Lazily load the show and season
        private WebTVShowDetailed _show;
        private WebTVSeasonDetailed _season;

        public WebTVShowDetailed Show
        {
            get
            {
                if (_show == null)
                    _show = Connections.Current.MAS.GetTVShowDetailedById(Episode.PID, Episode.ShowId);

                return _show;
            }
        }

        public WebTVSeasonDetailed Season
        {
            get
            {
                if (_season == null)
                    _season = Connections.Current.MAS.GetTVSeasonDetailedById(Episode.PID, Episode.SeasonId);

                return _season;
            }
        }

        protected override WebMediaItem Item { get { return Episode; } }

        public TVEpisodeViewModel(WebTVShowDetailed show, WebTVSeasonDetailed season, WebTVEpisodeDetailed episode)
        {
            Episode = episode;
            _show = show;
            _season = season;
        }

        public TVEpisodeViewModel(WebTVEpisodeDetailed episode)
        {
            Episode = episode;
        }

        public TVEpisodeViewModel(string episodeId)
        {
            try
            {
                Episode = Connections.Current.MAS.GetTVEpisodeDetailedById(Settings.ActiveSettings.TVShowProvider, episodeId);
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to load episode {0}", episodeId), ex);
            }
        }
    }
}