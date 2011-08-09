using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicAlbumBasic
    {
        public String AlbumId { get; set; }
        public String Title { get; set; }
        public String Genre { get; set; }
        public String Artist { get; set; }
        public String AlbumArtist { get; set; }

        public String Composer { get; set; }
        public String Publisher { get; set; } 
        public int Year { get; set; }
    }
}
