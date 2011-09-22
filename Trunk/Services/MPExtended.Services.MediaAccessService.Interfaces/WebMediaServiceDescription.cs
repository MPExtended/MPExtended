using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebMediaServiceDescription
    {
        public bool SupportsMovies { get; set; }
        public bool SupportsMusic { get; set; }
        public bool SupportsPictures { get; set; }
        public bool SupportsTvShows{ get; set; }
        public List<WebBackendProvider> AvailableMovieProvider { get; set; }
        public List<WebBackendProvider> AvailableMusicProvider { get; set; }
        public List<WebBackendProvider> AvailablePictureProvider { get; set; }
        public List<WebBackendProvider> AvailableTvShowProvider { get; set; }

        #region API Versions
        public int MovieApiVersion { get; set; }
        public int MusicApiVersion { get; set; }
        public int PicturesApiVersion { get; set; }
        public int TvShowsApiVersion { get; set; }
        public int FilesystemApiVersion { get; set; }
        #endregion

        public string ServiceVersion { get; set; }
    }
}