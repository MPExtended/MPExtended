using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicAlbumBasic
    {
        public string AlbumId { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Artist { get; set; }
        public string AlbumArtist { get; set; }

        public string Composer { get; set; }
        public string Publisher { get; set; } 
        public int Year { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
