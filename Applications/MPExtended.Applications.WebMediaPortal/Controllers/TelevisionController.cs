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
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.General;
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
            return View();
        }

        public ActionResult Menu()
        {
            return PartialView();
        }

        public ActionResult TVGuide()
        {
            var channelList = MPEServices.TAS.GetChannelsBasic(1);
            if (channelList != null)
            {
                return View(channelList);
            }
            return null;
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

        public ActionResult ProgramData(int channelId)
        {
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now.AddHours(4);

            var programsList = from p in MPEServices.TAS.GetProgramsDetailedForChannel(channelId, startTime, endTime)
                               select new Models.SingleTVProgramModel(p, startTime, endTime);

            return PartialView(programsList);
        }

        public ActionResult ProgramDetails(int programId)
        {
            var program = MPEServices.TAS.GetProgramBasicById(programId);
            if (program != null)
            {
                return View(program);
            }
            return null;
        }

        public ActionResult AddSchedule(int programId)
        {
            var program = MPEServices.TAS.GetProgramDetailedById(programId);
            MPEServices.TAS.AddSchedule(program.IdChannel, program.Title, program.StartTime, program.EndTime, 0);
            return RedirectToAction("ProgramDetails", "Television", new { programId = programId });
        }

        public ActionResult DeleteSchedule(int programId)
        {
            var program = MPEServices.TAS.GetProgramDetailedById(programId);
            int id = MPEServices.TAS.GetSchedules().Where(p => p.IdChannel == program.IdChannel && p.StartTime == program.StartTime && p.EndTime == program.EndTime).First().Id;
            MPEServices.TAS.DeleteSchedule(id);
            return RedirectToAction("ProgramDetails", "Television", new { programId = programId });
        }

        public ActionResult DeleteScheduleById(int scheduleId)
        {
            MPEServices.TAS.DeleteSchedule(scheduleId);
            return RedirectToAction("TVGuide", "Television");
        }

        public ActionResult ScheduleDetails(int scheduleId)
        {
            var schedule = MPEServices.TAS.GetScheduleById(scheduleId);
            if (schedule != null)
            {
                return View(schedule);
            }
            return null;
        }
    }
}
