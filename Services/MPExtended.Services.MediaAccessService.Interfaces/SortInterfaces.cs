#region Copyright (C) 2012-2013 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
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
