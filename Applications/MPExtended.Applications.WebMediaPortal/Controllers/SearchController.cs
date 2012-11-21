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
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class SearchController : BaseController
    {
        //
        // GET: /Search/
        public ActionResult Index()
        {
            return View("Search");
        }

        public ActionResult Results(string text)
        {
            IEnumerable<SearchResultsViewModel> list = new List<SearchResultsViewModel>();
            if (ServiceAvailability.MAS)
            {
                list = list.Concat(Connections.Current.MAS.Search(text).Select(x => new SearchResultsViewModel(x, CreateLink(x))));
            }

            if (ServiceAvailability.TAS)
            {
                list = list.Concat(Connections.Current.TAS.Search(text).Select(x => new SearchResultsViewModel(x, CreateLink(x))));
            }

            list = list.Where(x => x.URL != null).OrderByDescending(x => x.Score);

            // when there is *only* one hit with a 100% score, just redirect to that page
            if(list.Where(x => x.Score == 100).Count() == 1)
            {
                return Redirect(list.First().URL);
            }

            // else show all hits
            return View("Results", list);
        }

        private string CreateLink(WebSearchResult result)
        {
            switch (result.Type)
            {
                case WebMediaType.Movie:
                    return Url.Action("Details", "MovieLibrary", new { movie = result.Id });
                case WebMediaType.MusicAlbum:
                    return Url.Action("Album", "MusicLibrary", new { album = result.Id });
                case WebMediaType.MusicArtist:
                    return Url.Action("Albums", "MusicLibrary", new { artist = result.Id });
                case WebMediaType.MusicTrack:
                    return Url.Action("Album", "MusicLibrary", new { album = result.Details["AlbumId"], track = result.Id });
                case WebMediaType.TVEpisode:
                    return Url.Action("Details", "TVShowsLibrary", new { episode = result.Id });
                case WebMediaType.TVSeason:
                    return Url.Action("Episodes", "TVShowsLibrary", new { season = result.Id });
                case WebMediaType.TVShow:
                    return Url.Action("Seasons", "TVShowsLibrary", new { show = result.Id });
                default:
                    return null;
            }
        }

        private string CreateLink(WebTVSearchResult result)
        {
            switch (result.Type)
            {
                case WebTVSearchResultType.Program:
                    return Url.Action("ProgramDetails", "Television", new { programId = result.Id });
                case WebTVSearchResultType.Recording:
                    return Url.Action("Recording", "Television", new { id = result.Id });
                case WebTVSearchResultType.Schedule:
                    return Url.Action("Schedules", "Television");
                case WebTVSearchResultType.TVChannel:
                    return Url.Action("WatchLiveTV", "Television", new { channelId = result.Id });
                case WebTVSearchResultType.TVGroup:
                    return Url.Action("TVGuide", "Television", new { group = result.Id });
                default:
                    return null;
            }
        }
    }
}
