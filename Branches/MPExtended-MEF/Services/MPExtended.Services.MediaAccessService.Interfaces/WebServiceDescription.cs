using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebServiceDescription
    {
        public bool SupportsMovies { get; set; }
        public bool SupportsMusic { get; set; }
        public bool SupportsPictures { get; set; }
        public bool SupportsTvShows{ get; set; }
        public List<string> AvailableMovieProvider { get; set; }
        public List<string> AvailableMusicProvider { get; set; }
        public List<string> AvailablePictureProvider { get; set; }
        public List<string> AvailableTvShowProvider { get; set; }

        #region API Versions
        public int MovieApiVersion { get; set; }
        public int MusicApiVersion { get; set; }
        public int PicturesApiVersion { get; set; }
        public int TvShowsApiVersion { get; set; }
        public int FilesystemApiVersion { get; set; }
        #endregion

        public String ServiceVersion { get; set; }
    }
}