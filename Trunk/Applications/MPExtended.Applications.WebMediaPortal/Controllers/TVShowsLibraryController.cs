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
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ServiceModel;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Libraries.ServiceLib;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [Authorize]
    public class TVShowsLibraryController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                var series = MPEServices.NetPipeMediaAccessService.GetAllSeries();
                if (series != null)
                {
                    return View(series);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in TVShowsLibrary.Index", ex);
            }
            return View("Error");
        }

        public ActionResult Seasons(int serie)
        {
            try
            {
                var seasons = MPEServices.NetPipeMediaAccessService.GetAllSeasons(serie);
                if (seasons != null)
                {
                    return View(seasons);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in TVShowsLibrary.Seasons", ex);
            }
            return View("Error");
        }

        public ActionResult Episodes(int serie, int season)
        {
            try
            {
                var episodes = MPEServices.NetPipeMediaAccessService.GetAllEpisodesForSeason(serie, season);
                if (episodes != null)
                {
                    return View(episodes);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in TVShowsLibrary.Episodes", ex);
            }
            return View("Error");
        }

        public ActionResult Image(int serie, int season)
        {
            try
            {
                var image = System.IO.File.ReadAllBytes(MPEServices.NetPipeMediaAccessService.GetSeason(serie, season).SeasonBanner);
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

        public ActionResult EpisodeImage(int episode)
        {
            try
            {
                var image = System.IO.File.ReadAllBytes(MPEServices.NetPipeMediaAccessService.GetFullEpisode(episode).BannerUrl);
                if (image != null)
                {
                    return File(image, "image/jpg");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MovieLibrary.EpisodeImage", ex);
            }
            return null;
        }

        public ActionResult SeriesFanart(int serie)
        {
            try
            {
                var image = System.IO.File.ReadAllBytes(MPEServices.NetPipeMediaAccessService.GetFullSeries(serie).CurrentFanartUrl);
                if (image != null)
                {
                    return File(image, "image/jpg");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MovieLibrary.SeriesFanart", ex);
            }
            return null;
        }

        public ActionResult Details(int episode)
        {
            try
            {
                var fullEpisode = MPEServices.NetPipeMediaAccessService.GetFullEpisode(episode);
                if (fullEpisode != null)
                {
                    ViewBag.ShowPlay = fullEpisode.EpisodeFile != null;
                    return View(fullEpisode);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in TVShowLibrary.Details", ex);
            }
            return View("Error");
        }

        public ActionResult Play(int episode)
        {
            try
            {
                var fullEpisode = MPEServices.NetPipeMediaAccessService.GetFullEpisode(episode);
                if (fullEpisode != null)
                    return View(new Tuple<WebEpisodeFull, WebSeriesFull>(fullEpisode, MPEServices.NetPipeMediaAccessService.GetFullSeries(fullEpisode.IdSerie)));
            }
            catch (Exception ex)
            {
                Log.Error("Exception in TVShowLibrary.Play", ex);
            }
            return View("Error");
        }
    }
}
