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
    public class TelevisionController : Controller
    {

        //
        // GET: /Television/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TVGuide()
        {
            try
            {
                var channelList = MPEServices.NetPipeTVAccessService.GetChannelsBasic(1);
                if (channelList != null)
                {
                    return View(channelList);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in Television.TVGuide", ex);
            }
            return View("Error");
        }

        public ActionResult WatchLiveTV(int channelId)
        {

            try
            {
                var channel = MPEServices.NetPipeTVAccessService.GetChannelDetailedById(channelId);
                if (channel != null)
                {
                    return View(channel);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in Television.WatchLiveTV", ex);
            }
            return View("Error");
        }

        public ActionResult WatchRecording(int recordingId)
        {
            try
            {
                IEnumerable<WebRecordingBasic> recordings = MPEServices.NetPipeTVAccessService.GetRecordings().Where(r => r.Id == recordingId);
                if (recordings.Count() > 0)
                {
                    return View(recordings.First());
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in Television.WatchRecording", ex);
            }
            return View("Error");
        }

        public ActionResult Recordings()
        {
            try
            {
                var recordings = MPEServices.NetPipeTVAccessService.GetRecordings();
                if (recordings != null)
                {
                    return View(recordings);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in Television.Recordings", ex);
            }
            return View("Error");
        }

        public ActionResult Menu()
        {
            return PartialView();
        }

        public ActionResult ProgramData(int channelId)
        {
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now.AddHours(4);

            var programsList = from p in MPEServices.NetPipeTVAccessService.GetProgramsDetailedForChannel(channelId, startTime, endTime)
                               select new Models.SingleTVProgramModel(p, startTime, endTime);

            return PartialView(programsList);
        }

        public ActionResult ProgramDetails(int programId)
        {
            try
            {
                var program = MPEServices.NetPipeTVAccessService.GetProgramBasicById(programId);
                if (program != null)
                {
                    return View(program);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in Television.ProgramDetails", ex);
            }
            return View("Error");
        }

        public ActionResult AddSchedule(int programId)
        {
            var program = MPEServices.NetPipeTVAccessService.GetProgramDetailedById(programId);
            MPEServices.NetPipeTVAccessService.AddScheduleDetailed(program.IdChannel, program.Title, program.StartTime, program.EndTime, 0, 10, 15, "", 0);
            return RedirectToAction("ProgramDetails", "Television", new { programId = programId });
        }

        public ActionResult DeleteSchedule(int programId)
        {
            var program = MPEServices.NetPipeTVAccessService.GetProgramDetailedById(programId);
            int i = MPEServices.NetPipeTVAccessService.GetSchedules().Where(p => p.IdChannel == program.IdChannel && p.StartTime == program.StartTime && p.EndTime == program.EndTime).ElementAt(0).Id;
            MPEServices.NetPipeTVAccessService.DeleteSchedule(i);
            return RedirectToAction("ProgramDetails", "Television", new { programId = programId });
        }

        public ActionResult DeleteScheduleById(int scheduleId)
        {

            MPEServices.NetPipeTVAccessService.DeleteSchedule(scheduleId);
            return RedirectToAction("TVGuide", "Television");
        }

        public ActionResult ScheduleDetails(int scheduleId)
        {
            try
            {
                var schedule = MPEServices.NetPipeTVAccessService.GetScheduleById(scheduleId);
                if (schedule != null)
                {
                    return View(schedule);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in Television.ScheduleDetails", ex);
            }
            return View("Error");
        }
    }
}
