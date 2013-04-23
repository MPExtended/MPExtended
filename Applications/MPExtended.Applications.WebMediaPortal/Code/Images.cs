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
using System.Web.Mvc;
using System.Net;
using System.IO;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Mvc;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public static class Images
    {
        private static long preRecordInterval = 0;

        private static ActionResult ReturnFromService(object service, Func<Stream> method, string defaultFile = null, string etag = null)
        {
            if (etag != null && HttpContext.Current.Request.Headers["If-None-Match"] == String.Format("\"{0}\"", etag))
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.NotModified;
                SetCacheHeaders(6 * 30, etag);
                return new EmptyResult();
            }

            using (var scope = WCFClient.EnterOperationScope(service))
            {
                int returnCode = 0;
                Stream image = Stream.Null;
                try
                {
                    image = method.Invoke();
                    returnCode = WCFClient.GetHeader<int>("responseCode");
                }
                catch (Exception ex)
                {
                    Log.Warn("Failure while loading image", ex);
                }

                if ((HttpStatusCode)returnCode != HttpStatusCode.OK)
                {
                    // don't cache failed-to-load images very long, as artwork may be added later on
                    SetCacheHeaders(0);
                    if (defaultFile == null)
                    {
                        return new HttpStatusCodeResult(returnCode);
                    }
                    else
                    {
                        string virtualPath = ContentLocator.Current.LocateContent(defaultFile);
                        string physicalPath = HttpContext.Current.Server.MapPath(virtualPath);
                        return new FilePathResult(physicalPath, MIME.GetFromFilename(physicalPath, "application/octet-stream"));
                    }
                }

                // we can cache these for quite long, as they probably won't change
                SetCacheHeaders(6 * 30, etag);
                return new FileStreamResult(image, WCFClient.GetHeader<string>("contentType", "image/jpeg"));
            }
        }

        private static void SetCacheHeaders(double liveForDays, string etag = null)
        {
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.Public);
            HttpContext.Current.Response.Cache.SetProxyMaxAge(TimeSpan.FromDays(liveForDays));
            HttpContext.Current.Response.Cache.SetExpires(DateTime.Now.AddDays(liveForDays));
            if (etag != null)
                HttpContext.Current.Response.Cache.SetETag(String.Format("\"{0}\"", etag));
        }

        public static ActionResult ReturnFromService(WebMediaType mediaType, string id, WebFileType artworkType, int maxWidth, int maxHeight, string defaultFile = null)
        {
            IStreamingService service;
            int? provider = null;

            switch (mediaType)
            {
                case WebMediaType.Drive:
                case WebMediaType.File:
                case WebMediaType.Folder:
                    service = Connections.Current.MASStream;
                    provider = Settings.ActiveSettings.FileSystemProvider;
                    break;
                case WebMediaType.Movie:
                    service = Connections.Current.MASStream;
                    provider = Settings.ActiveSettings.MovieProvider;
                    break;
                case WebMediaType.MusicAlbum:
                case WebMediaType.MusicArtist:
                case WebMediaType.MusicTrack:
                    service = Connections.Current.MASStream;
                    provider = Settings.ActiveSettings.MusicProvider;
                    break;
                case WebMediaType.Picture:
                    service = Connections.Current.MASStream;
                    provider = Settings.ActiveSettings.PicturesProvider;
                    break;
                case WebMediaType.TVShow:
                case WebMediaType.TVSeason:
                case WebMediaType.TVEpisode:
                    service = Connections.Current.MASStream;
                    provider = Settings.ActiveSettings.TVShowProvider;
                    break;
                case WebMediaType.TV:
                    service = Connections.Current.TASStream;
                    break;
                case WebMediaType.Recording:
                    return ExtractRecordingImage(id, maxWidth, maxHeight, defaultFile);
                default:
                    throw new ArgumentException("Tried to load image for unknown mediatype " + mediaType);
            }

            string etag = String.Format("{0}_{1}_{2}_{3}_{4}_{5}", mediaType, provider, id, artworkType, maxWidth, maxHeight);
            return ReturnFromService(service, () => service.GetArtworkResized(mediaType, provider, id, artworkType, -1, maxWidth, maxHeight), defaultFile, etag);
        }

        public static ActionResult ReturnFromService(WebMediaType mediaType, string id, WebFileType artworkType, string defaultFile = null)
        {
            return ReturnFromService(mediaType, id, artworkType, 0, 0, defaultFile);
        }

        private static ActionResult ExtractRecordingImage(string id, int maxWidth, int maxHeight, string defaultFile = null)
        {
            if (preRecordInterval == 0)
            {
                try
                {
                    var readInterval = Connections.Current.TAS.ReadSettingFromDatabase("preRecordInterval");
                    preRecordInterval = Int64.Parse(readInterval) * 60 + 30;
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to read PreRecordInterval from TV Engine database", ex);
                    preRecordInterval = 30;
                }
            }

            return ReturnFromService(Connections.Current.TASStream, () =>
                Connections.Current.TASStream.ExtractImageResized(WebMediaType.Recording, 0, id, preRecordInterval, maxWidth, maxHeight), defaultFile);
        }
    }
}