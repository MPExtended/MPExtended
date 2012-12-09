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
using MoreLinq;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class SettingsController : BaseController
    {
        //
        // GET: /Settings/
        public ActionResult Index()
        {
            ViewBag.Version = VersionUtil.GetFullVersionString();
            ServiceAvailability.Reload();

            SettingsViewModel model = new SettingsViewModel(Settings.ActiveSettings);
            ProfileViewModel profile; 
            
            foreach (string platform in model.Platforms)
            {
                profile = model.AudioProfiles[platform];
                ViewData.Add(platform + "_audio", new SelectList(profile.AvailableProfiles, profile.DefaultProfile));
                profile = model.VideoProfiles[platform];
                ViewData.Add(platform + "_video", new SelectList(profile.AvailableProfiles, profile.DefaultProfile));
                profile = model.TvProfiles[platform];
                ViewData.Add(platform + "_tv", new SelectList(profile.AvailableProfiles, profile.DefaultProfile));
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(SettingsViewModel model)
        {
            ViewBag.Version = VersionUtil.GetFullVersionString();

            if (!ModelState.IsValid)
            {
                return View(new SettingsViewModel(Settings.ActiveSettings));
            }

            foreach (string platform in model.Platforms)
            {
                model.AudioProfiles[platform].DefaultProfile = Request.Form[platform + "_audio"];
                model.VideoProfiles[platform].DefaultProfile = Request.Form[platform + "_video"];
                model.TvProfiles[platform].DefaultProfile = Request.Form[platform + "_tv"];
            }

            model.SaveToConfiguration();
            Settings.ApplySkinSettings();
            return RedirectToAction("Index");
        }

        public ActionResult Services()
        {
            return View(new ServiceAddressesViewModel());
        }

        [HttpPost]
        public ActionResult Services(ServiceAddressesViewModel model)
        {
            Configuration.WebMediaPortal.MASUrl = model.MAS;
            Configuration.WebMediaPortal.TASUrl = model.TAS;
            Configuration.WebMediaPortal.ServiceUsername = model.Username;
            Configuration.WebMediaPortal.ServicePassword = model.Password;
            Configuration.Save();
            Connections.SetUrls(model.MAS, model.TAS);
            Log.Info("WebMediaPortal version {0} now connected with MAS {1} and TAS {2}",
                VersionUtil.GetFullVersionString(), Settings.ActiveSettings.MASUrl, Settings.ActiveSettings.TASUrl);
            Connections.LogServiceVersions();
            ServiceAvailability.Reload();
            return View("ServicesSaved");
        }

        public ActionResult DiscoverServices()
        {
            IServiceDiscoverer discoverer = new ServiceDiscoverer();
            var sets = discoverer.DiscoverSets(TimeSpan.FromSeconds(5));
            return Json(sets.DistinctBy(x => x.GetSetIdentifier()).Select(x => new FoundServicesModel(x)), JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckForUpdates()
        {
            if (!UpdateChecker.IsWorking())
                return Json(new { Error = true }, JsonRequestBehavior.AllowGet);
            if (!UpdateChecker.IsUpdateAvailable())
                return Json(new { Error = false, UpdateAvailable = false }, JsonRequestBehavior.AllowGet);

            var newVersion = UpdateChecker.GetLastReleasedVersion();
            return Json(new { Error = false, UpdateAvailable = true, NewVersion = newVersion.Version }, JsonRequestBehavior.AllowGet);
        }
    }
}
