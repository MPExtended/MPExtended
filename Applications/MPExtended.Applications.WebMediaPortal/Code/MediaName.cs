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

using MPExtended.Libraries.Service;
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
                        return Connections.Current.MAS.GetMovieDetailedById(Settings.ActiveSettings.MovieProvider, id).Title;
                    case WebMediaType.Collection:
                        return Connections.Current.MAS.GetCollectionById(Settings.ActiveSettings.MovieProvider, id).Title;
                    case WebMediaType.MovieActor:
                        return Connections.Current.MAS.GetMovieActorById(Settings.ActiveSettings.MovieProvider, id).Title;
                    case WebMediaType.MovieGenre:
                        return Connections.Current.MAS.GetMovieGenreById(Settings.ActiveSettings.MovieProvider, id).Title;
                    case WebMediaType.MusicAlbum:
                        return Connections.Current.MAS.GetMusicAlbumBasicById(Settings.ActiveSettings.MusicProvider, id).Title;
                    case WebMediaType.MusicTrack:
                        return Connections.Current.MAS.GetMusicTrackDetailedById(Settings.ActiveSettings.MusicProvider, id).Title;
                    case WebMediaType.Picture:
                        return Connections.Current.MAS.GetPictureDetailedById(Settings.ActiveSettings.PicturesProvider, id).Title;
                    case WebMediaType.MobileVideo:
                        return Connections.Current.MAS.GetMobileVideoBasicById(Settings.ActiveSettings.PicturesProvider, id).Title;
                    case WebMediaType.PictureFolder:
                        return Connections.Current.MAS.GetPictureFolderById(Settings.ActiveSettings.PicturesProvider, id).Title;
                    case WebMediaType.Recording:
                        return Connections.Current.TAS.GetRecordingById(Int32.Parse(id)).Title;
                    case WebMediaType.TV:
                        return Connections.Current.TAS.GetChannelDetailedById(Int32.Parse(id)).Title;
                    case WebMediaType.TVEpisode:
                        return Connections.Current.MAS.GetTVEpisodeDetailedById(Settings.ActiveSettings.TVShowProvider, id).Title;
                    case WebMediaType.TVShow:
                        return Connections.Current.MAS.GetTVShowDetailedById(Settings.ActiveSettings.TVShowProvider, id).Title;
                    case WebMediaType.TVShowActor:
                        return Connections.Current.MAS.GetTVShowActorById(Settings.ActiveSettings.TVShowProvider, id).Title;
                    case WebMediaType.TVShowGenre:
                        return Connections.Current.MAS.GetTVShowGenreById(Settings.ActiveSettings.TVShowProvider, id).Title;
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
