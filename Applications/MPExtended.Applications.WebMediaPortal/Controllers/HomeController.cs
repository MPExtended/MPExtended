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
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class HomeController : BaseController
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View(new HomeViewModel(ServiceAvailability));
        }

        public ActionResult NewMovies(int count)
        {
            return View(new HomeViewModel(ServiceAvailability).GetLastAddedMovies(count));
        }

        public ActionResult NewEpisodes(int count)
        {
            return View(new HomeViewModel(ServiceAvailability).GetLastAddedTVEpisodes(count));
        }

        public ActionResult AiredEpisodes(int count)
        {
            return View(new HomeViewModel(ServiceAvailability).GetLastAiredTVEpisodes(count));
        }

        public ActionResult NewAlbums(int count)
        {
            return View(new HomeViewModel(ServiceAvailability).GetLastAddedAlbums(count));
        }

        public ActionResult NewMusicTracks(int count)
        {
            return View(new HomeViewModel(ServiceAvailability).GetLastAddedMusicTracks(count));
        }

        public ActionResult NewRecordings(int count)
        {
            return View(new HomeViewModel(ServiceAvailability).GetLastRecordings(count));
        }

        public ActionResult CurrentSchedules()
        {
            return View(new HomeViewModel(ServiceAvailability).GetTodaysSchedules());
        }
    }
}
