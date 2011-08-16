using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public interface ITitleSortable
    {
        string Title { get; set; }
    }

    public interface IDateAddedSortable 
    {
        DateTime DateAdded { get; set; }
    }

    public interface IYearSortable
    {
        int Year { get; set; }
    }

    public interface IGenreSortable
    {
        string Genre { get; set; }
    }

    public interface IRatingSortable
    {
        int Rating { get; set; }
    }
}
