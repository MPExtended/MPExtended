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
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
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
                Show = MPEServices.MAS.GetTVShowDetailedById(Settings.ActiveSettings.TVShowProvider, showId);
                Seasons = MPEServices.MAS.GetTVSeasonsDetailedForTVShow(Show.PID, Show.Id, SortBy.TVSeasonNumber, OrderBy.Asc);
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
                Season = MPEServices.MAS.GetTVSeasonDetailedById(Settings.ActiveSettings.TVShowProvider, seasonId);
                Show = MPEServices.MAS.GetTVShowDetailedById(Season.PID, Season.ShowId);
                Episodes = MPEServices.MAS.GetTVEpisodesDetailedForSeason(Season.PID, seasonId, SortBy.TVEpisodeNumber, OrderBy.Asc);
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to load season {0}", seasonId), ex);
            }
        }
    }

    public class TVEpisodeViewModel
    {
        private WebMediaInfo mediaInfo;
        private WebFileInfo fileInfo;

        public WebTVShowDetailed Show { get; private set; }
        public WebTVSeasonDetailed Season { get; private set; }
        public WebTVEpisodeDetailed Episode { get; private set; }

        // See MovieViewModel for rationale behind making these a property
        public WebFileInfo FileInfo
        {
            get
            {
                if (fileInfo == null)
                    fileInfo = MPEServices.MAS.GetFileInfo(Episode.PID, WebMediaType.TVEpisode, WebFileType.Content, Episode.Id, 0);

                return fileInfo;
            }
        }

        public WebMediaInfo MediaInfo
        {
            get
            {
                if (mediaInfo == null)
                    mediaInfo = MPEServices.MASStreamControl.GetMediaInfo(WebStreamMediaType.TVEpisode, Episode.PID, Episode.Id);

                return mediaInfo;
            }
        }


        public string Quality
        {
            get
            {
                return MediaInfoFormatter.GetShortQualityName(MediaInfo);
            }
        }

        public string FullQuality
        {
            get
            {
                return MediaInfoFormatter.GetFullInfoString(MediaInfo, FileInfo);
            }
        }

        public TVEpisodeViewModel(WebTVShowDetailed show, WebTVSeasonDetailed season, WebTVEpisodeDetailed episode)
        {
            Show = show;
            Season = season;
            Episode = episode;
        }

        public TVEpisodeViewModel(string episodeId)
        {
            try
            {
                Episode = MPEServices.MAS.GetTVEpisodeDetailedById(Settings.ActiveSettings.TVShowProvider, episodeId);
                Season = MPEServices.MAS.GetTVSeasonDetailedById(Episode.PID, Episode.SeasonId);
                Show = MPEServices.MAS.GetTVShowDetailedById(Episode.PID, Episode.ShowId);
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to load episode {0}", episodeId), ex);
            }
        }
    }
}