#region Copyright (C) 2012-2013 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class RecordingController : BaseController
    {
        //
        // GET: /Recording/
        public ActionResult Index()
        {
            var recordings = Connections.Current.TAS.GetRecordings();
            if (recordings == null)
                return HttpNotFound();
            return View(recordings);
        }

        public ActionResult Details(int id)
        {
            var rec = Connections.Current.TAS.GetRecordingById(id);
            if (rec == null)
                return HttpNotFound();

            var fileInfo = Connections.Current.TAS.GetRecordingFileInfo(rec.Id);
            var mediaInfo = Connections.Current.TASStreamControl.GetMediaInfo(WebMediaType.Recording, null, rec.Id.ToString(), 0);
            ViewBag.Quality = MediaInfoFormatter.GetFullInfoString(mediaInfo, fileInfo);
            ViewBag.Resolution = MediaInfoFormatter.GetShortQualityName(mediaInfo);
            return View(rec);
        }

        public ActionResult PreviewImage(int id, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.Recording, id.ToString(), WebFileType.Content, width, height, "Images/default/recording.png");
        }

        public ActionResult Watch(int id)
        {
            var rec = Connections.Current.TAS.GetRecordingById(id);
            if (rec == null)
                return HttpNotFound();
            return View(rec);
        }
        
        public ActionResult DeleteRecording(int id)
        {
            var rec = Connections.Current.TAS.GetRecordingById(id);
            if (rec == null)
                return HttpNotFound();
            Connections.Current.TAS.DeleteRecording(rec.Id);
            return RedirectToAction("Index", "Recording");
        }        
    }
}
