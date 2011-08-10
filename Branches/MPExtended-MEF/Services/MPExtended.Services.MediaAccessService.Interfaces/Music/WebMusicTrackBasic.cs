using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicTrackBasic
    {
        public string TrackId { get; set; }
        public string Title { get; set; }
        public int TrackNumber { get; set; }
        public string FilePath { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
