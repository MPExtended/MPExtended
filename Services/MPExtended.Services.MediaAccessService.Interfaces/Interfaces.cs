using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public interface IActors
    {
        IList<WebActor> Actors { get; set; }
    }

    public interface IArtwork
    {
        IList<WebArtwork> Artwork { get; set; }
    }
}
