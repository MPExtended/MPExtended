using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.Composition;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.PlugIns.MAS
{
    [Export(typeof(IMovieLibrary))]
    public class MPMovingPictures : IMovieLibrary
    {
        public List<WebMovieBasic> GetAllMovies()
        {
            List<WebMovieBasic> list = new List<WebMovieBasic>();
          
            return list;
        }
    }


}
