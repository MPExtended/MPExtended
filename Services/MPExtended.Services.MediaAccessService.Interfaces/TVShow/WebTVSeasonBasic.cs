using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVSeasonBasic : ITitleSortable, IDateAddedSortable, IYearSortable, ITVSeasonNumberSortable
    {
        public WebTVSeasonBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
            BannerPaths = new List<string>();
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string ShowId { get; set; }
        public int SeasonNumber { get; set; }
        public bool IsProtected { get; set; }
        public int Year { get; set; }
        public int EpisodeCount { get; set; }
        public int UnwatchedEpisodeCount { get; set; }
        public DateTime DateAdded { get; set; }
        public IList<string> BannerPaths { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
