#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Applications.WebMediaPortal.Strings;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public enum SearchResultType
    {
        // From WebMediaType
        Movie = 0,
        MusicTrack = 1,
        Picture = 2,
        TVEpisode = 3,
        File = 4,
        TVShow = 5,
        TVSeason = 6,
        MusicAlbum = 7,
        MusicArtist = 8,
        Folder = 9,
        Drive = 10,
        Playlist = 11,
        Collection = 12,
        // Leaving out items from TAS, as those can't be returned

        // From WebTVSearchResult
        TVChannel = 100,
        RadioChannel = 101,
        Schedule = 102,
        Recording = 103,
        TVGroup = 104,
        RadioGroup = 105,
        Program = 106
    }

    public class SearchResultsViewModel
    {
        public string Title { get; set; }
        public int Score { get; set; }
        public string URL { get; set; }
        public SearchResultType Type { get; set; }

        public SearchResultsViewModel(WebSearchResult res, string url)
        {
            Score = res.Score;
            URL = url;
            Type = (SearchResultType)res.Type;
            Title = GetTitle(res);
        }

        public SearchResultsViewModel(WebTVSearchResult res, string url)
        {
            URL = url;
            Score = res.Score;
            Type = (SearchResultType)(res.Type + 100);
            Title = GetTitle(res);
        }

        private string GetTitle(WebSearchResult result)
        {
            switch (result.Type)
            {                
                case WebMediaType.MusicAlbum:
                    return String.Format(FormStrings.SearchResultAlbum, result.Title, result.Details["Artist"]);
                case WebMediaType.MusicTrack:
                    return String.Format(FormStrings.SearchResultTrack, result.Title, result.Details["Artist"]);

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
                    return String.Format(FormStrings.SearchResultRecording, result.Title, result.StartTime);
                case WebTVSearchResultType.Schedule:
                    return String.Format(FormStrings.SearchResultSchedule, result.Title);
                case WebTVSearchResultType.TVGroup:
                    return String.Format(FormStrings.SearchResultChannelGroup, result.Title);
                case WebTVSearchResultType.Program:
                    return String.Format(FormStrings.SearchResultProgram, result.Title, result.StartTime, result.ChannelName);

                default:
                    return result.Title;
            }
        }
    }
}
