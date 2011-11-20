using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebArtwork
    {
        public WebFileType Type { get; set; }
        public string Id { get; set; }
        public int Rating { get; set; }
        public string Filetype { get; set; }
        public int Offset { get; set; }
    }

    public class WebArtworkDetailed : WebArtwork
    {
        public string Path { get; set; }
    }
}
