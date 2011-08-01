using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.ServiceModel;
using WebMediaPortal.Code;
using WebMediaPortal.Services;
using WebMediaPortal.Models;
using MPExtended.Services.MediaAccessService.Interfaces;


namespace WebMediaPortal.Controllers
{
    [Authorize]
    public class MovieLibraryController : Controller
    {
        //
        // GET: /MovieLibrary/



        public ActionResult Index()
        {

            try
            {
                var movieList = WebServices.MediaAccessService.GetAllMovies(SortBy.Name, OrderBy.Asc);
                if (movieList != null)
                {
                    return View(movieList);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MovieLibrary.Index", ex);
            }
            return View("Error");
        }

        public ActionResult Details(int movie)
        {

            try
            {
                var fullMovie = WebServices.MediaAccessService.GetFullMovie(movie);
                if (fullMovie != null)
                {
                    return View(fullMovie);
                }
            }
            catch (EndpointNotFoundException ex)
            {
           
            
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MovieLibrary.Details", ex);
                return View("Error");

            }
            return View("Error");

        }
        public ActionResult Play(int movie)
        {

            try
            {
                var fullMovie = WebServices.MediaAccessService.GetFullMovie(movie);
                if (fullMovie != null)
                {
                    return View(fullMovie);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MovieLibrary.Details", ex);
            }
            return View("Error");

        }

        public ActionResult Image(int movie)
        {
            try
            {
                var image = System.IO.File.ReadAllBytes(WebServices.MediaAccessService.GetFullMovie(movie).CoverPath);
                if (image != null)
                {
                    return File(image, "image/jpg");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MovieLibrary.Image", ex);
            }
            return null;
        }
    }
}
