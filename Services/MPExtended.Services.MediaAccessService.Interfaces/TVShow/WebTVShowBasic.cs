using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVShowBasic : WebObject, ITitleSortable, IDateAddedSortable, IYearSortable, IGenreSortable, ICategorySortable, IRatingSortable
    {
        public WebTVShowBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
            BannerPaths = new List<string>();
            UserDefinedCategories = new List<string>();
            Genres = new List<string>();
        }

        public string Id { get; set; }
        public bool IsProtected { get; set; }
        public DateTime DateAdded { get; set; }
        public IList<string> UserDefinedCategories { get; set; }
        public IList<string> Genres { get; set; }
        public IList<string> BannerPaths { get; set; }

        public string Title { get; set; }
        public int Year { get; set; }
        public int EpisodeCount { get; set; }
        public int UnwatchedEpisodeCount { get; set; }
        public float Rating { get; set; }
        public string ContentRating { get; set; }
        public string IMDBId { get; set; }
        public string TVDBId { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
