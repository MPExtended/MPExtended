#region Copyright (C) 2012-2013 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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
using System.Linq;
using System.Web.Mvc;

using MPExtended.Services.Common.Interfaces;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
  [ServiceAuthorize]
  public class MovieLibraryController : BaseController
  {
    //
    // GET: /MovieLibrary/
    public ActionResult Index(string filter = null)
    {
      var movieList = Connections.Current.MAS.GetMoviesDetailed(Settings.ActiveSettings.MovieProvider, filter, WebSortField.Title, WebSortOrder.Asc);
      return View(movieList.Where(x => !String.IsNullOrEmpty(x.Title)).Select(x => new MovieViewModel(x)));
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
      return Images.ReturnFromService(WebMediaType.Movie, movie, WebFileType.Cover, width, height, "Images/default/movie-cover.png");
    }

    public ActionResult Fanart(string movie, int width = 0, int height = 0, int num = -1)
    {
      return Images.ReturnFromService(WebMediaType.Movie, movie, WebFileType.Backdrop, width, height, "Images/default/movie-fanart.png", num);
    }

    public ActionResult Logo(string movie, int width = 0, int height = 0)
    {
      return Images.ReturnFromService(WebMediaType.Movie, movie, WebFileType.Logo, width, height, "Images/default/movie-logo.png");
    }

    // Actors
    public ActionResult Actors(string filter = null)
    {
      var actors = Connections.Current.MAS.GetMovieActors(Settings.ActiveSettings.MovieProvider, filter, WebSortField.Title, WebSortOrder.Asc)
          .Where(x => !String.IsNullOrEmpty(x.Title));
      return View(actors);
    }

    public ActionResult Actor(string actor)
    {
      var model = new MovieActorViewModel(actor);
      if (model.Actor == null)
        return HttpNotFound();

      return View(model);
    }

    public ActionResult ActorCover(string actor, int width = 0, int height = 0)
    {
      return Images.ReturnFromService(WebMediaType.MovieActor, actor, WebFileType.Cover, width, height, "Images/default/actor-cover.png");
    }

    public ActionResult ActorFanart(string actor, int width = 0, int height = 0, int num = -1)
    {
      return Images.ReturnFromService(WebMediaType.MovieActor, actor, WebFileType.Backdrop, width, height, "Images/default/actor-fanart.png", num);
    }

    // Collection
    public ActionResult Collections(string filter = null)
    {
      var collection = Connections.Current.MAS.GetCollections(Settings.ActiveSettings.MovieProvider, filter, WebSortField.Title, WebSortOrder.Asc)
          .Where(x => !String.IsNullOrEmpty(x.Title));
      return View(collection);
    }

    public ActionResult Collection(string collection)
    {
      var model = new CollectionViewModel(collection);
      if (model.Collection == null)
        return HttpNotFound();

      return View(model);
    }

    public ActionResult CollectionCover(string collection, int width = 0, int height = 0)
    {
      return Images.ReturnFromService(WebMediaType.Collection, collection, WebFileType.Cover, width, height, "Images/default/collection-cover.png");
    }

    public ActionResult CollectionFanart(string collection, int width = 0, int height = 0, int num = -1)
    {
      return Images.ReturnFromService(WebMediaType.Collection, collection, WebFileType.Backdrop, width, height, "Images/default/collection-fanart.png", num);
    }

    // Genres
    public ActionResult Genres(string filter = null)
    {
      var genres = Connections.Current.MAS.GetMovieGenres(Settings.ActiveSettings.MovieProvider, filter, WebSortField.Title, WebSortOrder.Asc)
          .Where(x => !String.IsNullOrEmpty(x.Title));
      return View(genres);
    }

    public ActionResult Genre(string genre)
    {
      var model = new MovieGenreViewModel(genre);
      if (model.Genre == null)
        return HttpNotFound();

      return View(model);
    }

    public ActionResult GenreCover(string genre, int width = 0, int height = 0)
    {
      return Images.ReturnFromService(WebMediaType.MovieGenre, genre, WebFileType.Cover, width, height, "Images/default/genre-cover.png");
    }

    public ActionResult GenreFanart(string genre, int width = 0, int height = 0, int num = -1)
    {
      return Images.ReturnFromService(WebMediaType.MovieGenre, genre, WebFileType.Backdrop, width, height, "Images/default/genre-fanart.png", num);
    }
  }
}
