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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    public class StreamController : BaseController
    {
        // This is the timeout after which streams are automatically killed
        private const int STREAM_TIMEOUT = 5;
		
        private static List<string> PlayerOpenedBy = new List<string>();

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

        [Authorize]
        public ActionResult Download(WebStreamMediaType type, string item)
        {
            // Create URL to GetMediaItem
            var queryString = HttpUtility.ParseQueryString(String.Empty); // you can't instantiate that class manually for some reason
            queryString["type"] = ((int)type).ToString();
            queryString["itemId"] = item;
            string rootUrl = type == WebStreamMediaType.TV || type == WebStreamMediaType.Recording ? MPEServices.HttpTASStreamRoot : MPEServices.HttpMASStreamRoot;
            Uri fullUri = new Uri(rootUrl + "GetMediaItem?" + queryString.ToString());

            // Check stream type
            StreamType streamMode = Settings.ActiveSettings.StreamType;
            if (streamMode == StreamType.DirectWhenPossible)
            {
                streamMode = NetworkInformation.IsOnLAN(HttpContext.Request.UserHostAddress) ? StreamType.Direct : StreamType.Proxied;
            }

            // Do the actual streaming
            if (streamMode == StreamType.Proxied)
            {
                GetStreamControl(type).AuthorizeStreaming();
                ProxyStream(fullUri.ToString());
            }
            else if (streamMode == StreamType.Direct)
            {
                GetStreamControl(type).AuthorizeRemoteHostForStreaming(HttpContext.Request.UserHostAddress);
                return Redirect(fullUri.ToString());
            }
            return new EmptyResult();
        }

        private ActionResult GenerateStream(WebStreamMediaType type, string itemId, string transcoder, int starttime)
        {
            // Check if there is actually a player requested for this stream
            if (!PlayerOpenedBy.Contains(Request.UserHostAddress))
            {
                return new HttpUnauthorizedResult();
            }
       
            // Do a standard stream
            string identifier = "webmediaportal-" + Guid.NewGuid().ToString("D");
            if (!GetStreamControl(type).InitStream((WebStreamMediaType)type, GetProvider(type), itemId, "WebMediaPortal", identifier, STREAM_TIMEOUT))
            {
                Log.Error("Streaming: InitStream failed");
                return new EmptyResult();
            }

            string url = GetStreamControl(type).StartStream(identifier, transcoder, starttime);
            if (String.IsNullOrEmpty(url))
            {
                Log.Error("Streaming: StartStream failed");
                return new EmptyResult();
            }

            // Check stream type
            StreamType streamMode = Settings.ActiveSettings.StreamType;
            if (streamMode == StreamType.DirectWhenPossible)
            {
                streamMode = NetworkInformation.IsOnLAN(HttpContext.Request.UserHostAddress) ? StreamType.Direct : StreamType.Proxied;
            }

            // Do the actual streaming
            if (streamMode == StreamType.Proxied)
            {
				GetStreamControl(type).AuthorizeStreaming();
                ProxyStream(url);
            }
            else if (streamMode == StreamType.Direct)
            {
                GetStreamControl(type).AuthorizeRemoteHostForStreaming(HttpContext.Request.UserHostAddress);
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
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            HttpContext.Response.ContentType = response.Headers["Content-Type"] == null ? "video/MP2T" : response.Headers["Content-Type"];
            foreach (string header in response.Headers.Keys)
            {
                if (header.StartsWith("Content-"))
                {
                    HttpContext.Response.AddHeader(header, response.Headers[header]);
                }
                else if (header.StartsWith("X-Content-")) // We set the Content-Length header with the X- prefix because WCF removes the normal header
                {
                    HttpContext.Response.AddHeader(header.Substring(2), response.Headers[header]);
                }
            }

            // stream to output
            while (HttpContext.Response.IsClientConnected && (read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                HttpContext.Response.OutputStream.Write(buffer, 0, read);
                HttpContext.Response.OutputStream.Flush(); // TODO: is this needed?
            }
        }

        //
        // Stream wrapper URLs
        public ActionResult TV(int item, string transcoder, int starttime = 0)
        {
            return GenerateStream(WebStreamMediaType.TV, item.ToString(), transcoder, starttime);
        }

        public ActionResult Movie(int item, string transcoder, int starttime = 0)
        {
            return GenerateStream(WebStreamMediaType.Movie, item.ToString(), transcoder, starttime);
        }

        public ActionResult TVEpisode(int item, string transcoder, int starttime = 0)
        {
            return GenerateStream(WebStreamMediaType.TVEpisode, item.ToString(), transcoder, starttime);
        }

        public ActionResult Recording(int item, string transcoder, int starttime = 0)
        {
            return GenerateStream(WebStreamMediaType.Recording, item.ToString(), transcoder, starttime);
        }

        public ActionResult MusicTrack(int item, string transcoder, int starttime = 0)
        {
            return GenerateStream(WebStreamMediaType.MusicTrack, item.ToString(), transcoder, starttime);
        }

        //
        // Player
        private WebTranscoderProfile GetProfile(IWebStreamingService streamControl, string defaultProfile)
        {
            // get transcoding profile
            string profileName = null;
            if (Request.QueryString["transcoder"] != null)
                profileName = Request.QueryString["transcoder"];
            if (Request.Form["transcoder"] != null)
                profileName = Request.Form["transcoder"];
            if (profileName == null)
                profileName = defaultProfile;
            return streamControl.GetTranscoderProfileByName(profileName);
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

        internal ActionResult CreatePlayer(IWebStreamingService streamControl, PlayerViewModel model, List<StreamTarget> targets, WebTranscoderProfile profile)
        {
            // save stream request
            if (!PlayerOpenedBy.Contains(Request.UserHostAddress))
            {
                PlayerOpenedBy.Add(Request.UserHostAddress);
            }

            // get all transcoder profiles
            List<string> profiles = new List<string>();
            foreach (StreamTarget target in targets)
            {
                profiles = profiles.Concat(streamControl.GetTranscoderProfilesForTarget(target.Name).Select(x => x.Name)).ToList();
            }

            // get view properties
            VideoPlayer player = targets.First(x => x.Name == profile.Target).Player;
            string viewName = Enum.GetName(typeof(VideoPlayer), player) + "Player";

            // generate view
            model.Transcoders = profiles;
            model.Transcoder = profile.Name;
            model.TranscoderProfile = profile;
            model.Player = player;
            model.PlayerViewName = viewName;
            return PartialView("Player", model);
        }

        [Authorize]
        public ActionResult Player(WebStreamMediaType type, string itemId)
        {
            PlayerViewModel model = new PlayerViewModel();
            model.MediaType = type;
            model.MediaId = itemId;

            // get profile
            var defaultProfile = type == WebStreamMediaType.TV || type == WebStreamMediaType.Recording ?
                Settings.ActiveSettings.DefaultTVProfile :
                Settings.ActiveSettings.DefaultMediaProfile;
            var profile = GetProfile(GetStreamControl(type), defaultProfile);
 
            // get size
            model.Size = GetStreamControl(type).GetStreamSize(type, GetProvider(type), itemId, profile.Name);

            // generate url
            RouteValueDictionary parameters = new RouteValueDictionary();
            parameters["item"] = itemId;
            parameters["transcoder"] = profile.Name;
            model.URL = Url.Action(Enum.GetName(typeof(WebStreamMediaType), type), parameters);

            // generic part
            return CreatePlayer(GetStreamControl(type), model, StreamTarget.GetVideoTargets(), profile);
        }

        [Authorize]
        public ActionResult MusicPlayer(string albumId)
        {
            AlbumPlayerViewModel model = new AlbumPlayerViewModel();
            model.MediaId = albumId;
            WebTranscoderProfile profile = GetProfile(MPEServices.MASStreamControl, Settings.ActiveSettings.DefaultAudioProfile);
            model.Tracks = MPEServices.MAS.GetMusicTracksBasicForAlbum(Settings.ActiveSettings.MusicProvider, albumId);
            return CreatePlayer(MPEServices.MASStreamControl, model, StreamTarget.GetAudioTargets(), profile);
        }

        [Authorize]
        public ActionResult Playlist(WebStreamMediaType type, string itemId)
        {
            // save stream request
            if (!PlayerOpenedBy.Contains(Request.UserHostAddress))
            {
                PlayerOpenedBy.Add(Request.UserHostAddress);
            }

            // get profile
            var defaultProfile = type == WebStreamMediaType.TV || type == WebStreamMediaType.Recording ?
                Settings.ActiveSettings.DefaultTVProfile :
                Settings.ActiveSettings.DefaultMediaProfile;
            var profile = GetProfile(GetStreamControl(type), defaultProfile);

            // generate url
            RouteValueDictionary parameters = new RouteValueDictionary();
            parameters["item"] = itemId;
            parameters["transcoder"] = profile.Name;
            string url = Url.Action(Enum.GetName(typeof(WebStreamMediaType), type), "Stream", parameters, Request.Url.Scheme, Request.Url.Host);

            // create playlist
            StringBuilder m3u = new StringBuilder();
            m3u.AppendLine("#EXTM3U");
            m3u.AppendLine("#EXTINF:-1, " + MediaName.GetMediaName(type, itemId));
            m3u.AppendLine(url);

            // return it
            byte[] data = Encoding.UTF8.GetBytes(m3u.ToString());
            return File(data, "audio/x-mpegurl", "stream.m3u");
        }
    }
}