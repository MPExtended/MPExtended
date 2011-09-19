#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    public class PictureController : Controller
    {
        //
        // GET: /Pictures/
        public ActionResult Index()
        {
            return View(MPEServices.NetPipeMediaAccessService.GetPictureDirectory(@"\\whs\Fotos"));
        }

        public ActionResult Browse(string path)
        {
            return View(MPEServices.NetPipeMediaAccessService.GetPictureDirectory(new System.Text.ASCIIEncoding().GetString(Server.UrlTokenDecode(path))));
        }

        public ActionResult Image(string path)
        {
            byte[] image = System.IO.File.ReadAllBytes(new System.Text.ASCIIEncoding().GetString(Server.UrlTokenDecode(path)));
            return File(image, "image/jpg");
        }
    }
}
