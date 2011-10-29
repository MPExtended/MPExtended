using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Movie
{
    public class WebMovieDetailed : WebMovieBasic
    {
        public WebMovieDetailed() : base()
        {
            Directors = new List<string>();
            Writers = new List<string>();
            Actors = new List<string>();
        }

        public IList<string> Directors { get; set; }
        public IList<string> Writers { get; set; }
        public IList<string> Actors { get; set; }
        public string Summary { get; set; }       
    
        // use ISO short name (en, nl, de, etc)
        public string Language { get; set; }
    }
}
