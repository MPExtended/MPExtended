#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class HomeViewModel
    {
        public AvailabilityModel Availability { get; set; }

        public HomeViewModel(AvailabilityModel availabilityModel)
        {
            Availability = availabilityModel;
        }

        public IEnumerable<MovieViewModel> GetLastAddedMovies(int count, bool unwatched = false)
        {
            try
            {
                return Connections.Current.MAS.GetMoviesDetailedByRange(Settings.ActiveSettings.MovieProvider, 0, count - 1, sort: WebSortField.DateAdded, order: WebSortOrder.Desc)
                    .Where(movie => !String.IsNullOrEmpty(movie.Title))
                    .Select(movie => new MovieViewModel(movie));
            }
            catch (Exception)
            {
                return new List<MovieViewModel>();
            }
        }

        public IEnumerable<AlbumViewModel> GetLastAddedAlbums(int count)
        {
            try
            {
                return Connections.Current.MAS.GetMusicAlbumsBasicByRange(Settings.ActiveSettings.MusicProvider, 0, count - 1, sort: WebSortField.DateAdded, order: WebSortOrder.Desc)
                    .Where(album => !String.IsNullOrEmpty(album.Title) && album.Artists.Any())
                    .Select(album => new AlbumViewModel(album));
            }
            catch (Exception)
            {
                return new List<AlbumViewModel>();
            }
        }

        public IEnumerable<MusicTrackViewModel> GetLastAddedMusicTracks(int count)
        {
            try
            {
                return Connections.Current.MAS.GetMusicTracksDetailedByRange(Settings.ActiveSettings.MusicProvider, 0, count - 1, sort: WebSortField.DateAdded, order: WebSortOrder.Desc)
                    .Where(track => !String.IsNullOrEmpty(track.Title) && track.Artist.Any())
                    .Select(track => new MusicTrackViewModel(track));
            }
            catch (Exception)
            {
                return new List<MusicTrackViewModel>();
            }
        }

        public IEnumerable<TVEpisodeViewModel> GetLastAddedTVEpisodes(int count, bool unwatched = false)
    {
            try
            {
                return Connections.Current.MAS.GetTVEpisodesDetailedByRange(Settings.ActiveSettings.TVShowProvider, 0, count - 1, sort: WebSortField.DateAdded, order: WebSortOrder.Desc)
                    .Select(ep => new TVEpisodeViewModel(ep))
                    .Where(ep => !String.IsNullOrEmpty(ep.Episode.Title));
            }
            catch (Exception)
            {
                return new List<TVEpisodeViewModel>();
            }
        }

        public IEnumerable<TVEpisodeViewModel> GetLastAiredTVEpisodes(int count, bool unwatched = false)
    {
            try
            {
                return Connections.Current.MAS.GetTVEpisodesDetailedByRange(Settings.ActiveSettings.TVShowProvider, 0, count - 1, sort: WebSortField.TVDateAired, order: WebSortOrder.Desc)
                    .Select(ep => new TVEpisodeViewModel(ep))
                    .Where(ep => !String.IsNullOrEmpty(ep.Episode.Title));
            }
            catch (Exception)
            {
                return new List<TVEpisodeViewModel>();
            }
        }

        public IEnumerable<WebRecordingBasic> GetLastRecordings(int count)
        {
            try
            {
                return Connections.Current.TAS.GetRecordingsByRange(0, count - 1, sort: WebSortField.StartTime, order: WebSortOrder.Desc)
                    .Where(rec => rec.Title != null);
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
                return Connections.Current.TAS.GetScheduledRecordingsForToday(sort: WebSortField.StartTime, order: WebSortOrder.Desc);
            }
            catch (Exception)
            {
                return new List<WebScheduledRecording>();
            }
        }
    }
}