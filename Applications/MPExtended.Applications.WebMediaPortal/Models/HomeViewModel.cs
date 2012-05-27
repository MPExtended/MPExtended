#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Linq;
using System.Web;
using MPExtended.Libraries.Client;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class HomeViewModel
    {
        public AvailabilityModel Availability { get; set; }

        public HomeViewModel(AvailabilityModel availabilityModel)
        {
            Availability = availabilityModel;
        }

        public IEnumerable<WebMovieBasic> GetLastAddedMovies(int count = 4)
        {
            try
            {
                return MPEServices.MAS.GetMoviesDetailedByRange(Settings.ActiveSettings.MovieProvider, 0, count - 1, sort: SortBy.DateAdded, order: OrderBy.Desc);
            }
            catch (Exception)
            {
                return new List<WebMovieBasic>();
            }
        }

        public IEnumerable<WebTVEpisodeDetailed> GetLastAddedTVEpisodes(int count = 4)
        {
            try
            {
                return MPEServices.MAS.GetTVEpisodesDetailedByRange(Settings.ActiveSettings.TVShowProvider, 0, count - 1, SortBy.DateAdded, OrderBy.Desc);
            }
            catch (Exception)
            {
                return new List<WebTVEpisodeDetailed>();
            }
        }

        public IEnumerable<WebTVEpisodeDetailed> GetLastAiredTVEpisodes(int count = 4)
        {
            try
            {
                return MPEServices.MAS.GetTVEpisodesDetailedByRange(Settings.ActiveSettings.TVShowProvider, 0, count - 1, SortBy.TVDateAired, OrderBy.Desc);
            }
            catch (Exception)
            {
                return new List<WebTVEpisodeDetailed>();
            }
        }

        public IEnumerable<WebRecordingBasic> GetLastRecordings(int count = 4)
        {
            try
            {
                return MPEServices.TAS.GetRecordingsByRange(0, 4, SortField.StartTime, SortOrder.Desc);
            }
            catch (Exception)
            {
                return new List<WebRecordingBasic>();
            }
        }

        public IEnumerable<WebScheduledRecording> GetTodaysSchedules()
        {
            try
            {
                return MPEServices.TAS.GetScheduledRecordingsForToday(SortField.StartTime, SortOrder.Desc);
            }
            catch (Exception)
            {
                return new List<WebScheduledRecording>();
            }
        }
    }
}