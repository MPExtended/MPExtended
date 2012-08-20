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
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Controllers;
using MPExtended.Applications.WebMediaPortal.Mvc;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebMediaPortalApplication : HttpApplication
    {
        public static string GetInstallationDirectory()
        {
            // this should match with the path specified in the IIS Express config (see IISExpressHost.cs)
            return Installation.GetFileLayoutType() == FileLayoutType.Source ?
                Path.Combine(Installation.GetSourceRootDirectory(), "Applications", "MPExtended.Applications.WebMediaPortal") :
                Path.Combine(Installation.GetInstallDirectory(MPExtendedProduct.WebMediaPortal), "www");
        }

        protected void Application_Start()
        {
            // standard ASP.NET MVC setup
            AreaRegistration.RegisterAllAreas();
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            RouteTable.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            RouteTable.Routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "Home", action = "Index", id = UrlParameter.Optional });

            // initialize settings and the skin-override mechanism
            ContentLocator.Current = new ContentLocator(Context.Server);
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new SkinnableViewEngine());
            Settings.ApplySkinSettings();

            // set connection settings
            Connections.SetUrls(Settings.ActiveSettings.MASUrl, Settings.ActiveSettings.TASUrl);
            Log.Info("WebMediaPortal version {0} started with MAS {1} and TAS {2}",
                VersionUtil.GetFullVersionString(), Settings.ActiveSettings.MASUrl, Settings.ActiveSettings.TASUrl);
            Connections.LogServiceVersions();

            // automatically reload changes to the configuration files, mainly so that we instantly pick up new/deleted users. 
            Configuration.EnableChangeWatching();
        }

        protected void Application_Error()
        {
            Exception exception = null;
            try
            {
                // get exception and reset response
                exception = Server.GetLastError();
                var httpException = exception as HttpException;
                Response.Clear();
                Server.ClearError();

                // generate routing for new request context to the ErrorController
                var routeData = new RouteData();
                routeData.Values["controller"] = "Error";
                routeData.Values["action"] = "General";
                routeData.Values["exception"] = exception;
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // specific output for HTTP errors
                if (httpException != null)
                {
                    Response.StatusCode = httpException.GetHttpCode();
                    if (Response.StatusCode == (int)HttpStatusCode.NotFound)
                        routeData.Values["action"] = "Http404";
                }

                // start new controller
                IController errorController = new ErrorController();
                var rc = new RequestContext(new HttpContextWrapper(Context), routeData);
                errorController.Execute(rc);
            }
            catch (Exception secondException)
            {
                Log.Fatal("Error occured during error handling... I don't like this.");
                if (exception != null)
                    Log.Error("Original error was:", exception);
                Log.Error("Second exception is:", secondException);
            }
        }
    }
}