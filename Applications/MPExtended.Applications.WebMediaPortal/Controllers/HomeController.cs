#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NewMovies()
        {
             var tmp = MPEServices.NetPipeMediaAccessService.GetMoviesDetailedByRange(null, null, 0, 3, SortBy.DateAdded, OrderBy.Desc);
             return PartialView(tmp);
        }

        public ActionResult NewEpisodes()
        {
            var tmp = MPEServices.NetPipeMediaAccessService.GetTVEpisodesDetailedByRange(0, 3, SortBy.TVDateAired, OrderBy.Desc);
            var list = tmp.Select(x => new EpisodeModel
            {
                Episode = x,
                Season = MPEServices.NetPipeMediaAccessService.GetTVSeasonDetailedById(x.SeasonId),
                Show = MPEServices.NetPipeMediaAccessService.GetTVShowDetailedById(x.ShowId)
            });

            return PartialView(list);
        }

        public ActionResult NewRecordings()
        {
            List<WebRecordingBasic> tmp = MPEServices.NetPipeTVAccessService.GetRecordings().OrderByDescending(p => p.StartTime).ToList();
            return PartialView(tmp.Count > 4 ? tmp.GetRange(0, 4) : tmp);
        }

        public ActionResult CurrentSchedules()
        {
            var tmp = MPEServices.NetPipeTVAccessService.GetSchedules();
            return PartialView(tmp.Where(p => p.StartTime.Day == DateTime.Now.Day));
        }
    }
}
