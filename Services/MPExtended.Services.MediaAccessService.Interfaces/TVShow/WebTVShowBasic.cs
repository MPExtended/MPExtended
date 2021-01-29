using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVShowBasic : WebObject, ITitleSortable, IDateAddedSortable, IYearSortable, IGenreSortable, IRatingSortable, IArtwork, ITVShowActors
  {
        public WebTVShowBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
            Actors = new List<WebTVShowActor>();
            Genres = new List<string>();
            Artwork = new List<WebArtwork>();
            ExternalId = new List<WebExternalId>();
        }

        public string Id { get; set; }
        public bool IsProtected { get; set; }
        public DateTime DateAdded { get; set; }
        public IList<WebTVShowActor> Actors { get; set; }
        public IList<string> Genres { get; set; }
        public IList<WebArtwork> Artwork { get; set; }
        public IList<WebExternalId> ExternalId { get; set; }

        public string Title { get; set; }
        public int Year { get; set; }
        public int EpisodeCount { get; set; }
        public int UnwatchedEpisodeCount { get; set; }
        public int SeasonCount { get; set; }
        public float Rating { get; set; }
        public string ContentRating { get; set; }
        public int FanartCount { get; set; }

        public string TVDBId
        {
            get
            {
                return ExternalId.Where(x => x.Site == "TVDB").FirstOrDefault()?.Id ?? string.Empty;
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
