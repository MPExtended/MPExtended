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
    internal static class PathMapper
    {
        public static string GetSkinVirtualViewPath(HttpContextBase context, string path)
        {
            var relativePath = ViewEngines.Engines.OfType<SkinnableViewEngine>()
                .Select(sve => sve.BaseDirectory + "/" + path)
                .FirstOrDefault(sp => File.Exists(context.Server.MapPath(sp)));
            if (relativePath == null)
            {
                relativePath = "~/Views/" + path;
            }
            return relativePath;
        }

        public static string GetSkinVirtualContentPath(HttpContextBase context, string path)
        {
            var relativePath = ViewEngines.Engines.OfType<SkinnableViewEngine>()
                .Select(sve => sve.BaseDirectory + "/Content/" + path)
                .FirstOrDefault(sp => File.Exists(context.Server.MapPath(sp)));
            if (relativePath == null)
            {
                relativePath = "~/Content/" + path;
            }
            return relativePath;
        }
    }
}