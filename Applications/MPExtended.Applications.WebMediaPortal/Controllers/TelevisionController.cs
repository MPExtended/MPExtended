#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class TelevisionController : BaseController
    {
        //
        // GET: /Television/
        public ActionResult Index()
        {
            return TVGuide();
        }

        public ActionResult TVGuide(int? group = null, string time = null, string date = null)
        {
            DateTime startTime;
            if (time == null || !DateTime.TryParse(time, out startTime))
            {
                startTime = DateTime.Now;
            }

            DateTime passedDate;
            if (date != null && DateTime.TryParse(date, out passedDate))
            {
                startTime = new DateTime(passedDate.Year, passedDate.Month, passedDate.Day, startTime.Hour, startTime.Minute, startTime.Second);
            }

            var lastHour = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute >= 30 ? 30 : 0, 0, DateTimeKind.Local);
            var endOfGuide = lastHour.AddHours(4);

            var groups = Connections.Current.TAS.GetGroups();
            var activeGroup = Connections.Current.TAS.GetGroupById(group != null ? group.Value : Settings.ActiveSettings.DefaultGroup.GetValueOrDefault(groups.First().Id));
            if (activeGroup == null)
            {
                activeGroup = Connections.Current.TAS.GetGroupById(groups.First().Id);
            }

            var model = new TVGuideViewModel(groups, activeGroup, lastHour, endOfGuide);
            return View(model);
        }

        public ActionResult ChannelLogo(int channelId, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.TV, channelId.ToString(), WebFileType.Logo, width, height, "Images/default/logo.png");
        }

        public ActionResult ProgramDetails(int programId)
        {
            var program = Connections.Current.TAS.GetProgramDetailedById(programId);
            if (program == null)
                return HttpNotFound();
            return View(new ProgramDetailsViewModel(program));
        }

        public ActionResult WatchLiveTV(int channelId)
        {
            var channel = Connections.Current.TAS.GetChannelDetailedById(channelId);
            if (channel == null)
                return HttpNotFound();
            return View(channel);
        }

        public ActionResult Recordings()
        {
            var recordings = Connections.Current.TAS.GetRecordings();
            if (recordings == null)
                return HttpNotFound();
            return View(recordings);
        }

        public ActionResult Recording(int id)
        {
            var rec = Connections.Current.TAS.GetRecordingById(id);
            if (rec == null)
                return HttpNotFound();

            var fileInfo = Connections.Current.TAS.GetRecordingFileInfo(rec.Id);
            var mediaInfo = Connections.Current.TASStreamControl.GetMediaInfo(WebMediaType.Recording, null, rec.Id.ToString(), 0);
            ViewBag.Quality = MediaInfoFormatter.GetFullInfoString(mediaInfo, fileInfo);
            return View(rec);
        }

        public ActionResult RecordingPreview(int id, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.Recording, id.ToString(), WebFileType.Content, width, height, "Images/default/recording.png");
        }


        public ActionResult WatchRecording(int id)
        {
            var rec = Connections.Current.TAS.GetRecordingById(id);
            return View(rec);
        }
    }
}
