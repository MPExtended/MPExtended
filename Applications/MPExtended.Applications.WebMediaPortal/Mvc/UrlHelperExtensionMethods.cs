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
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MPExtended.Libraries.Service;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public static class UrlHelperExtensionMethods
    {
        public static string ContentLink(this UrlHelper helper, string contentPath)
        {
            try
            {
                return helper.Content(ContentLocator.Current.LocateContent(contentPath));
            }
            catch (FileNotFoundException e)
            {
                Log.Warn(String.Format("Failed to create URL for ContentLink('{0}')", contentPath), e);
                return String.Empty;
            }
        }

        public static string ViewContentLink(this UrlHelper helper, string viewContentPath)
        {
            try
            {
                return helper.Content(ContentLocator.Current.LocateView(viewContentPath));
            }
            catch (FileNotFoundException e)
            {
                Log.Warn(String.Format("Failed to create URL for ViewContentLink('{0}')", viewContentPath), e);
                return String.Empty;
            }
        }

        public static string Artwork(this UrlHelper helper, WebMediaType mediaType, string id)
        {
            switch (mediaType)
            {
                case WebMediaType.Movie:
                    return helper.Action("Cover", "MovieLibrary", new { movie = id });
                case WebMediaType.MusicAlbum:
                    return helper.Action("AlbumImage", "MusicLibrary", new { album = id });
                case WebMediaType.MusicArtist:
                    return helper.Action("ArtistImage", "MusicLibrary", new { artist = id });
                case WebMediaType.MusicTrack:
                    return helper.Action("TrackImage", "MusicLibrary", new { track = id });
                case WebMediaType.Radio:
                case WebMediaType.TV:
                    return helper.Action("ChannelLogo", "Television", new { channelId = id });
                case WebMediaType.Recording:
                    // TODO: Make width configurable with a parameter (object attributes or something like it)
                    return helper.Action("PreviewImage", "Recording", new { id = id, width = 640 });
                case WebMediaType.TVEpisode:
                    return helper.Action("EpisodeImage", "TVShowsLibrary", new { episode = id });
                case WebMediaType.TVSeason:
                    return helper.Action("SeasonImage", "TVShowsLibrary", new { season = id });
                case WebMediaType.TVShow:
                    return helper.Action("SeriesPoster", "TVShowsLibrary", new { season = id });
                default:
                    return String.Empty;
            }
        }
    }
}