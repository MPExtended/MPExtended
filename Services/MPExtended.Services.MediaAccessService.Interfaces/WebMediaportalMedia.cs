using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public interface WebMediaportalMedia
    {
        string Type { get; }
    }

    public class WebMediaportalMediaId : WebMediaportalMedia
    {
        public string Type { get; set; }
        public string Id { get; set; }
    }

    public class WebMediaportalMediaAlbum : WebMediaportalMedia
    {
        public string Type { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
    }

    public class WebMediaportalMediaArtist : WebMediaportalMedia
    {
        public string Type { get; set; }
        public string Artist { get; set; }
    }

    public class WebMediaportalMediaFile : WebMediaportalMedia
    {
        public string Type { get; set; }
        public string Path { get; set; }
    }

    public class WebMediaportalMediaTVSeason : WebMediaportalMedia
    {
        public string Type { get; set; }
        public int ShowId { get; set; }
        public int SeasonIndex { get; set; }
    }
}
