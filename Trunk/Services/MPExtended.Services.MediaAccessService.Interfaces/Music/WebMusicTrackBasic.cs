using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicTrackBasic : WebMediaItem, ITitleSortable, IDateAddedSortable, IYearSortable, IGenreSortable, IMusicTrackNumberSortable
    {
        public WebMusicTrackBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
        }

        public string Id { get; set; }
        public string ArtistId { get; set; }
        public string AlbumId { get; set; }
        public string Title { get; set; }
        public int TrackNumber { get; set; }
        public IList<string> Path { get; set; }
        public int Year { get; set; }
        public DateTime DateAdded { get; set; }
        public IList<string> Genres { get; set; }

        public WebMediaType Type
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
