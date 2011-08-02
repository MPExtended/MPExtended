using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Applications.WebMediaPortal.Services;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    public class SettingsController : Controller
    {
        //
        // GET: /Settings/
        
        public ActionResult Index()
        {
            return View(new SettingsViewModel());
        }
        public ActionResult Update(string SelectedProfile, string SelectedGroup)
        {
            SettingModel settings = new SettingModel();
            settings.DefaultGroup = Int32.Parse(SelectedGroup);
            settings.TranscodingProfile = SelectedProfile;
            Settings.GlobalSettings = settings;

            return RedirectToAction("Index");
        }

    }
}
