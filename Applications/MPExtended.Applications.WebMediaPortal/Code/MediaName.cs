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

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public static class MediaName
    {
        public static string GetMediaName(WebStreamMediaType type, string id)
        {
            try
            {
                switch (type)
                {
                    case WebStreamMediaType.File:
                        return MPEServices.MAS.GetFileSystemFileBasicById(Settings.ActiveSettings.FileSystemProvider, id).Title;
                    case WebStreamMediaType.Movie:
                        return MPEServices.MAS.GetMovieBasicById(Settings.ActiveSettings.MovieProvider, id).Title;
                    case WebStreamMediaType.MusicAlbum:
                        return MPEServices.MAS.GetMusicAlbumBasicById(Settings.ActiveSettings.MusicProvider, id).Title;
                    case WebStreamMediaType.MusicTrack:
                        return MPEServices.MAS.GetMusicTrackBasicById(Settings.ActiveSettings.MusicProvider, id).Title;
                    case WebStreamMediaType.Picture:
                        return MPEServices.MAS.GetPictureBasicById(Settings.ActiveSettings.PicturesProvider, id).Title;
                    case WebStreamMediaType.Recording:
                        return MPEServices.TAS.GetRecordingById(Int32.Parse(id)).Title;
                    case WebStreamMediaType.TV:
                        return MPEServices.TAS.GetChannelBasicById(Int32.Parse(id)).DisplayName;
                    case WebStreamMediaType.TVEpisode:
                        return MPEServices.MAS.GetTVEpisodeBasicById(Settings.ActiveSettings.TVShowProvider, id).Title;
                    case WebStreamMediaType.TVSeason:
                    case WebStreamMediaType.TVShow:
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