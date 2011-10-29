using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVShowBasic : WebObject, ITitleSortable, IDateAddedSortable, IYearSortable, IGenreSortable, ICategorySortable, IRatingSortable, IArtwork
    {
        public WebTVShowBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
            UserDefinedCategories = new List<string>();
            Genres = new List<string>();
            Artwork = new List<WebArtwork>();
        }

        public string Id { get; set; }
        public bool IsProtected { get; set; }
        public DateTime DateAdded { get; set; }
        public IList<string> UserDefinedCategories { get; set; }
        public IList<string> Genres { get; set; }
        public IList<WebArtwork> Artwork { get; set; }

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
