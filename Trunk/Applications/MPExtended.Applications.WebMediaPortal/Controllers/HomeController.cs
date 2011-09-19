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
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Libraries.ServiceLib;

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
        public ActionResult NewEpisodes()
        {
            return PartialView();
        }

        public ActionResult NewMovies()
        {
            try
            {
                List<WebMovieFull> tmp = MPEServices.NetPipeMediaAccessService.GetMoviesDetailed(1, 4, SortBy.DateAdded, OrderBy.Desc);
                return PartialView(tmp);
            }
            catch (Exception ex)
            {
                Log.Error("Exception in Home.NewMovies" + ex.ToString(), ex);
            }
            return PartialView("Error");
        }

        public ActionResult NewRecordings()
        {
            try
            {
                List<WebRecordingBasic> tmp = MPEServices.NetPipeTVAccessService.GetRecordings().OrderByDescending(p => p.StartTime).ToList();
                return PartialView(tmp.GetRange(0, tmp.Count / 10));
            }
            catch (Exception ex)
            {
                Log.Error("Exception in Home.NewRecordings" + ex.ToString(), ex);
            }
            return PartialView("Error");
        }

        public ActionResult CurrentSchedules()
        {
            try
            {
                List<WebScheduleBasic> tmp = MPEServices.NetPipeTVAccessService.GetSchedules();
                return PartialView(tmp.Where(p => p.StartTime.Day == DateTime.Now.Day));


            }
            catch (Exception ex)
            {
                Log.Error("Exception in Home.CurrentSchedules" + ex.ToString(), ex);
            }
            return PartialView("Error");
        }
    }
}
