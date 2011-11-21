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
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    public class PictureController : BaseController
    {
        //
        // GET: /Pictures/
        public ActionResult Index()
        {
            var categories = MPEServices.MAS.GetAllPictureCategories(Settings.ActiveSettings.PicturesProvider);
            if (categories != null)
            {
                return View(categories);
            }
            return null;
        }

        public ActionResult Browse(string category)
        {          
            var images = MPEServices.MAS.GetPicturesBasicByCategory(Settings.ActiveSettings.PicturesProvider, category);
            if (images != null)
            {
                return View(images);
            }
            return null;
        }

        public ActionResult Image(string id)
        {         
            var image = MPEServices.MAS.RetrieveFile(Settings.ActiveSettings.PicturesProvider, WebMediaType.Picture, WebFileType.Content, id, 0);
            if (image != null)
            {
                return File(image, "image/jpg");
            }
            return null;
        }
    }
}
