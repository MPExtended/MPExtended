#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.Service;
using MPExtended.Applications.WebMediaPortal.Code;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    public class ErrorController : BaseController
    {
        public ActionResult General(Exception exception)
        {
            Log.Warn(String.Format("Error happened in controller body during request {0}", HttpContext.Request.RawUrl), exception);
            ViewBag.Request = HttpContext.Request.Url;
            return View(exception);
        }

        public ActionResult Http404()
        {
            Log.Info("Requested non-existing page {0}", HttpContext.Request.RawUrl);
            return View();
        }
    }
}
