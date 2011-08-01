using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebMusicTrack
    {
        //MPDatabase Values
        public int TrackId { get; set; }
        public String[] Artists { get; set; }
        public String[] AlbumArtists { get; set; }
        public string ShortedAlbumArtist { get; set; }
        public int Duration { get; set; }

        //FileSystem Values
        public int TrackNum { get; set; }
        public int Year { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public string Title { get; set; }
        public string TrackNumber { get; set; }
        public string FilePath { get; set; }

        public WebMusicTrack()
        {
        }

        public WebMusicTrack(int trackNum, int year, string album, string genre, string[] albumArtists, string title, string filePath)
        {
            this.TrackNum = trackNum;
            this.Year = year;
            this.Album = album;
            this.Genre = genre;
            this.AlbumArtists = albumArtists;
            this.Title = title;
            this.FilePath = filePath;
        }

        public WebMusicTrack(int idTrack, string album, string[] artists, int trackno, string title, string file, int duration)
        {
            this.TrackId = idTrack;
            this.Album = album;
            this.Artists = artists;
            this.TrackNum = Convert.ToUInt16(trackno);
            this.Title = title;
            this.FilePath = file;
            this.Duration = duration;
        }
    }
}