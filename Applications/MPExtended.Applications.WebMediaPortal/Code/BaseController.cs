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
using MPExtended.Libraries.General;
using MPExtended.Applications.WebMediaPortal.Models;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public class BaseController : Controller
    {
        protected AvailabilityModel ServiceAvailability
        {
            get
            {
                if (Session["availabilityModel"] == null)
                {
                    Session["availabilityModel"] = new AvailabilityModel();
                }
                return Session["availabilityModel"] as AvailabilityModel;
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || filterContext.IsChildAction)
            {
                return;
            }

            // log the error
            Log.Debug("During request {0}", filterContext.HttpContext.Request.RawUrl);
            Log.Warn("Error happened in controller body", filterContext.Exception);

            // return exception page
            filterContext.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error.cshtml",
                ViewData = new ViewDataDictionary()
                {
                    Model = filterContext.Exception
                },
            };
            (filterContext.Result as ViewResult).ViewBag.Availability = ServiceAvailability;
            (filterContext.Result as ViewResult).ViewBag.Version = VersionUtil.GetVersionName();
            (filterContext.Result as ViewResult).ViewBag.BuildVersion = VersionUtil.GetBuildVersion().ToString();
            (filterContext.Result as ViewResult).ViewBag.Request = filterContext.HttpContext.Request.Url;
            filterContext.ExceptionHandled = true;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.Availability = ServiceAvailability;
        }
    }
}