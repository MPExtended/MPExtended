using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebMovieFull : WebMovie
    {
        public class WebMovieFile
        {
            public string Filename { get; set; }
            public string DiscId { get; set; }
            public string Hash { get; set; }
            public int Part { get; set; }
            public int Duration { get; set; }
            public int VideoWidth { get; set; }
            public int VideoHeight { get; set; }
            public string VideoResolution { get; set; }
            public string VideoCodec { get; set; }
            public string AudioCodec { get; set; }
            public string AudioChannels { get; set; }
            public bool HasSubtitles { get; set; }
            public string VideoFormat { get; set; }
        }

        public String AlternateTitles { get; set; }
        public String SortBy { get; set; }
        public String Directors { get; set; }
        public String Writers { get; set; }
        public String Actors { get; set; }
        public String Certification { get; set; }
        public String Language { get; set; }
        public String Summary { get; set; }
        public double Score { get; set; }
        public int Popularity { get; set; }
        public DateTime DateAdded { get; set; }
        public int Runtime { get; set; }
        public String ImdbId { get; set; }
        public String CoverPathAlternate { get; set; }
        public String CoverPath { get; set; }

        public int Parts { get; set; }
        public List<WebMovieFile> Files { get; set; }

        public WebMovieFull()
        {
            Files = new List<WebMovieFile>();
            DateAdded = new DateTime(1970, 1, 1);
        }
    }
}