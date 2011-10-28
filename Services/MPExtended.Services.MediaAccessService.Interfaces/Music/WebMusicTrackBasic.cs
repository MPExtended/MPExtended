using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicTrackBasic : WebMediaItem, ITitleSortable, IYearSortable, IGenreSortable, IMusicTrackNumberSortable
    {
        public WebMusicTrackBasic()
        {
            ArtistId = new List<string>();
            Genres = new List<string>();
        }

        public IList<string> Artist { get; set; }
        public IList<string> ArtistId { get; set; }
        public string Album { get; set; }
        public string AlbumId { get; set; }
        public string Title { get; set; }
        public int TrackNumber { get; set; }
        public int Year { get; set; }
        public IList<string> Genres { get; set; }

        public override WebMediaType Type
        {
            get
            {
                return WebMediaType.MusicTrack;
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
