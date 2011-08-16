using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebSeriesFull : WebSeries
    {
        public String SortName { get; set; }
        public String OrigName { get; set; }
        public String Status { get; set; }
        public String[] BannerUrls { get; set; }
        public String[] FanartUrls { get; set; }
        public String[] PosterUrls { get; set; }
        public String ContentRating { get; set; }
        public String Network { get; set; }
        public String Summary { get; set; }
        public String AirsDay { get; set; }
        public String AirsTime { get; set; }
        public String[] Actors { get; set; }
        public int Runtime { get; set; }
        public DateTime FirstAired { get; set; }
        public String EpisodeOrder { get; set; }

    }
}