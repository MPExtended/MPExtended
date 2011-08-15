using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel.Composition;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;


namespace MPExtended.PlugIns.MAS.MovingPictures

{
    [Export(typeof(IMovieLibrary))]
    [ExportMetadata("Database","MovingPictures")]
    public class MPMovingPictures : IMovieLibrary
    {

        public IList<WebMovieBasic> GetAllMovies()
        {
            throw new NotImplementedException();
        }

        public IList<WebMovieDetailed> GetAllMoviesDetailed()
        {
            throw new NotImplementedException();
        }

        public WebMovieBasic GetMovieBasicById(string movieId)
        {
            throw new NotImplementedException();
        }

        public WebMovieDetailed GetMovieDetailedById(string movieId)
        {
            throw new NotImplementedException();
        }

        public IList<string> GetAllGenres()
        {
            throw new NotImplementedException();
        }

        public DirectoryInfo GetSourceRootDirectory()
        {
            throw new NotImplementedException();
        }
    }


}
