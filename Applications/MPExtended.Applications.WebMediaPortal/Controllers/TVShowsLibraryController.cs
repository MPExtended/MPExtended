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
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class TVShowsLibraryController : BaseController
    {
        // Series
        public ActionResult Index(string genre = null)
        {
            IEnumerable<WebTVShowDetailed> series = Connections.Current.MAS.GetTVShowsDetailed(Settings.ActiveSettings.TVShowProvider);
            if (!String.IsNullOrEmpty(genre))
                series = series.Where(x => x.Genres.Contains(genre));

            return View(series);
        }

        public ActionResult ShowInfo(string show)
        {
            var model = new TVShowViewModel(show);
            if (model.Show == null)
                return HttpNotFound();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SeriesFanart(string show, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.TVShow, show, WebFileType.Backdrop, width, height, "Images/default/tvshow-fanart.png");
        }

        public ActionResult SeriesPoster(string show, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.TVShow, show, WebFileType.Poster, width, height, "Images/default/tvshow-poster.png");
        }

        public ActionResult SeriesBanner(string show, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.TVShow, show, WebFileType.Banner, width, height, "Images/default/tvshow-banner.png");
        }

        // Seasons
        public ActionResult Seasons(string show)
        {
            var model = new TVShowViewModel(show);
            if (model.Show == null)
                return HttpNotFound();

            return View(model);
        }

        public ActionResult SeasonInfo(string season)
        {
            var model = new TVSeasonViewModel(season);
            if (model.Season == null)
                return HttpNotFound();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Image(string season, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.TVSeason, season, WebFileType.Banner, width, height, "Images/default/tvseason-banner.png");
        }

        // Episodes
        public ActionResult Episodes(string season)
        {
            var model = new TVSeasonViewModel(season);
            if (model.Season == null)
                return HttpNotFound();

            return View(model);
        }

        public ActionResult Details(string episode)
        {
            var model = new TVEpisodeViewModel(episode);
            if (model.Episode == null)
                return HttpNotFound();

            ViewBag.ShowPlay = model.Episode.Path.Count > 0;
            return View(model);
        }

        public ActionResult EpisodeInfo(string episode)
        {
            var model = new TVEpisodeViewModel(episode);
            if (model.Episode == null)
                return HttpNotFound();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Play(string episode)
        {
            var model = new TVEpisodeViewModel(episode);
            if (model.Episode == null)
                return HttpNotFound();

            return View(model);
        }

        public ActionResult EpisodeImage(string episode, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.TVEpisode, episode, WebFileType.Banner, width, height, "Images/default/tvepisode-banner.png");
        }
    }
}
