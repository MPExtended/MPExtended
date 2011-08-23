using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicTrackDetailed : WebMusicTrackBasic
    {
        public WebMusicArtistBasic Artist { get; set; }
    }
}
