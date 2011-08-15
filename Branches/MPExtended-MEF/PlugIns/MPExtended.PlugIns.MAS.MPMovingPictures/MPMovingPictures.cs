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
        private MovingPicturesDB _db = null;
        public MPMovingPictures()
        {
            _db = new MovingPicturesDB();
        }

        public List<WebMovieBasic> GetAllMovies()
        {
          return  _db.GetAllMovies();
        }

        public IList<WebMovieDetailed> GetAllMoviesDetailed()
        {
            return _db.GetAllMoviesDetailed();
        }

        public WebMovieBasic GetMovieBasicById(string movieId)
        {
            throw new NotImplementedException();
        }

        public WebMovieDetailed GetMovieDetailedById(string movieId)
        {
            return _db.GetFullMovie(movieId);
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
