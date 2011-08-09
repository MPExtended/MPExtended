using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Movie
{
    public class WebMovieDetailed : WebMovieBasic
    {
        public IList<String> Directors { get; set; }
        public IList<String> Writers { get; set; }
        public IList<String> Actors { get; set; }
        //may be use Language enum?
        public String Language { get; set; }
    }
}
