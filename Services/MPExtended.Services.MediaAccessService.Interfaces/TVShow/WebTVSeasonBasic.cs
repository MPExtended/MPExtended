using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVSeasonBasic : WebObject, ITitleSortable, IDateAddedSortable, IYearSortable, ITVSeasonNumberSortable, IArtwork
    {
        public WebTVSeasonBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
            Artwork = new List<WebArtwork>();
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
        public IList<WebArtwork> Artwork { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
