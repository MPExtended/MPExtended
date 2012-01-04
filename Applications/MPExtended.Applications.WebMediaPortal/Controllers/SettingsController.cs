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
using MPExtended.Libraries.General;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [Authorize]
    public class SettingsController : BaseController
    {
        //
        // GET: /Settings/
        public ActionResult Index()
        {
            ViewBag.Version = VersionUtil.GetVersionName();
            ViewBag.BuildVersion = VersionUtil.GetBuildVersion().ToString();

            return View(new SettingsViewModel(Settings.ActiveSettings));
        }

        [HttpPost]
        public ActionResult Index(SettingsViewModel model)
        {
            ViewBag.Version = VersionUtil.GetVersionName();
            ViewBag.BuildVersion = VersionUtil.GetBuildVersion().ToString();

            if (!ModelState.IsValid)
            {
                return View(new SettingsViewModel(Settings.ActiveSettings));
            }

            Settings.ActiveSettings = model.ToSettingModel(Settings.ActiveSettings);
            return RedirectToAction("Index");
        }

        public ActionResult Services()
        {
            return View(new ServiceSettingsViewModel(Settings.ActiveSettings));
        }

        [HttpPost]
        public ActionResult Services(ServiceSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(new ServiceSettingsViewModel(Settings.ActiveSettings));
            }

            Settings.ActiveSettings = model.ToSettingModel(Settings.ActiveSettings);
            MPEServices.SetConnectionUrls(Settings.ActiveSettings.MASUrl, Settings.ActiveSettings.TASUrl);
            return RedirectToAction("Services");
        }
    }
}
