using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ServiceModel;
using WebMediaPortal.Services;
using WebMediaPortal.Code;
using MPExtended.Services.TVAccessService.Interfaces;

namespace WebMediaPortal.Controllers
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
                var channelList = WebServices.TVService.GetChannelsBasic(1);
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
                var channel = WebServices.TVService.GetChannelDetailedById(channelId);
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
                IEnumerable<WebRecording> recordings = WebServices.TVService.GetRecordings().Where(r => r.IdRecording == recordingId);
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
                var recordings = WebServices.TVService.GetRecordings();
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

            var programsList = from p in WebServices.TVService.GetProgramsDetailedForChannel(channelId, startTime, endTime)
                               select new Models.SingleTVProgramModel(p, startTime, endTime);

            return PartialView(programsList);
        }

        public ActionResult ProgramDetails(int programId)
        {
            try
            {
                var program = WebServices.TVService.GetProgramBasicById(programId);
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
            var program = WebServices.TVService.GetProgramDetailedById(programId);
            WebServices.TVService.AddScheduleDetailed(program.IdChannel, program.Title, program.StartTime, program.EndTime, 0, 10, 15, "", 0);
            return RedirectToAction("ProgramDetails", "Television", new { programId = programId });
        }
        public ActionResult DeleteSchedule(int programId)
        {
            var program = WebServices.TVService.GetProgramDetailedById(programId);
            int i = WebServices.TVService.GetSchedules().Where(p => p.IdChannel == program.IdChannel && p.StartTime == program.StartTime && p.EndTime == program.EndTime).ElementAt(0).IdSchedule;
            WebServices.TVService.DeleteSchedule(i);
            return RedirectToAction("ProgramDetails", "Television", new { programId = programId });
        }

        public ActionResult DeleteScheduleById(int scheduleId)
        {

            WebServices.TVService.DeleteSchedule(scheduleId);
            return RedirectToAction("TVGuide", "Television");
        }



        public ActionResult ScheduleDetails(int scheduleId)
        {
            try
            {
                var schedule = WebServices.TVService.GetScheduleById(scheduleId);
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
