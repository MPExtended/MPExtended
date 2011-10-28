#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [Authorize]
    public class TVShowsLibraryController : Controller
    {
        public ActionResult Index()
        {
            var series = MPEServices.NetPipeMediaAccessService.GetAllTVShowsBasic(Settings.ActiveSettings.TVShowProvider, null, null, SortBy.Title, OrderBy.Asc);
            if (series != null)
            {
                return View(series);
            }
            return null;
        }

        public ActionResult Seasons(string show)
        {
            var seasons = MPEServices.NetPipeMediaAccessService.GetTVSeasonsBasicForTVShow(Settings.ActiveSettings.TVShowProvider, show, SortBy.TVSeasonNumber, OrderBy.Asc);
            if (seasons != null)
            {
                return View(seasons);
            }
            return null;
        }

        public ActionResult Episodes(string season)
        {
            var episodes = MPEServices.NetPipeMediaAccessService.GetTVEpisodesBasicForSeason(Settings.ActiveSettings.TVShowProvider, season, SortBy.TVEpisodeNumber, OrderBy.Asc);
            if (episodes != null)
            {
                return View(episodes);
            }
            return null;
        }

        public ActionResult Image(string season)
        {
            var image = MPEServices.NetPipeStreams.GetArtwork(WebStreamMediaType.TVSeason, Settings.ActiveSettings.TVShowProvider, season, WebArtworkType.Banner, 0);
            if (image != null)
            {
                return File(image, "image/jpg");
            }
            return null;
        }

        public ActionResult EpisodeImage(string episode)
        {
            var image = MPEServices.NetPipeStreams.GetArtwork(WebStreamMediaType.TVEpisode, Settings.ActiveSettings.TVShowProvider, episode, WebArtworkType.Banner, 0);
            if (image != null)
            {
                return File(image, "image/jpg");
            }
            return null;
        }

        public ActionResult SeriesFanart(string show)
        {
            var image = MPEServices.NetPipeStreams.GetArtwork(WebStreamMediaType.TVShow, Settings.ActiveSettings.TVShowProvider, show, WebArtworkType.Backdrop, 0);
            if (image != null)
            {
                return File(image, "image/jpg");
            }
            return null;
        }

        public ActionResult Details(string episode)
        {
            var fullEpisode = MPEServices.NetPipeMediaAccessService.GetTVEpisodeDetailedById(Settings.ActiveSettings.TVShowProvider, episode);
            if (fullEpisode != null)
            {
                ViewBag.ShowPlay = fullEpisode.Path != null;
                return View(fullEpisode);
            }
            return null;
        }

        public ActionResult Play(string episode)
        {
            var fullEpisode = MPEServices.NetPipeMediaAccessService.GetTVEpisodeDetailedById(Settings.ActiveSettings.TVShowProvider, episode);
            if (fullEpisode != null)
            {
                EpisodeModel model = new EpisodeModel()
                {
                    Episode = fullEpisode,
                    Show = MPEServices.NetPipeMediaAccessService.GetTVShowDetailedById(fullEpisode.PID, fullEpisode.ShowId),
                    Season = MPEServices.NetPipeMediaAccessService.GetTVSeasonDetailedById(fullEpisode.PID, fullEpisode.SeasonId)
                };
                return View(model);
            }
            return null;
        }
    }
}
