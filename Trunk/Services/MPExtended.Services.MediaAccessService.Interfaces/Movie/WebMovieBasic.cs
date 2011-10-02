using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.Services.MediaAccessService.Interfaces.Movie
{
    public class WebMovieBasic : WebMediaItem, ITitleSortable, IDateAddedSortable, IYearSortable, IGenreSortable, IRatingSortable, ICategorySortable
    {
        public WebMovieBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
            Genres = new List<string>();
            CoverPaths = new List<string>();
            Path = new List<string>();
            UserDefinedCategories = new List<string>();
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public IList<string> Genres { get; set; }
        public bool IsProtected { get; set; }
        public int Year { get; set; }
        public IList<string> CoverPaths { get; set; }
        public DateTime DateAdded { get; set; }
        public IList<string> Path { get; set; }
        public float Rating { get; set; }
        public int Runtime { get; set; }
        public IList<string> UserDefinedCategories { get; set; }

        public WebMediaType Type 
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