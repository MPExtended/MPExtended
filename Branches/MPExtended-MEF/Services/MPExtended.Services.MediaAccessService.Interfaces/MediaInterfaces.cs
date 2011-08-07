using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    // each interface represents the structure of MEF plugins
    // A new MEF plugin has to implement one of these interfaces in order to be used by the service.

    public interface IMusicLibrary
    {
     
    }
    public interface IMovieLibrary
    {
        List<WebMovieBasic> GetAllMovies();
    }
    public interface ITVShowLibrary
    {
        
    }
    public interface IPictureLibrary
    {
        
    }
}
