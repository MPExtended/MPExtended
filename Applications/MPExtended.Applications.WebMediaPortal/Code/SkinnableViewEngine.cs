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

namespace MPExtended.Applications.WebMediaPortal.Code
{
    internal class SkinnableViewEngine : RazorViewEngine
    {
        private string _skin;
        public string Skin
        {
            get
            {
                return _skin;
            }

            set
            {
                _skin = value;
                SetSkin(_skin);
            }
        }

        public string BaseDirectory
        {
            get
            {
                return "~/Skins/" + Skin;
            }
        }

        public SkinnableViewEngine(string skin)
        {
            this.Skin = skin;
        }

        protected void SetSkin(string skin)
        {
            FileExtensions = new string[] { 
                "cshtml", 
                "vbhtml" 
            };

            List<string> directories = new List<string>();
            directories.Add("~/Skins/" + skin);
            foreach (var plugin in Plugins.ListPlugins())
            {
                directories.Add("~/Plugins/" + plugin + "/Views");
            }
            directories.Add("~/Views");

            List<string> files = new List<string>();
            foreach (var dir in directories)
            {
                files.Add(dir + "/{1}/{0}.cshtml");
                files.Add(dir + "/{1}/{0}.vbhtml");
                files.Add(dir + "/Shared/{0}.cshtml");
                files.Add(dir + "/Shared/{0}.vbhtml");
            }

            MasterLocationFormats = files.ToArray();
            PartialViewLocationFormats = files.ToArray();
            ViewLocationFormats = files.ToArray();
        }

        public static string GetCurrentSkinDirectory(HttpContextBase context)
        {
            var relativePath = ViewEngines.Engines.OfType<SkinnableViewEngine>()
                .Select(sve => sve.BaseDirectory)
                .FirstOrDefault(sp => Directory.Exists(context.Server.MapPath(sp)));
            return relativePath == null ? "~/Views" : relativePath;
        }
    }
}