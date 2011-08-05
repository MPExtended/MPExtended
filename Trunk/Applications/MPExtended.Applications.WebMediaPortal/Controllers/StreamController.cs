#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
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
using MPExtended.Libraries.ServiceLib;

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
        public ActionResult TV(int item, string transcoder, string size)
        {
            string identifier = "webmediaportal-" + Guid.NewGuid().ToString("D");
            if (!MPEServices.NetPipeWebStreamService.InitTVStream(item, "WebMediaPortal", identifier))
            {
                Log.Error("Streaming: InitStream failed");
                return new EmptyResult();
            }

            return DoStreaming(identifier, transcoder);
        }

        private ActionResult GenerateStream(StreamMedia type, string itemId, string transcoder)
        {
            string identifier = "webmediaportal-" + Guid.NewGuid().ToString("D");
            if (!MPEServices.NetPipeWebStreamService.InitStream((WebMediaType)type, itemId, "WebMediaPortal", identifier))
            {
                Log.Error("Streaming: InitStream failed");
                return new EmptyResult();
            }

            return DoStreaming(identifier, transcoder);
        }

        private ActionResult DoStreaming(string identifier, string transcoderProfile)
        {
            if (!MPEServices.NetPipeWebStreamService.StartStream(identifier, transcoderProfile, 0))
            {
                Log.Error("Streaming: StartStream failed");
                return new EmptyResult();
            }

            // TODO: this should be done better
            byte[] buffer = new byte[65536];
            int read;
            string url = "http://localhost:4322/MPExtended/StreamingService/stream/RetrieveStream?identifier=" + identifier;
            Stream inputStream = WebRequest.Create(url).GetResponse().GetResponseStream();

            // set headers and diisable buffer
            HttpContext.Response.Buffer = false;
            HttpContext.Response.BufferOutput = false;
            HttpContext.Response.ContentType = MPEServices.NetPipeWebStreamService.GetTranscoderProfileByName(transcoderProfile).MIME;
            HttpContext.Response.StatusCode = 200;

            while (HttpContext.Response.IsClientConnected && (read = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                HttpContext.Response.OutputStream.Write(buffer, 0, read);
                HttpContext.Response.OutputStream.Flush();
            }

            if (!MPEServices.NetPipeWebStreamService.FinishStream(identifier))
            {
                Log.Error("Streaming: FinishStream failed");
            }

            return new EmptyResult();
        }

        //
        // Stream wrapper URLs
        public ActionResult Movie(int item, string transcoder)
        {
            return GenerateStream(StreamMedia.Movie, item.ToString(), transcoder);
        }

        public ActionResult Serie(int item, string transcoder)
        {
            return GenerateStream(StreamMedia.Serie, item.ToString(), transcoder);
        }

        public ActionResult Recording(int item, string transcoder)
        {
            return GenerateStream(StreamMedia.Recording, item.ToString(), transcoder);
        }

        public ActionResult Music(int item, string transcoder)
        {
            return GenerateStream(StreamMedia.Music, item.ToString(), transcoder);
        }

        //
        // Player
        public ActionResult Player(StreamMedia type, string itemId, bool showVideo = true)
        {
            // TODO: insert proper support for non-resizing players
            // TODO: insert proper support for VLC player

            // get the profile
            string target = showVideo ? "pc-flash-video" : "pc-flash-audio";
            string preferredProfile = showVideo ? "Flash LQ" : "Flash Audio";
            string transcoderName = Request.Params["player"] != null ? Request.Params["player"] : preferredProfile;
            WebTranscoderProfile profile = MPEServices.NetPipeWebStreamService.GetTranscoderProfileByName(transcoderName);
            if (profile == null || profile.Target != target) {
                List<WebTranscoderProfile> profiles = MPEServices.NetPipeWebStreamService.GetTranscoderProfilesForTarget(target);
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
            else if (type == StreamMedia.TV)
            {
                playerSize = MPEServices.NetPipeWebStreamService.GetTVStreamSize(Int32.Parse(itemId), profile.Name);
            }
            else
            {
                playerSize = MPEServices.NetPipeWebStreamService.GetStreamSize((WebMediaType)type, itemId, profile.Name);
            }

            // generate url
            RouteValueDictionary parameters = new RouteValueDictionary();
            parameters["item"] = itemId;
            parameters["transcoder"] = transcoderName;

            // generate view
            return PartialView(viewName, new StreamModel
            {
                URL = Url.Action(Enum.GetName(typeof(StreamMedia), type), parameters),
                Size = playerSize
            });
        }
    }
}