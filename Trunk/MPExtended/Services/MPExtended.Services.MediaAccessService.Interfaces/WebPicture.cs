using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebPicture
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Comment { get; set; }
        public string DateTaken { get;  set; }
        public string CameraManufacturer { get;  set; }
        public string CameraModel { get;  set; }
        public string Copyright { get; set; }
        public double Mpixel { get;  set; }
        public string Height { get;  set; }
        public string Width { get;  set; }
        public string Dpi { get;  set; }
        public string Author { get; set; }
        public string Rating { get; set; }
        public string Filename { get; set; }

      
    }
}