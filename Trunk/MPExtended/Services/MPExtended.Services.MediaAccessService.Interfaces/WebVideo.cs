using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebVideo
    {
        public int VideoId;
        public string Genre;
        public string Title;
        public string Plot;
        public string File;

        public WebVideo() { }
        public WebVideo(int _movieId, string _genre, string _title, string _plot, string _file)
        {
            this.VideoId = _movieId;
            this.Genre = _genre;
            this.Title = _title;
            this.Plot = _plot;
            this.File = _file;
        }

    }
}