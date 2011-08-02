using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Applications.WebMediaPortal.Services;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;
namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult NewEpisodes()
        {

            return PartialView();
        }
        public ActionResult NewMovies()
        {
            try
            {
                List<WebMovieFull> tmp = WebServices.MediaAccessService.GetMoviesDetailed(1, 4, SortBy.DateAdded, OrderBy.Desc);
                return PartialView(tmp);
            }
            catch (Exception ex)
            {
                Log.Error("Exception in Home.NewMovies" + ex.ToString(), ex);
            }
            return PartialView("Error");
        }
        public ActionResult NewRecordings()
        {
            try
            {
                List<WebRecording> tmp = WebServices.TVService.GetRecordings().OrderByDescending(p => p.StartTime).ToList();
                return PartialView(tmp.GetRange(0, tmp.Count / 10));
            }
            catch (Exception ex)
            {
                Log.Error("Exception in Home.NewMovies" + ex.ToString(), ex);
            }
            return PartialView("Error");
        }
        public ActionResult CurrentSchedules()
        {
            try
            {
                List<WebSchedule> tmp = WebServices.TVService.GetSchedules();
                return PartialView(tmp.Where(p => p.StartTime.Day == DateTime.Now.Day));


            }
            catch (Exception ex)
            {
                Log.Error("Exception in Home.NewMovies" + ex.ToString(), ex);
            }
            return PartialView("Error");
        }






    }
}
