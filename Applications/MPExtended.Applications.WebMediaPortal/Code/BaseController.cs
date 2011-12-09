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
using System.Web;
using System.Web.Mvc;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public class BaseController : Controller
    {
        private static bool establishedMasConnection;
        private static bool establishedTasConnection;

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || filterContext.IsChildAction)
            {
                return;
            }

            // log the error
            Log.Warn("Error during controller body", filterContext.Exception);

            // set viewbag properties
            SetViewBagProperties();

            // return exception page
            filterContext.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error.cshtml",
                ViewData = new ViewDataDictionary()
                {
                    Model = filterContext.Exception
                },
            };
            (filterContext.Result as ViewResult).ViewBag.LayoutHasMAS = ViewBag.LayoutHasMAS;
            (filterContext.Result as ViewResult).ViewBag.LayoutHasTAS = ViewBag.LayoutHasTAS;
            filterContext.ExceptionHandled = true;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SetViewBagProperties();
        }

        private void SetViewBagProperties()
        {
            establishedMasConnection = establishedMasConnection ? true : MPEServices.HasMASConnection;
            establishedTasConnection = establishedTasConnection ? true : MPEServices.HasTASConnection;
            ViewBag.LayoutHasMAS = establishedMasConnection;
            ViewBag.LayoutHasTAS = establishedTasConnection;
        }
    }
}