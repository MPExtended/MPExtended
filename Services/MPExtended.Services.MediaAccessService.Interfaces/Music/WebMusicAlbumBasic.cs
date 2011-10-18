using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicAlbumBasic : WebObject, ITitleSortable, IDateAddedSortable, IYearSortable, IGenreSortable, ICategorySortable, IMusicComposerSortable
    {
        public WebMusicAlbumBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
            Genres = new List<string>();
            Artists = new List<string>();
            ArtistsId = new List<string>();
            Composer = new List<string>();
            CoverPaths = new List<string>();
            UserDefinedCategories = new List<string>();
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public IList<string> Genres { get; set; }
        public string AlbumArtist { get; set; }
        public string AlbumArtistId { get; set; }
        public IList<string> Artists { get; set; }
        public IList<string> ArtistsId { get; set; }
        public IList<string> Composer { get; set; }
        public DateTime DateAdded { get; set; }
        public int Year { get; set; }
        public IList<string> CoverPaths { get; set; }
        public IList<string> UserDefinedCategories { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
