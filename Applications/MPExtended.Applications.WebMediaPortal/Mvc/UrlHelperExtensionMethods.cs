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
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.Service;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public static class UrlHelperExtensionMethods
    {
        public static string AbsoluteAction(this UrlHelper helper, string actionName, string controllerName = null, RouteValueDictionary routeValues = null)
        {
            var request = helper.RequestContext.HttpContext.Request;
            var path = helper.Action(actionName, controllerName, routeValues);
            return String.Format("{0}://{1}{2}", ExternalUrl.GetScheme(request), ExternalUrl.GetHost(request), path);
        }

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

        private static string ArtworkImplementation(this UrlHelper helper, WebMediaType mediaType, string id, Func<string, string, RouteValueDictionary, string> actionMethod)
        {
            switch (mediaType)
            {
                case WebMediaType.Movie:
                    return actionMethod("Cover", "MovieLibrary", new RouteValueDictionary(new { movie = id }));
                case WebMediaType.Collection:
                    return actionMethod("Collection", "MovieLibrary", new RouteValueDictionary(new { collection = id }));
                case WebMediaType.MusicAlbum:
                    return actionMethod("AlbumImage", "MusicLibrary", new RouteValueDictionary(new { album = id }));
                case WebMediaType.MusicArtist:
                    return actionMethod("ArtistImage", "MusicLibrary", new RouteValueDictionary(new { artist = id }));
                case WebMediaType.MusicTrack:
                    return actionMethod("TrackImage", "MusicLibrary", new RouteValueDictionary(new { track = id }));
                case WebMediaType.Radio:
                case WebMediaType.TV:
                    return actionMethod("ChannelLogo", "Television", new RouteValueDictionary(new { channelId = id }));
                case WebMediaType.Recording:
                    // TODO: Make width configurable with a parameter (object attributes or something like it)
                    return actionMethod("PreviewImage", "Recording", new RouteValueDictionary(new { id = id, width = 640 }));
                case WebMediaType.TVEpisode:
                    return actionMethod("EpisodeImage", "TVShowsLibrary", new RouteValueDictionary(new { episode = id }));
                case WebMediaType.TVSeason:
                    return actionMethod("SeasonImage", "TVShowsLibrary", new RouteValueDictionary(new { season = id }));
                case WebMediaType.TVShow:
                    return actionMethod("SeriesPoster", "TVShowsLibrary", new RouteValueDictionary(new { season = id }));
                default:
                    return String.Empty;
            }
        }

        public static string Artwork(this UrlHelper helper, WebMediaType mediaType, string id)
        {
            return ArtworkImplementation(helper, mediaType, id, helper.Action);
        }

        public static string AbsoluteArtwork(this UrlHelper helper, WebMediaType mediaType, string id)
        {
            return ArtworkImplementation(helper, mediaType, id, helper.AbsoluteAction);
        }
    }
}
