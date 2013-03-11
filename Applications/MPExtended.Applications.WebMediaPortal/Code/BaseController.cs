#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Net;
using System.Resources;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Applications.WebMediaPortal.Strings;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    // Requiring a session state results in the execution of requests being serialized, which is awful for the performance of certain
    // pages, especially those with a lot of images. 
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public abstract class BaseController : Controller
    {
        private static AvailabilityModel _availabilityModel;

        protected AvailabilityModel ServiceAvailability
        {
            get
            {
                if (_availabilityModel == null)
                    _availabilityModel = new AvailabilityModel();

                return _availabilityModel;
            }
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            // make sure the thread locale is set before the model state is validated, which uses the thread locale
            LoadLanguage(requestContext.HttpContext.Request);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SetViewBagProperties(ViewBag);
        }

        // Maybe this should be done in Web.config instead
        protected override HttpNotFoundResult HttpNotFound(string statusDescription)
        {
            throw new HttpException((int)HttpStatusCode.NotFound, statusDescription);
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
            if (!String.IsNullOrEmpty(Settings.ActiveSettings.DefaultLanguage))
                languages.Add(Settings.ActiveSettings.DefaultLanguage);
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