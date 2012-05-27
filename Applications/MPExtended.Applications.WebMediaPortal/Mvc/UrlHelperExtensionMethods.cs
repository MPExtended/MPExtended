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
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public static class UrlHelperExtensionMethods
    {
        public static string VirtualViewContent(this UrlHelper helper, string viewPath)
        {
            return PathMapper.GetSkinVirtualViewPath(helper.RequestContext.HttpContext, viewPath);
        }

        public static string ViewContent(this UrlHelper helper, string viewPath)
        {
            return helper.Content(PathMapper.GetSkinVirtualViewPath(helper.RequestContext.HttpContext, viewPath));
        }

        public static string GenerateViewContentUrl(string viewPath, HttpContextBase httpContext)
        {
            return UrlHelper.GenerateContentUrl(PathMapper.GetSkinVirtualViewPath(httpContext, viewPath), httpContext);
        }

        public static string VirtualSkinContent(this UrlHelper helper, string contentPath)
        {
            return PathMapper.GetSkinVirtualContentPath(helper.RequestContext.HttpContext, contentPath);
        }

        public static string SkinContent(this UrlHelper helper, string contentPath)
        {
            return helper.Content(PathMapper.GetSkinVirtualContentPath(helper.RequestContext.HttpContext, contentPath));
        }

        public static string GenerateSkinContentUrl(string viewPath, HttpContextBase httpContext)
        {
            return UrlHelper.GenerateContentUrl(PathMapper.GetSkinVirtualContentPath(httpContext, viewPath), httpContext);
        }
    }
}