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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    public class StreamController : BaseController
    {
        //
        // Streaming
        private int? GetProvider(WebStreamMediaType type)
        {
            switch (type)
            {
                case WebStreamMediaType.File:
                    return Settings.ActiveSettings.FileSystemProvider;
                case WebStreamMediaType.Movie:
                    return Settings.ActiveSettings.MovieProvider;
                case WebStreamMediaType.MusicAlbum:
                case WebStreamMediaType.MusicTrack:
                    return Settings.ActiveSettings.MusicProvider;
                case WebStreamMediaType.Picture:
                    return Settings.ActiveSettings.PicturesProvider;
                case WebStreamMediaType.Recording:
                case WebStreamMediaType.TV:
                    return 0;
                case WebStreamMediaType.TVEpisode:
                case WebStreamMediaType.TVSeason:
                case WebStreamMediaType.TVShow:
                    return Settings.ActiveSettings.TVShowProvider;
                default:
                    // this cannot happen
                    return 0;
            }
        }

        private ActionResult Download(WebStreamMediaType type, string itemId, string transcoder)
        {
            // Create URL to GetMediaItem
            var queryString = HttpUtility.ParseQueryString(String.Empty); // you can't instantiate that class manually for some reason
            queryString["type"] = ((int)type).ToString();
            queryString["itemId"] = itemId;
            string rootUrl = type == WebStreamMediaType.TV || type == WebStreamMediaType.Recording ? MPEServices.HttpTASStreamRoot : MPEServices.HttpMASStreamRoot;
            Uri fullUri = new Uri(rootUrl + "GetMediaItem?" + queryString.ToString());

            // Check stream type
            StreamType streamMode = Settings.ActiveSettings.StreamType;
            if (streamMode == StreamType.DirectWhenPossible)
            {
                streamMode = NetworkInformation.IsOnLAN(NetworkInformation.NormalizeAddress(HttpContext.Request.UserHostAddress)) ? StreamType.Direct : StreamType.Proxied;
            }

            // Do the actual streaming
            if (streamMode == StreamType.Proxied)
            {
                ProxyStream(fullUri.ToString());
            }
            else if (streamMode == StreamType.Direct)
            {
                return Redirect(fullUri.ToString());
            }
            return new EmptyResult();
        }

        private ActionResult GenerateStream(WebStreamMediaType type, string itemId, string transcoder)
        {
            // Do a standard stream
            string identifier = "webmediaportal-" + Guid.NewGuid().ToString("D");
            if (!GetStreamControl(type).InitStream((WebStreamMediaType)type, GetProvider(type), itemId, "WebMediaPortal", identifier))
            {
                Log.Error("Streaming: InitStream failed");
                return new EmptyResult();
            }

            string url = GetStreamControl(type).StartStream(identifier, transcoder, 0);
            if (String.IsNullOrEmpty(url))
            {
                Log.Error("Streaming: StartStream failed");
                return new EmptyResult();
            }

            // Check stream type
            StreamType streamMode = Settings.ActiveSettings.StreamType;
            if (streamMode == StreamType.DirectWhenPossible)
            {
                streamMode = NetworkInformation.IsOnLAN(NetworkInformation.NormalizeAddress(HttpContext.Request.UserHostAddress)) ? StreamType.Direct : StreamType.Proxied;
            }

            // Do the actual streaming
            if (streamMode == StreamType.Proxied)
            {
                ProxyStream(url);
            }
            else if (streamMode == StreamType.Direct)
            {
                return Redirect(url);
            }

            // kill stream (doesn't matter much if this doesn't happen, WSS kills streams automatically nowadays)
            if (!GetStreamControl(type).FinishStream(identifier))
            {
                Log.Error("Streaming: FinishStream failed");
            }
            return new EmptyResult();
        }

        protected void ProxyStream(string sourceUrl)
        {
            byte[] buffer = new byte[65536]; // we don't actually read the full buffer each time, so a big size is ok
            int read;

            // do request
            WebResponse response = WebRequest.Create(sourceUrl).GetResponse();
            Stream sourceStream = response.GetResponseStream();

            // set headers and disable buffer
            HttpContext.Response.Buffer = false;
            HttpContext.Response.BufferOutput = false;
            HttpContext.Response.ContentType = response.Headers["Content-Type"] == null ? "video/MP2T" : response.Headers["Content-Type"];
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

            // stream to output
            while (HttpContext.Response.IsClientConnected && (read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                HttpContext.Response.OutputStream.Write(buffer, 0, read);
                HttpContext.Response.OutputStream.Flush(); // TODO: is this needed?
            }
        }

        //
        // Stream wrapper URLs
        public ActionResult TV(int item, string transcoder)
        {
            return GenerateStream(WebStreamMediaType.TV, item.ToString(), transcoder);
        }

        public ActionResult Movie(int item, string transcoder)
        {
            return GenerateStream(WebStreamMediaType.Movie, item.ToString(), transcoder);
        }

        public ActionResult TVEpisode(int item, string transcoder)
        {
            return GenerateStream(WebStreamMediaType.TVEpisode, item.ToString(), transcoder);
        }

        public ActionResult Recording(int item, string transcoder)
        {
            return GenerateStream(WebStreamMediaType.Recording, item.ToString(), transcoder);
        }

        public ActionResult MusicTrack(int item, string transcoder)
        {
            return GenerateStream(WebStreamMediaType.MusicTrack, item.ToString(), transcoder);
        }

        //
        // Player
        [Authorize]
        public ActionResult Player(WebStreamMediaType type, string itemId, bool video = true)
        {
            // get transcoding profile
            IWebStreamingService streamControl = GetStreamControl(type);
            WebTranscoderProfile profile = null;
            if (Request.QueryString["transcoder"] != null)
                profile = GetStreamControl(type).GetTranscoderProfileByName(Request.QueryString["transcoder"]);
            if (Request.Form["transcoder"] != null)
                profile = GetStreamControl(type).GetTranscoderProfileByName(Request.Form["transcoder"]);
            if (profile == null)
            {
                string defaultName = "";
                if(type == WebStreamMediaType.TV || type == WebStreamMediaType.Recording) 
                {
                    defaultName = Settings.ActiveSettings.DefaultTVProfile;
                } 
                else if(video)
                {
                    defaultName = Settings.ActiveSettings.DefaultMediaProfile;
                }
                else 
                {
                    defaultName = Settings.ActiveSettings.DefaultAudioProfile;
                }
                profile = GetStreamControl(type).GetTranscoderProfileByName(defaultName);
            }

            // get all transcoder profiles
            List<StreamTarget> targets = video ? StreamTarget.GetVideoTargets() : StreamTarget.GetAudioTargets();
            List<string> profiles = new List<string>();
            foreach (StreamTarget target in targets)
            {
                profiles = profiles.Concat(GetStreamControl(type).GetTranscoderProfilesForTarget(target.Name).Select(x => x.Name)).ToList();
            }

            // get view properties
            VideoPlayer player = targets.First(x => x.Name == profile.Target).Player;
            string viewName = Enum.GetName(typeof(VideoPlayer), player) + "Player";

            // player size
            WebResolution playerSize;
            if (!video)
            {
                playerSize = new WebResolution() { Width = 300, Height = 150 };
            } 
            else
            {
                playerSize = GetStreamControl(type).GetStreamSize(type, GetProvider(type), itemId, profile.Name);
            }

            // generate url
            RouteValueDictionary parameters = new RouteValueDictionary();
            parameters["item"] = itemId;
            parameters["transcoder"] = profile.Name;

            // generate view
            return PartialView(new PlayerViewModel
            {
                Transcoders = profiles,
                Transcoder = profile.Name,
                Player = player,
                PlayerViewName = viewName,
                URL = Url.Action(Enum.GetName(typeof(WebStreamMediaType), type), parameters),
                Size = playerSize
            });
        }

        private IWebStreamingService GetStreamControl(WebStreamMediaType type)
        {
            if (type == WebStreamMediaType.TV || type == WebStreamMediaType.Recording)
            {
                return MPEServices.TASStreamControl;
            }
            else
            {
                return MPEServices.MASStreamControl;
            }
        }
    }
}