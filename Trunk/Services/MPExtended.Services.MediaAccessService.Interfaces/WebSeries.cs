using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebSeries
    {
        public int Id { get; set; }
        public String PrettyName { get; set; }
        public int EpisodeCount { get; set; }
        public String ImdbId { get; set; }
        public double Rating { get; set; }
        public int RatingCount { get; set; }
        public String CurrentFanartUrl { get; set; }
        public String CurrentBannerUrl { get; set; }
        public String CurrentPosterUrl { get; set; }
        public String GenreString { get; set; }
        public String[] Genres { get; set; }
        public WebSeries() { }

    }
}