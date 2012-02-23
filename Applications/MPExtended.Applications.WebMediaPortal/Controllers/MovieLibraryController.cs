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
using MPExtended.Libraries.Client;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class MovieLibraryController : BaseController
    {
        //
        // GET: /MovieLibrary/
        public ActionResult Index()
        {
            var movieList = MPEServices.MAS.GetAllMoviesBasic(Settings.ActiveSettings.MovieProvider, sort:SortBy.Title, order:OrderBy.Asc);
            if (movieList != null)
            {
                return View(movieList);
            }
            return null;
        }

        public ActionResult Details(string movie)
        {
            var fullMovie = MPEServices.MAS.GetMovieDetailedById(Settings.ActiveSettings.MovieProvider, movie);
            if (fullMovie != null)
            {
                return View(fullMovie);
            }
            return null;
        }

        public ActionResult Play(string movie)
        {
            var fullMovie = MPEServices.MAS.GetMovieDetailedById(Settings.ActiveSettings.MovieProvider, movie);
            if (fullMovie != null)
            {
                return View(fullMovie);
            }
            return null;
        }

        public ActionResult Image(string movie)
        {
            var image = MPEServices.MASStream.GetArtwork(WebStreamMediaType.Movie, Settings.ActiveSettings.MovieProvider, movie, WebArtworkType.Cover, 0);
            if (image != null)
            {
                return File(image, "image/jpg");
            }
            return null;
        }
    }
}
