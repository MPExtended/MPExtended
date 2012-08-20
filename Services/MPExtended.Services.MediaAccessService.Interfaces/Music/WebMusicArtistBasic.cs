using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicArtistBasic : WebObject, ITitleSortable, IArtwork
    {
        public WebMusicArtistBasic()
        {
            Artwork = new List<WebArtwork>();
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public IList<WebArtwork> Artwork { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
