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

        public string MovieId { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public bool IsProtected { get; set; }
        public int Year { get; set; }
        public string CoverThumbPath { get; set; }
        public string BackdropPath { get; set; }
        public string CoverPath { get; set; }
        public DateTime DateAdded { get; set; }
        public string FileName { get; set; }
        public int Rating { get; set; }
    

        public override string ToString()
        {
            return Title;
        }
    }

}
