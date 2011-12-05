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
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.General;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [Authorize]
    public class TelevisionController : BaseController
    {
        //
        // GET: /Television/
        public ActionResult Index()
        {
            return TVGuide();
        }

        public ActionResult TVGuide()
        {
            var lastHour = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0, DateTimeKind.Local);
            var endOfGuide = lastHour.AddHours(4);

            var model = new TVGuideViewModel(MPEServices.TAS.GetGroupById(1), lastHour, endOfGuide);
            return View(model);
        }

        public ActionResult ChannelLogo(int channelId)
        {
            var logo = MPEServices.TASStream.GetArtwork(WebStreamMediaType.TV, null, channelId.ToString(), WebArtworkType.Logo, 0);
            return File(logo, "image/png");
        }

        public ActionResult ProgramDetails(int programId)
        {
            var program = MPEServices.TAS.GetProgramBasicById(programId);
            if (program == null)
            {
                return HttpNotFound();
            }
            return View(new ProgramDetailsViewModel(program));
        }

        public ActionResult DeleteSchedule(int programId)
        {
            var program = MPEServices.TAS.GetProgramDetailedById(programId);
            int id = MPEServices.TAS.GetSchedules().Where(p => p.IdChannel == program.IdChannel && p.StartTime == program.StartTime && p.EndTime == program.EndTime).First().Id;
            MPEServices.TAS.DeleteSchedule(id);
            return RedirectToAction("ProgramDetails", "Television", new { programId = programId });
        }

        public ActionResult AddSchedule(int programId)
        {
            var program = MPEServices.TAS.GetProgramDetailedById(programId);
            if (program == null)
            {
                return HttpNotFound();
            }

            return View(new AddScheduleViewModel(program));
        }

        [HttpPost]
        public ActionResult AddSchedule(FormCollection fc, int programId)
        {
            WebScheduleType type;
            if (!Enum.TryParse<WebScheduleType>(fc["type"], out type))
            {
                // TODO: error message
                type = WebScheduleType.Once;
            }

            var program = MPEServices.TAS.GetProgramDetailedById(programId);
            if (program == null)
            {
                return HttpNotFound();
            }

            MPEServices.TAS.AddSchedule(program.IdChannel, program.Title, program.StartTime, program.EndTime, type);
            return RedirectToAction("ProgramDetails", new { programId = programId });
        }

        public ActionResult Schedules()
        {
            var list = MPEServices.TAS.GetSchedules(SortField.Name, SortOrder.Asc).Select(x => new ScheduleViewModel(x));
            return View(list);
        }

        public ActionResult DeleteScheduleById(int id)
        {
            MPEServices.TAS.DeleteSchedule(id);
            return RedirectToAction("Schedules");
        }

        public ActionResult WatchLiveTV(int channelId)
        {
            var channel = MPEServices.TAS.GetChannelDetailedById(channelId);
            if (channel != null)
            {
                return View(channel);
            }
            return null;
        }

        public ActionResult WatchRecording(int recordingId)
        {
            var rec = MPEServices.TAS.GetRecordingById(recordingId);
            return View(rec);
        }

        public ActionResult Recordings()
        {
            var recordings = MPEServices.TAS.GetRecordings();
            if (recordings != null)
            {
                return View(recordings);
            }
            return null;
        }
    }
}
