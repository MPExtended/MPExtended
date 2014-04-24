﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.Common.Interfaces;

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
        IList<WebCategory> Categories { get; set; }
    }

    public interface IMusicTrackNumberSortable 
    {
        int TrackNumber { get; set; }
        int DiscNumber { get; set; }
    }

    public interface IMusicComposerSortable
    {
        IList<string> Composer { get; set; }
    }

    public interface ITVEpisodeNumberSortable
    {
        int EpisodeNumber { get; set; }
        int SeasonNumber { get; set; }
    }

    public interface ITVSeasonNumberSortable
    {
        int SeasonNumber { get; set; }
    }

    public interface IPictureDateTakenSortable
    {
        DateTime DateTaken { get; set; }
    }

    public interface ITVDateAiredSortable
    {
        DateTime FirstAired { get; set; }
    }

    public interface ITypeSortable
    {
        WebMediaType Type { get; set; }
    }
}
