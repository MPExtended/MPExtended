using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using MPExtended.Services.MediaAccessService.Code;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.PlugIns.MAS.MPVideos
{
    [Export(typeof(IMovieLibrary))]
    [ExportMetadata("Database", "MPMyVideo")]
    public class MPVideos : IMovieLibrary
    {
        private MPVideoDB _db = null;
        public MPVideos()
        {
            _db = new MPVideoDB();
        }

        public IList<WebMovieBasic> GetAllMovies()
        {
            return _db.GetAllVideos();
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

        public System.IO.DirectoryInfo GetSourceRootDirectory()
        {
            throw new NotImplementedException();
        }
    }
}
