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
using System.Web.Routing;
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

        public static string Artwork(this UrlHelper helper, WebMediaType mediaType, string id, string protocol = null, string hostName = null)
        {
            switch (mediaType)
            {
                case WebMediaType.Movie:
                    return helper.Action("Cover", "MovieLibrary", new RouteValueDictionary(new { movie = id }), protocol, hostName);
                case WebMediaType.MusicAlbum:
                    return helper.Action("AlbumImage", "MusicLibrary", new RouteValueDictionary(new { album = id }), protocol, hostName);
                case WebMediaType.MusicArtist:
                    return helper.Action("ArtistImage", "MusicLibrary", new RouteValueDictionary(new { artist = id }), protocol, hostName);
                case WebMediaType.MusicTrack:
                    return helper.Action("TrackImage", "MusicLibrary", new RouteValueDictionary(new { track = id }), protocol, hostName);
                case WebMediaType.Radio:
                case WebMediaType.TV:
                    return helper.Action("ChannelLogo", "Television", new RouteValueDictionary(new { channelId = id }), protocol, hostName);
                case WebMediaType.Recording:
                    // TODO: Make width configurable with a parameter (object attributes or something like it)
                    return helper.Action("PreviewImage", "Recording", new RouteValueDictionary(new { id = id, width = 640 }), protocol, hostName);
                case WebMediaType.TVEpisode:
                    return helper.Action("EpisodeImage", "TVShowsLibrary", new RouteValueDictionary(new { episode = id }), protocol, hostName);
                case WebMediaType.TVSeason:
                    return helper.Action("SeasonImage", "TVShowsLibrary", new RouteValueDictionary(new { season = id }), protocol, hostName);
                case WebMediaType.TVShow:
                    return helper.Action("SeriesPoster", "TVShowsLibrary", new RouteValueDictionary(new { season = id }), protocol, hostName);
                default:
                    return String.Empty;
            }
        }
    }
}