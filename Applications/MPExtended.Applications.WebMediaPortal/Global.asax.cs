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
using System.Web.Routing;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Mvc;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            Log.Setup("WebMediaPortal.log", false);

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            // initialize settings skin-override mechanism
            ContentLocator.Current = new ContentLocator(Context.Server, null);
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new SkinnableViewEngine());
            Settings.LoadSettings();

            // set connection settings
            MPEServices.SetConnectionUrls(Settings.ActiveSettings.MASUrl, Settings.ActiveSettings.TASUrl);
            Log.Info("WebMediaPortal version {0} starting with MAS {1} and TAS {2}",
                VersionUtil.GetFullVersionString(), Settings.ActiveSettings.MASUrl, Settings.ActiveSettings.TASUrl);
            MPEServices.LogServiceVersions();

            // automatically reload changes to the configuration files, mainly so that we instantly pick up new/deleted users. 
            Configuration.EnableChangeWatching();
        }
    }
}