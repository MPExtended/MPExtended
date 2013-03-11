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
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Libraries.Service;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class HomeController : BaseController
    {
        private string PartialViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var writer = new StringWriter())
            {
                var result = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var context = new ViewContext(ControllerContext, result.View, ViewData, TempData, writer);
                result.View.Render(context, writer);

                return writer.ToString();
            }
        }

        private ActionResult CreateResult(string view, object model, bool partial)
        {
            if (partial)
            {
                try
                {
                    return Content(PartialViewToString(view, model));
                }
                catch (Exception ex)
                {
                    Log.Error(String.Format("Failed to render partial view {0} on homepage", view), ex);
                    return new EmptyResult();
                }
            }

            return View(view, model);
        }

        private ActionResult CreateResult(object model, bool partial)
        {
            return CreateResult(ControllerContext.RouteData.GetRequiredString("action"), model, partial);
        }

        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View(new HomeViewModel(ServiceAvailability));
        }

        public ActionResult NewMovies(int count, bool partial = false)
        {
            return CreateResult(new HomeViewModel(ServiceAvailability).GetLastAddedMovies(count), partial);
        }

        public ActionResult NewEpisodes(int count, bool partial = false)
        {
            return CreateResult(new HomeViewModel(ServiceAvailability).GetLastAddedTVEpisodes(count), partial);
        }

        public ActionResult AiredEpisodes(int count, bool partial = false)
        {
            return CreateResult(new HomeViewModel(ServiceAvailability).GetLastAiredTVEpisodes(count), partial);
        }

        public ActionResult NewAlbums(int count, bool partial = false)
        {
            return CreateResult(new HomeViewModel(ServiceAvailability).GetLastAddedAlbums(count), partial);
        }

        public ActionResult NewMusicTracks(int count, bool partial = false)
        {
            return CreateResult(new HomeViewModel(ServiceAvailability).GetLastAddedMusicTracks(count), partial);
        }

        public ActionResult NewRecordings(int count, bool partial = false)
        {
            return CreateResult(new HomeViewModel(ServiceAvailability).GetLastRecordings(count), partial);
        }

        public ActionResult CurrentSchedules(bool partial = false)
        {
            return CreateResult(new HomeViewModel(ServiceAvailability).GetTodaysSchedules(), partial);
        }
    }
}
