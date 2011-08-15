using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Movie
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
}
