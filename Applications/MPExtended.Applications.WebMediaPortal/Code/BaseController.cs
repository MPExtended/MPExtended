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
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MPExtended.Libraries.Service;
using MPExtended.Applications.WebMediaPortal.Strings;
using MPExtended.Applications.WebMediaPortal.Models;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    // Requiring a session state results in the execution of requests being serialized, which is awful for the performance of certain
    // pages, especially those with a lot of images. 
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class BaseController : Controller
    {
        private static AvailabilityModel availabilityModel = new AvailabilityModel();

        protected AvailabilityModel ServiceAvailability
        {
            get
            {
                return availabilityModel;
            }
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            // make sure the thread locale is set before the model state is validated, which uses the thread locale
            LoadLanguage(requestContext.HttpContext.Request);
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
            (filterContext.Result as ViewResult).ViewBag.Request = filterContext.HttpContext.Request.Url;
            SetViewBagProperties((filterContext.Result as ViewResult).ViewBag);
            filterContext.ExceptionHandled = true;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SetViewBagProperties(ViewBag);
        }

        private void SetViewBagProperties(dynamic bag)
        {
            bag.Availability = ServiceAvailability;
            bag.FullVersion = VersionUtil.GetFullVersionString();
            bag.Language = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
        }

        private static void LoadLanguage(HttpRequestBase request)
        {
            // load a list of languages in order of preference
            List<string> languages = new List<string>();
            if (request.Params["language"] != null)
                languages.Add(request.Params["language"]);
            if (request.UserLanguages != null)
                languages.AddRange(request.UserLanguages);

            // load the highest-ranked available language
            foreach (string language in languages)
            {
                try
                {
                    // check if we've got resources for this language
                    CultureInfo ci = CultureInfo.CreateSpecificCulture(language);
                    ResourceManager manager = new ResourceManager(typeof(UIStrings));
                    if (manager.GetResourceSet(ci, true, true) == null)
                        continue;

                    Thread.CurrentThread.CurrentUICulture = ci;
                    Thread.CurrentThread.CurrentCulture = ci;
                    break;
                }
                catch (CultureNotFoundException)
                {
                    // just fall through to next language, or don't set a custom language worst case
                }
                catch (MissingManifestResourceException)
                {
                    // just fall through to next language, or don't set a custom language worst case
                }
            }
        }
    }
}