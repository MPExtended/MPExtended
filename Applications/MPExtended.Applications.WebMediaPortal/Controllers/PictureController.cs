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
    public class PictureController : Controller
    {
        //
        // GET: /Pictures/
        public ActionResult Index()
        {
            try
            {
                var categories = MPEServices.NetPipeMediaAccessService.GetAllPictureCategoriesBasic(Settings.ActiveSettings.PicturesProvider);
                if (categories != null)
                {
                    return View(categories);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in PictureLibrary.Browse", ex);
            }
            return null;
        }

        public ActionResult Browse(string category)
        {          

            try
            {
                var images = MPEServices.NetPipeMediaAccessService.GetPicturesBasicByCategory(Settings.ActiveSettings.PicturesProvider, category);
                if (images != null)
                {
                    return View(images);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in PictureLibrary.Browse", ex);
            }
            return null;
        }

        public ActionResult Image(string id)
        {         
            try
            {
                var image = MPEServices.NetPipeMediaAccessService.RetrieveFile(Settings.ActiveSettings.PicturesProvider, WebMediaType.Picture, WebFileType.Content, id, 0);
                if (image != null)
                {
                    return File(image, "image/jpg");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in PictureLibrary.Image", ex);
            }
            return null;
        }
    }
}
