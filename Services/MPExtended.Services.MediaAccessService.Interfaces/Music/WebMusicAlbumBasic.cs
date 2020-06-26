using MPExtended.Services.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicAlbumBasic : WebObject, IRatingSortable, ITitleSortable, IDateAddedSortable, IYearSortable, IGenreSortable, IMusicComposerSortable, IArtwork
    {
        public WebMusicAlbumBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
            Genres = new List<string>();
            Artists = new List<string>();
            ArtistsId = new List<string>();
            Composer = new List<string>();
            Artwork = new List<WebArtwork>();
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
        public float Rating { get; set; }
        public IList<WebArtwork> Artwork { get; set; }

        public WebMediaType Type 
        {
            get
            {
                return WebMediaType.MusicAlbum;
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
