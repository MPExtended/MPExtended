using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebSeason
    {
        public String Id { get; set; }
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodesCount { get; set; }
        public int EpisodesCountUnwatched { get; set; }
        public String SeasonBanner { get; set; }
        public String[] AlternateSeasonBanners { get; set; }

        public override string ToString()
        {
            return "Season " + SeasonNumber;
        }
    }
}