using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebMovie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TagLine { get; set; }
        public string Genre { get; set; }
        public int Year { get; set; }
        public string CoverThumbPath { get; set; }
        public string BackdropPath { get; set; }
    }
}