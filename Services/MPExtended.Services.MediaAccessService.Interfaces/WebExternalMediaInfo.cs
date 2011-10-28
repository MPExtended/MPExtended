using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public interface WebExternalMediaInfo
    {
        string Type { get; }
    }

    public class WebExternalMediaInfoId : WebExternalMediaInfo
    {
        public string Type { get; set; }
        public string Id { get; set; }
    }

    public class WebExternalMediaInfoAlbum : WebExternalMediaInfo
    {
        public string Type { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
    }

    public class WebExternalMediaInfoArtist : WebExternalMediaInfo
    {
        public string Type { get; set; }
        public string Artist { get; set; }
    }

    public class WebExternalMediaInfoFile : WebExternalMediaInfo
    {
        public string Type { get; set; }
        public string Path { get; set; }
    }

    public class WebExternalMediaInfoSeason : WebExternalMediaInfo
    {
        public string Type { get; set; }
        public int ShowId { get; set; }
        public int SeasonIndex { get; set; }
    }
}
