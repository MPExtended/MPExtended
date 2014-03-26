using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicArtistDetailed : WebMusicArtistBasic
    {
        public string Biography { get; set; }
        public string Tones { get; set; }
        public string Styles { get; set; }

        public IList<string> Genres { get; set; }
    }
}
