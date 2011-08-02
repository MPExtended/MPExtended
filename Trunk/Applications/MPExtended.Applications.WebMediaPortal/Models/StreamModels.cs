using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using MPExtended.Services.StreamingService.Interfaces;


namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class StreamModel
    {
        public WebResolution Size { get; set; }
        public string URL { get; set; }
    }

    public enum VideoPlayer
    {
        Flash,
        VLC
    }

    public enum StreamMedia
    {
        TV = -1,
        Movie = WebMediaType.MovieItem,
        Serie = WebMediaType.TvSeriesItem,
        Recording = WebMediaType.RecordingItem,
        Music = WebMediaType.MusicTrackItem
    }
}