using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMediaPortal.Services;

namespace WebMediaPortal.Controllers
{
    public class PictureController : Controller
    {
        //
        // GET: /Pictures/
        public ActionResult Index()
        {
            return View(WebServices.MediaAccessService.GetPictureDirectory(@"\\whs\Fotos"));
        }

        public ActionResult Browse(string path)
        { 
            return View(WebServices.MediaAccessService.GetPictureDirectory(new System.Text.ASCIIEncoding().GetString(Server.UrlTokenDecode(path))));
        }

        public ActionResult Image(string path)
        {
            byte[] image = System.IO.File.ReadAllBytes(new System.Text.ASCIIEncoding().GetString(Server.UrlTokenDecode(path)));
            return File(image, "image/jpg");
        }
    }
}
