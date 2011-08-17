using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicTrackBasic : WebMediaItem
    {
        public WebMusicTrackBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
        }

        public string Id { get; set; }
        public string ArtistId { get; set; }
        public string Title { get; set; }
        public int TrackNumber { get; set; }
        public string Path { get; set; }
        public int Year { get; set; }
        public DateTime DateAdded { get; set; }

        public override string ToString()
        {
            return Title;
        }


        public WebMediaType Type
        {
            get
            {
                return WebMediaType.Music;
            }
            set
            {
                Type = value;
            }
        }
    }
}
