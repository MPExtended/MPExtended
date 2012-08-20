using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.MediaAccessService.Interfaces.Movie
{
    public class WebMovieBasic : WebMediaItem, IYearSortable, IGenreSortable, IRatingSortable, IActors
    {
        public WebMovieBasic()
        {
            Genres = new List<string>();
            ExternalId = new List<WebExternalId>();
            Actors = new List<WebActor>();
        }

        public bool IsProtected { get; set; }
        public IList<string> Genres { get; set; }
        public IList<WebExternalId> ExternalId { get; set; }
        public IList<WebActor> Actors { get; set; }

        public int Year { get; set; }
        public float Rating { get; set; }
        public int Runtime { get; set; }

        public override WebMediaType Type 
        {
            get
            {
                return WebMediaType.Movie;
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}