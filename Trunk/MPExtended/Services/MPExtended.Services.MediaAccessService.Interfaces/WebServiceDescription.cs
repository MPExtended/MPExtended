using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebServiceDescription
    {
        public bool SupportsVideos { get; set; }
        public bool SupportsMusic { get; set; }
        public bool SupportsPictures { get; set; }
        public bool SupportsTvSeries { get; set; }
        public bool SupportsMovingPictures { get; set; }
        public bool SupportsMyFilms { get; set; }

        #region API Versions
        public int VideoApiVersion { get; set; }
        public int MusicApiVersion { get; set; }
        public int PicturesApiVersion { get; set; }
        public int TvSeriesApiVersion { get; set; }
        public int MovingPicturesApiVersion { get; set; }
        public int MyFilmsApiVersion { get; set; }
        public int StreamingApiVersion { get; set; }
        #endregion

        public String ServiceVersion { get; set; }

        public WebServiceDescription()
        {
            SupportsVideos = false;
            SupportsMusic = false;
            SupportsPictures = false;
            SupportsTvSeries = false;
            SupportsMovingPictures = false;
            SupportsMyFilms = false;
        }
    }
}