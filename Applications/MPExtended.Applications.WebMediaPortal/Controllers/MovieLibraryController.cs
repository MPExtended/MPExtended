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
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class MovieLibraryController : BaseController
    {
        //
        // GET: /MovieLibrary/
        public ActionResult Index(string genre = null)
        {
            IEnumerable<WebMovieDetailed> movieList = MPEServices.MAS.GetAllMoviesDetailed(Settings.ActiveSettings.MovieProvider, sort: WebSortField.Title, order: WebSortOrder.Asc);
            if (!String.IsNullOrEmpty(genre))
            {
                movieList = movieList.Where(x => x.Genres.Contains(genre));
            }

            return View(movieList.Select(x => new MovieViewModel(x)));
        }

        public ActionResult Details(string movie)
        {
            var model = new MovieViewModel(movie);
            if (model.Movie == null)
                return HttpNotFound();
            return View(model);
        }

        [HttpGet]
        public ActionResult MovieInfo(string movie)
        {
            var model = new MovieViewModel(movie);
            if (model.Movie == null)
                return HttpNotFound();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Play(string movie)
        {
            var model = new MovieViewModel(movie);
            if (model.Movie == null)
                return HttpNotFound();
            return View(model);
        }

        public ActionResult Cover(string movie, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.Movie, movie, WebArtworkType.Cover, width, height, "Images/default/movie-cover.png");
        }

        public ActionResult Fanart(string movie, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.Movie, movie, WebArtworkType.Backdrop, width, height, "Images/default/movie-fanart.png");
        }
    }
}
