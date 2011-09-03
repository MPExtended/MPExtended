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
        IList<string> Genres { get; set; }
    }

    public interface IRatingSortable
    {
        float Rating { get; set; }
    }

    public interface ICategorySortable
    {
        IList<string> UserDefinedCategories { get; set; }
    }

    public interface IMusicTrackNumberSortable 
    {
        int TrackNumber { get; set; }
    }

    public interface IMusicComposerSortable
    {
        IList<string> Composer { get; set; }
    }

    public interface ITVEpisodeNumberSortable
    {
        int EpisodeNumber { get; set; }
        string SeasonId { get; set; }
    }

    public interface ITVSeasonNumberSortable
    {
        int SeasonNumber { get; set; }
    }

    public interface IPictureDateTakenSortable
    {
        DateTime DateTaken { get; set; }
    }
}
