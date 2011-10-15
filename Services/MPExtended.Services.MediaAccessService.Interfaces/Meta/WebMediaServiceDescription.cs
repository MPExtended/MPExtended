using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Meta
{
    public class WebMediaServiceDescription
    {
        public int MovieApiVersion { get; set; }
        public int MusicApiVersion { get; set; }
        public int PicturesApiVersion { get; set; }
        public int TvShowsApiVersion { get; set; }
        public int FilesystemApiVersion { get; set; }

        public bool SupportsMovies { get; set; }
        public bool SupportsMusic { get; set; }
        public bool SupportsPictures { get; set; }
        public bool SupportsTvShows { get; set; }
        public bool SupportsFilesystem { get; set; }

        public string ServiceVersion { get; set; }
    }
}