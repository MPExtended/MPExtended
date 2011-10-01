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
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [Authorize]
    public class MovieLibraryController : Controller
    {
        //
        // GET: /MovieLibrary/
        public ActionResult Index()
        {

            try
            {
                var movieList = MPEServices.NetPipeMediaAccessService.GetAllMoviesBasic(SortBy.Title, OrderBy.Asc);
                if (movieList != null)
                {
                    return View(movieList);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MovieLibrary.Index", ex);
            }
            return View("Error");
        }

        public ActionResult Details(string movie)
        {

            try
            {
                var fullMovie = MPEServices.NetPipeMediaAccessService.GetMovieDetailedById(movie);
                if (fullMovie != null)
                {
                    return View(fullMovie);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MovieLibrary.Details", ex);
                return View("Error");

            }
            return View("Error");
        }

        public ActionResult Play(string movie)
        {

            try
            {
                var fullMovie = MPEServices.NetPipeMediaAccessService.GetMovieDetailedById(movie);
                if (fullMovie != null)
                {
                    return View(fullMovie);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MovieLibrary.Details", ex);
            }
            return View("Error");
        }

        public ActionResult Image(string movie)
        {
            try
            {
                var image = MPEServices.NetPipeStreams.GetArtwork(WebStreamMediaType.Movie, WebArtworkType.Banner, movie, 0);
                if (image != null)
                {
                    return File(image, "image/jpg");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MovieLibrary.Image", ex);
            }
            return null;
        }
    }
}
