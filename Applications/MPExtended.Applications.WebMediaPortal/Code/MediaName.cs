#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public static class MediaName
    {
        public static string GetMediaName(WebMediaType type, string id)
        {
            try
            {
                switch (type)
                {
                    case WebMediaType.Movie:
                        return MPEServices.MAS.GetMovieDetailedById(Settings.ActiveSettings.MovieProvider, id).Title;
                    case WebMediaType.MusicAlbum:
                        return MPEServices.MAS.GetMusicAlbumBasicById(Settings.ActiveSettings.MusicProvider, id).Title;
                    case WebMediaType.MusicTrack:
                        return MPEServices.MAS.GetMusicTrackDetailedById(Settings.ActiveSettings.MusicProvider, id).Title;
                    case WebMediaType.Recording:
                        return MPEServices.TAS.GetRecordingById(Int32.Parse(id)).Title;
                    case WebMediaType.TV:
                        return MPEServices.TAS.GetChannelDetailedById(Int32.Parse(id)).DisplayName;
                    case WebMediaType.TVEpisode:
                        return MPEServices.MAS.GetTVEpisodeDetailedById(Settings.ActiveSettings.TVShowProvider, id).Title;
                    case WebMediaType.TVShow:
                        return MPEServices.MAS.GetTVShowDetailedById(Settings.ActiveSettings.TVShowProvider, id).Title;
                    case WebMediaType.TVSeason:
                    default:
                        return "";
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Could not load display name of media", ex);
            }
            return "";
        }
    }
}