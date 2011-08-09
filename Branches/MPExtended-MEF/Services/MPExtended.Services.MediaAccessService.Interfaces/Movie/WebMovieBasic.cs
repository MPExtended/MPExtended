using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Movie
{
    public class WebMovieBasic
    {
        public WebMovieBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
        }

        public String MovieId { get; set; }
        public String Title { get; set; }
        public String Genre { get; set; }
        public int Year { get; set; }
        public String CoverThumbPath { get; set; }
        public String BackdropPath { get; set; }
        public DateTime DateAdded { get; set; }
        public string FileName { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }

}
