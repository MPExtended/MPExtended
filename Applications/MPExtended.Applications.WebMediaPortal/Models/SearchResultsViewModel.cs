#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class SearchResultsViewModel
    {
        public string Title { get; set; }
        public int Score { get; set; }
        public string URL { get; set; }

        public SearchResultsViewModel(WebSearchResult res, string url)
        {
            Score = res.Score;
            URL = url;
            Title = GetTitle(res);
        }

        public SearchResultsViewModel(WebTVSearchResult res, string url)
        {
            URL = url;
            Score = res.Score;
            Title = GetTitle(res);
        }

        private string GetTitle(WebSearchResult result)
        {
            switch (result.Type)
            {                
                case WebMediaType.MusicAlbum:
                    return result.Title + " (album) by " + result.Details["Artist"];
                case WebMediaType.MusicTrack:
                    return result.Title + " by " + result.Details["Artist"];

                case WebMediaType.TVEpisode:
                    return String.Format("{0} ({1} {2}x{3})", result.Title, result.Details["ShowName"], result.Details["SeasonNumber"], result.Details["EpisodeNumber"]);

                default:
                    return result.Title;
            }
        }

        private string GetTitle(WebTVSearchResult result)
        {
            switch (result.Type)
            {
                case WebTVSearchResultType.Recording:
                    return String.Format("{0} (recorded on {1})", result.Title, result.StartTime);
                case WebTVSearchResultType.Schedule:
                    return String.Format("{0} (scheduled)", result.Title);
                case WebTVSearchResultType.TVGroup:
                    return String.Format("{0} (channel group)", result.Title);
                case WebTVSearchResultType.Program:
                    return String.Format("{0} ({1} on {2})", result.Title, result.StartTime, result.ChannelName);

                default:
                    return result.Title;
            }
        }
    }
}
