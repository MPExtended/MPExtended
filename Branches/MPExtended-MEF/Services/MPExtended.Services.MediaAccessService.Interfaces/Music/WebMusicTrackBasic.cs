using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicTrackBasic : MediaItem
    {
        public string TrackId { get; set; }
        public string ArtistId { get; set; }
        public string Title { get; set; }
        public int TrackNumber { get; set; }
        public string FilePath { get; set; }
        public int Year { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
