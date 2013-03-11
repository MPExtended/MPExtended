#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Applications.WebMediaPortal.Mvc;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    internal class SkinnableViewEngine : RazorViewEngine
    {
        public SkinnableViewEngine()
        {
            UpdateActiveSkin();
        }

        public void UpdateActiveSkin()
        {
            FileExtensions = new string[] { 
                "cshtml", 
                "vbhtml" 
            };

            List<string> files = new List<string>();
            foreach (var directory in ContentLocator.Current.ViewDirectories)
            {
                files.Add(directory + "/{1}/{0}.cshtml");
                files.Add(directory + "/{1}/{0}.vbhtml");
                files.Add(directory + "/Shared/{0}.cshtml");
                files.Add(directory + "/Shared/{0}.vbhtml");
            }

            MasterLocationFormats = files.ToArray();
            PartialViewLocationFormats = files.ToArray();
            ViewLocationFormats = files.ToArray();
        }
    }
}