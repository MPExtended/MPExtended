using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicAlbumBasic
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
        public DateTime DateAdded { get; set; }
        public IList<string> Genre { get; set; }
        public int Year { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
