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
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public IList<string> Genres { get; set; }
        public bool IsProtected { get; set; }
        public int Year { get; set; }
        public string BackdropPath { get; set; }
        public string CoverPath { get; set; }
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