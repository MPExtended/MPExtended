using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicAlbumBasic : ITitleSortable, IYearSortable, IMusicComposerSortable
    {
        public WebMusicAlbumBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public IList<string> Genres { get; set; }
        public string AlbumArtist { get; set; }
        public IList<string> Artists { get; set; }
        public IList<string> Composer { get; set; }
        public string ArtistId { get; set; }
        public DateTime DateAdded { get; set; }
        public int Year { get; set; }
        public string CoverPath { get; set; }
        public IList<string> UserDefinedCategories { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
