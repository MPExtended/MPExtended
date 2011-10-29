#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    public class StreamController : Controller
    {
        //
        // GET: /Stream/
        public ActionResult Index()
        {
            return View();
        }

        //
        // Streaming
        private int GetProvider(WebStreamMediaType type)
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

        private ActionResult GenerateStream(WebStreamMediaType type, string itemId, string transcoder)
        {
            string identifier = "webmediaportal-" + Guid.NewGuid().ToString("D");
            if (!MPEServices.MASStreamControl.InitStream((WebStreamMediaType)type, GetProvider(type), itemId, "WebMediaPortal", identifier))
            {
                Log.Error("Streaming: InitStream failed");
                return new EmptyResult();
            }

            return DoStreaming(identifier, transcoder);
        }

        private ActionResult DoStreaming(string identifier, string transcoderProfile)
        {
            string url = MPEServices.MASStreamControl.StartStream(identifier, transcoderProfile, 0);
            if (String.IsNullOrEmpty(url))
            {
                Log.Error("Streaming: StartStream failed");
                return new EmptyResult();
            }

            // TODO: this should be done better
            byte[] buffer = new byte[65536];
            int read;
            Stream inputStream = WebRequest.Create(url).GetResponse().GetResponseStream();

            // set headers and disable buffer
            HttpContext.Response.Buffer = false;
            HttpContext.Response.BufferOutput = false;
            HttpContext.Response.ContentType = MPEServices.MASStreamControl.GetTranscoderProfileByName(transcoderProfile).MIME;
            HttpContext.Response.StatusCode = 200;

            // stream to output
            while (HttpContext.Response.IsClientConnected && (read = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                HttpContext.Response.OutputStream.Write(buffer, 0, read);
                HttpContext.Response.OutputStream.Flush();
            }

            if (!MPEServices.MASStreamControl.FinishStream(identifier))
            {
                Log.Error("Streaming: FinishStream failed");
            }

            return new EmptyResult();
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
        public ActionResult Player(WebStreamMediaType type, string itemId, bool showVideo = true)
        {
            // TODO: insert proper support for non-resizing players
            // TODO: insert proper support for VLC player

            // get the profile
            string target = showVideo ? "pc-flash-video" : "pc-flash-audio";
            string preferredProfile = showVideo ? "Flash LQ" : "Flash Audio";
            string transcoderName = Request.Params["player"] != null ? Request.Params["player"] : preferredProfile;
            WebTranscoderProfile profile = MPEServices.MASStreamControl.GetTranscoderProfileByName(transcoderName);
            if (profile == null || profile.Target != target) {
                List<WebTranscoderProfile> profiles = MPEServices.MASStreamControl.GetTranscoderProfilesForTarget(target);
                if(profiles.Count == 0)
                    throw new ArgumentException("Profile does not exists");
                profile = profiles.First();
            }
            VideoPlayer player = VideoPlayer.Flash;
            string viewName = Enum.GetName(typeof(VideoPlayer), player) + "Player";

            // player size
            WebResolution playerSize;
            if (!showVideo)
            {
                playerSize = new WebResolution() { Width = 300, Height = 120 };
            } 
            else
            {
                playerSize = MPEServices.MASStreamControl.GetStreamSize((WebStreamMediaType)type, GetProvider((WebStreamMediaType)type), itemId, profile.Name);
            }

            // generate url
            RouteValueDictionary parameters = new RouteValueDictionary();
            parameters["item"] = itemId;
            parameters["transcoder"] = transcoderName;

            // generate view
            return PartialView(viewName, new StreamModel
            {
                URL = Url.Action(Enum.GetName(typeof(WebStreamMediaType), type), parameters),
                Size = playerSize
            });
        }
    }
}