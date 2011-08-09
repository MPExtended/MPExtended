using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicTrackBasic
    {
        public String TrackId { get; set; }
        public String Title { get; set; }
        public int TrackNumber { get; set; }
        public string FileName { get; set; }
    }
}
