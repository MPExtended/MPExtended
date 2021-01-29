using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicTrackDetailed : WebMusicTrackBasic, IArtists, IAlbumArtist
  {
        public WebMusicTrackDetailed()
        {
            Artists = new List<WebMusicArtistBasic>();
            AlbumArtistObject = new WebMusicArtistBasic();
        }

        public IList<WebMusicArtistBasic> Artists { get; set; }
        public WebMusicArtistBasic AlbumArtistObject { get; set; }
    }
}
