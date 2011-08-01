using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebMusicAlbum
    {
        // artists
        public String[] AlbumArtists { get; set; }
        public String[] Artists { get; set; }
        public string ShortedAlbumArtist { get; set; }

        public string Title { get; set; }
        public uint Year { get; set; } // this one is probably empty in the database
        public string Genre { get; set; } // this one too
        public string Composer { get; set; } // TODO: never filled
        public string Publisher { get; set; } // TODO: never filled
        public string CoverPathL { get; set; }
        public string CoverPath { get; set; } // TODO: never filled nor used?

        public WebMusicAlbum(string artists, string title, uint year, string genre, string composer, string publisher)
        {
            this.Artists = Artists;
            this.Title = title;
            this.Year = year;
            this.Genre = genre;
            this.Composer = composer;
            this.Publisher = publisher;
        }

        public WebMusicAlbum()
        { 
        }
    }
}