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

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public static class HtmlHelperExtensionMethods
    {
        private const string assetManagerKey = "assetManager";
        private const string jQueryHelperKey = "jQueryHelper";

        public static AssetManager Assets(this HtmlHelper htmlHelper)
        {
            var manager = htmlHelper.ViewContext.HttpContext.Items[assetManagerKey] as AssetManager;
            if (manager == null)
            {
                manager = new AssetManager(htmlHelper);
                htmlHelper.ViewContext.HttpContext.Items[assetManagerKey] = manager;
            }
            return manager;
        }

        public static JQueryHelper jQuery(this HtmlHelper htmlHelper)
        {
            var helper = htmlHelper.ViewContext.HttpContext.Items[jQueryHelperKey] as JQueryHelper;
            if (helper == null)
            {
                helper = new JQueryHelper(htmlHelper);
                htmlHelper.ViewContext.HttpContext.Items[jQueryHelperKey] = helper;
            }
            return helper;
        }
    }
}