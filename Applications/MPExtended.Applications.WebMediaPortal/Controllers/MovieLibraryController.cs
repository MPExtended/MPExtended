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
using MPExtended.Libraries.Service;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class MovieLibraryController : BaseController
    {
        //
        // GET: /MovieLibrary/
        public ActionResult Index(string genre = null)
        {
            IEnumerable<WebMovieBasic> movieList = MPEServices.MAS.GetAllMoviesBasic(Settings.ActiveSettings.MovieProvider, sort: SortBy.Title, order: OrderBy.Asc);
            if (!String.IsNullOrEmpty(genre))
            {
                movieList = movieList.Where(x => x.Genres.Contains(genre));
            }

            return View(movieList);
        }

        public ActionResult Details(string movie)
        {
            var fullMovie = MPEServices.MAS.GetMovieDetailedById(Settings.ActiveSettings.MovieProvider, movie);
            if (fullMovie == null)
                return HttpNotFound();

            var fileInfo = MPEServices.MAS.GetFileInfo(fullMovie.PID, WebMediaType.Movie, WebFileType.Content, fullMovie.Id, 0);
            var mediaInfo = MPEServices.MASStreamControl.GetMediaInfo(WebStreamMediaType.Movie, fullMovie.PID, fullMovie.Id);
            ViewBag.Quality = MediaInfoFormatter.GetFullInfoString(mediaInfo, fileInfo);
            return View(fullMovie);
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

        public ActionResult Image(string movie, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(() =>
                MPEServices.MASStream.GetArtworkResized(WebStreamMediaType.Movie, Settings.ActiveSettings.MovieProvider, movie, WebArtworkType.Cover, 0, width, height));
        }
    }
}
