using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicArtistBasic
    {
        public WebMusicArtistBasic(string title, string artistId)
        {
            Title = title;
            ArtistId = artistId;
        }
        public string ArtistId { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
