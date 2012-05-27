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
using System.Web.Mvc;
using System.Net;
using System.IO;
using MPExtended.Applications.WebMediaPortal.Mvc;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    internal static class Images
    {
        private static ActionResult ReturnFromService(Func<Stream> method, string defaultFile = null)
        {
            using (var scope = WCFClient.EnterOperationScope(MPEServices.MASStream))
            {
                var image = method.Invoke();

                var returnCode = WCFClient.GetHeader<int>("responseCode");
                if ((HttpStatusCode)returnCode != HttpStatusCode.OK)
                {
                    if (defaultFile == null)
                    {
                        return new HttpStatusCodeResult(returnCode);
                    }
                    else
                    {
                        string virtualPath = PathMapper.GetSkinVirtualContentPath(new HttpContextWrapper(HttpContext.Current), defaultFile);
                        string physicalPath = HttpContext.Current.Server.MapPath(virtualPath);
                        return new FilePathResult(physicalPath, MIME.GetFromFilename(physicalPath, "application/octet-stream"));
                    }
                }

                return new FileStreamResult(image, WCFClient.GetHeader<string>("contentType", "image/jpeg"));
            }
        }

        public static ActionResult ReturnFromService(WebStreamMediaType mediaType, string id, WebArtworkType artworkType, int maxWidth, int maxHeight, string defaultFile = null)
        {
            IStreamingService service;
            int? provider = null;

            switch (mediaType)
            {
                case WebStreamMediaType.Drive:
                case WebStreamMediaType.File:
                case WebStreamMediaType.Folder:
                    service = MPEServices.MASStream;
                    provider = Settings.ActiveSettings.FileSystemProvider;
                    break;
                case WebStreamMediaType.Movie:
                    service = MPEServices.MASStream;
                    provider = Settings.ActiveSettings.MovieProvider;
                    break;
                case WebStreamMediaType.MusicAlbum:
                case WebStreamMediaType.MusicArtist:
                case WebStreamMediaType.MusicTrack:
                    service = MPEServices.MASStream;
                    provider = Settings.ActiveSettings.MusicProvider;
                    break;
                case WebStreamMediaType.Picture:
                    service = MPEServices.MASStream;
                    provider = Settings.ActiveSettings.PicturesProvider;
                    break;
                case WebStreamMediaType.TVShow:
                case WebStreamMediaType.TVSeason:
                case WebStreamMediaType.TVEpisode:
                    service = MPEServices.MASStream;
                    provider = Settings.ActiveSettings.TVShowProvider;
                    break;
                case WebStreamMediaType.TV:
                case WebStreamMediaType.Recording:
                    service = MPEServices.MASStream;
                    break;
                default:
                    throw new ArgumentException("Tried to load image for unknown mediatype " + mediaType);
            }

            return ReturnFromService(() => service.GetArtworkResized(mediaType, provider, id, artworkType, 0, maxWidth, maxHeight), defaultFile);
        }

        public static ActionResult ReturnFromService(WebStreamMediaType mediaType, string id, WebArtworkType artworkType, string defaultFile = null)
        {
            return ReturnFromService(mediaType, id, artworkType, 0, 0, defaultFile);
        }
    }
}