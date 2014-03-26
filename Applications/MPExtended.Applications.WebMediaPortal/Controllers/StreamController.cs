﻿#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Applications.WebMediaPortal.Mvc;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Config;
using MPExtended.Libraries.Service.Extensions;
using MPExtended.Libraries.Service.Network;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Applications.WebMediaPortal.Code.ActionResults;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    public class StreamController : BaseController
    {
        // This is the timeout after which streams are automatically killed (in seconds)
        private const int STREAM_TIMEOUT_DIRECT = 10;
        private const int STREAM_TIMEOUT_PROXY = 300;
        private const int STREAM_TIMEOUT_HTTPLIVE = 60;

        // This is the read timeout from the proxy input stream
        private const int STREAM_PROXY_READ_TIMEOUT = 10;

        private static List<string> PlayerOpenedBy = new List<string>();
        private static Dictionary<string, string> RunningStreams = new Dictionary<string, string>();
        private static Dictionary<string, string> HttpLiveUrls = new Dictionary<string, string>();

        // Make this a static property to avoid seeding it with the same time for CreatePlayer() and GenerateStream()
        private static Random randomGenerator = new Random();

        //
        // Util
        protected int? GetProvider(WebMediaType type)
        {
            switch (type)
            {
                case WebMediaType.File:
                    return Settings.ActiveSettings.FileSystemProvider;
                case WebMediaType.Movie:
                    return Settings.ActiveSettings.MovieProvider;
                case WebMediaType.MusicAlbum:
                case WebMediaType.MusicTrack:
                    return Settings.ActiveSettings.MusicProvider;
                case WebMediaType.Picture:
                    return Settings.ActiveSettings.PicturesProvider;
                case WebMediaType.Recording:
                case WebMediaType.TV:
                    return 0;
                case WebMediaType.TVEpisode:
                case WebMediaType.TVSeason:
                case WebMediaType.TVShow:
                    return Settings.ActiveSettings.TVShowProvider;
                default:
                    // this cannot happen
                    return 0;
            }
        }

        protected bool IsUserAuthenticated()
        {
            if (!Configuration.Authentication.Enabled || Configuration.Authentication.UnauthorizedStreams ||
				PlayerOpenedBy.Contains(Request.UserHostAddress) || User.Identity.IsAuthenticated)
                return true;

            // Also allow the user to authenticate through HTTP headers. This is a bit of an ugly hack, but it's a nice way
            // for people to authenticate from scripts etc. 
            if (Request.Headers["Authorization"] != null && Request.Headers["Authorization"].StartsWith("Basic "))
            {
                var content = Request.Headers["Authorization"].Substring(6);
                var details = Encoding.ASCII.GetString(Convert.FromBase64String(content)).Split(':');
                return Configuration.Authentication.Users.Any(x => x.Username == details[0] && x.ValidatePassword(details[1]));
            }

            return false;
        }

        protected WebTranscoderProfile GetProfile(IWebStreamingService streamControl, string defaultProfile)
        {
            string profileName = Request.QueryString["transcoder"] ?? Request.Form["transcoder"] ?? defaultProfile;
            var profile = streamControl.GetTranscoderProfileByName(profileName);
            if (profile != null)
                return profile;

            Log.Error("Warning: Requested non-existing profile {0}; using default {1}", profileName, defaultProfile);
            return streamControl.GetTranscoderProfileByName(defaultProfile);
        }

        protected IWebStreamingService GetStreamControl(WebMediaType type)
        {
            return type == WebMediaType.TV || type == WebMediaType.Recording ? Connections.Current.TASStreamControl : Connections.Current.MASStreamControl;
        }


        protected StreamType GetStreamMode()
        {
            if (Settings.ActiveSettings.StreamType != StreamType.DirectWhenPossible)
                return Settings.ActiveSettings.StreamType;

            return NetworkInformation.IsLocalAddress(ExternalUrl.GetOnlyHostname(Request)) 
                    && NetworkInformation.IsOnLAN(HttpContext.Request.UserHostAddress)
                ? StreamType.Direct : StreamType.Proxied;
        }

        private string GetDefaultProfile(WebMediaType type)
        {
            StreamingProfileType profileType;
            switch (type)
            {
                case WebMediaType.TV:
                case WebMediaType.Recording:
                    profileType = StreamingProfileType.Tv;
                    break;
                case WebMediaType.MusicTrack:
                    profileType = StreamingProfileType.Audio;
                    break;
                default:
                    profileType = StreamingProfileType.Video;
                    break;
            }
            return Configuration.StreamingPlatforms.GetDefaultProfileForUserAgent(profileType, Request.UserAgent);
        }

        //
        // Streaming
        public ActionResult Download(WebMediaType type, string item, string token = null)
        {
            // Check authentication
            if (!IsUserAuthenticated())
            {
                string expectedData = String.Format("{0}_{1}_{2}", type, item, HttpContext.Application["randomToken"]);
                byte[] expectedBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(expectedData));
                string expectedToken = expectedBytes.ToHexString();
                if (token == null || expectedToken != token)
                {
                    Log.Error("Denying download type={0}, item={1}, token={2}, ip={3}: user is not logged in and token is invalid", type, item, token, Request.UserHostAddress);
                    return new HttpUnauthorizedResult();
                }
            }

            // Create URL to GetMediaItem
            Log.Debug("User wants to download type={0}; item={1}", type, item);
            var queryString = HttpUtility.ParseQueryString(String.Empty); // you can't instantiate that class manually for some reason
            var userDescription = !String.IsNullOrEmpty(HttpContext.User.Identity.Name) ? String.Format(" (user {0})", HttpContext.User.Identity) : 
                                  // TODO: also grab the user from the Authorization header here
                                  !String.IsNullOrEmpty(token) ? " (token-based download)" : String.Empty;
            queryString["clientDescription"] = "WebMediaPortal download" + userDescription;
            queryString["type"] = ((int)type).ToString();
            queryString["itemId"] = item;
            string address = type == WebMediaType.TV || type == WebMediaType.Recording ? Connections.Current.Addresses.TAS : Connections.Current.Addresses.MAS;
            string fullUrl = String.Format("http://{0}/MPExtended/StreamingService/stream/GetMediaItem?{1}", address, queryString.ToString());
            UriBuilder fullUri = new UriBuilder(fullUrl);

            // If we can access the file without any problems, let IIS stream it; that is a lot faster
            if (NetworkInformation.IsLocalAddress(fullUri.Host) && type != WebMediaType.TV)
            {
                Log.Debug("Host download directly through IIS");
                var path = type == WebMediaType.Recording ?
                    Connections.Current.TAS.GetRecordingFileInfo(Int32.Parse(item)).Path :
                    Connections.Current.MAS.GetMediaItem(GetProvider(type), type, item).Path[0];
                if (System.IO.File.Exists(path))
                {
                    return new RangeFilePathResult(MIME.GetFromFilename(path, "application/octet-stream"), new FileInfo(path));
                }
            }

            String clientDescription = "WebMediaPortal download" + userDescription;

            // If we connect to the services at localhost, actually give the extern IP address to users
            if (NetworkInformation.IsLocalAddress(fullUri.Host))
                fullUri.Host = NetworkInformation.GetIPAddressForUri();

            // Do the actual streaming
            if (GetStreamMode() == StreamType.Proxied)
            {
                Log.Debug("Proxying download at {0}", fullUri.ToString());
                GetStreamControl(type).AuthorizeStreaming();
                if (type == WebMediaType.Recording)
                {
                    WebRecordingFileInfo fileInfo = Connections.Current.TAS.GetRecordingFileInfo(Int32.Parse(item));
                    return new RangeWSSResult(fileInfo, clientDescription, type, GetProvider(type), item);
                }
                else
                {
                    WebFileInfo fileInfo = Connections.Current.MAS.GetFileInfo(GetProvider(type), type, WebFileType.Content, item, 0);
                    return new RangeWSSResult(fileInfo, clientDescription, type, GetProvider(type), item);

                }
            }
            else if (GetStreamMode() == StreamType.Direct)
            {
                Log.Debug("Redirecting user to download at {0}", fullUri.ToString());
                GetStreamControl(type).AuthorizeRemoteHostForStreaming(HttpContext.Request.UserHostAddress);
                return Redirect(fullUri.ToString());
            }
            return new EmptyResult();
        }

        private ActionResult GenerateStream(WebMediaType type, string itemId, int fileindex, string transcoder, int starttime, string continuationId)
        {
            // Check if there is actually a player requested for this stream
            if (!IsUserAuthenticated())
            {
                Log.Warn("User {0} (host {1}) requested a stream but isn't authenticated - denying access to stream", HttpContext.User.Identity.Name, Request.UserHostAddress);
                return new HttpUnauthorizedResult();
            }

            // Load and validate profile
            WebTranscoderProfile profile = GetStreamControl(type).GetTranscoderProfileByName(transcoder);
            if (profile == null)
            {
                Log.Error("Requested stream for non-existing profile '{0}', other parameters type={1}; itemId={2}; starttime={3}; continuationId={4}", transcoder, type, itemId, starttime, continuationId);
                return new HttpNotFoundResult();
            }

            // Delegate to HLS streaming if needed
            if (profile.HasVideoStream && StreamTarget.GetVideoTargets().First(x => profile.Targets.Contains(x.Name)).Player == VideoPlayer.HLS)
                return GenerateHttpLiveStream(type, itemId, fileindex, profile, starttime, continuationId);

            // Generate random identifier, and continuationId if needed
            string identifier = "webmediaportal-" + randomGenerator.Next(10000, 99999);
            continuationId = continuationId ?? "none-provided-" + randomGenerator.Next(10000, 99999).ToString();

            // Kill previous stream, but only if we expect it to be still running (avoid useless calls in non-seek and proxied cases)
            if (RunningStreams.ContainsKey(continuationId))
            {
                Log.Debug("Killing off old stream for continuationId {0} with identifier {1} first", continuationId, RunningStreams[continuationId]);
                GetStreamControl(type).FinishStream(RunningStreams[continuationId]);
            }

            // Check stream mode, generate timeout setting and dump all info we got
            StreamType streamMode = GetStreamMode();
            int timeout = streamMode == StreamType.Direct ? STREAM_TIMEOUT_DIRECT : STREAM_TIMEOUT_PROXY;
            Log.Debug("Starting stream type={0}; itemId={1}; index={2}; transcoder={3}; starttime={4}; continuationId={5}", 
                type, itemId, fileindex, transcoder, starttime, continuationId);
            Log.Debug("Stream is for user {0} from host {1}, has identifier {2} and is using mode {3} with timeout {4}s",
                HttpContext.User.Identity.Name, Request.UserHostAddress, identifier, streamMode, timeout);

            // Start the stream
            string clientDescription = String.Format("WebMediaPortal (user {0})", HttpContext.User.Identity.Name);
            using (var scope = WCFClient.EnterOperationScope(GetStreamControl(type)))
            {
                WCFClient.SetHeader("forwardedFor", HttpContext.Request.UserHostAddress);
                if (!GetStreamControl(type).InitStream((WebMediaType)type, GetProvider(type), itemId, fileindex, clientDescription, identifier, timeout))
                {
                    Log.Error("InitStream failed");
                    return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);
                }
            }

            // Save stream
            RunningStreams[continuationId] = identifier;
            string url = GetStreamControl(type).StartStream(identifier, transcoder, starttime);
            if (String.IsNullOrEmpty(url))
            {
                Log.Error("StartStream failed");
                return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            Log.Debug("Stream started successfully and is at {0}", url);

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

            // Kill stream (doesn't matter much if this doesn't happen, WSS kills streams automatically nowadays)
            Log.Debug("Finished stream {0}", identifier);
            RunningStreams.Remove(continuationId);
            if (!GetStreamControl(type).FinishStream(identifier))
            {
                Log.Error("FinishStream failed");
            }
            return new EmptyResult();
        }

        protected void ProxyStream(string sourceUrl)
        {
            byte[] buffer = new byte[65536]; // we don't actually read the full buffer each time, so a big size is ok
            int read;

            // do request
            Log.Debug("Proxying stream from {0} with buffer size {1}", sourceUrl, buffer.Length);
            WebRequest request = WebRequest.Create(sourceUrl);
            request.Headers.Add("X-Forwarded-For", HttpContext.Request.UserHostAddress);
            WebResponse response = request.GetResponse();
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

            // set reasonable timeouts on the sourceStream
            sourceStream.ReadTimeout = STREAM_PROXY_READ_TIMEOUT * 1000;

            // stream to output
            try
            {
                while (HttpContext.Response.IsClientConnected && (read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    HttpContext.Response.OutputStream.Write(buffer, 0, read);
                    HttpContext.Response.OutputStream.Flush(); // TODO: is this needed?
                }
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Exception while proxying stream {0}", sourceUrl), ex);
            }
        }

        //
        // HTTP Live Streaming
        public ActionResult StartHttpLiveStream(WebMediaType type, string itemId, int fileindex, string transcoder, string continuationId)
        {
            if (!IsUserAuthenticated())
            {
                Log.Warn("User {0} (host {1}) requested a HLS stream but isn't authenticated - denying access", HttpContext.User.Identity.Name, Request.UserHostAddress);
                return new HttpUnauthorizedResult();
            }

            var profile = GetProfile(GetStreamControl(type), transcoder);
            string identifier = ActuallyStartHttpLiveStream(type, itemId, fileindex, profile, 0, continuationId);
            if (identifier != null)
            {
                string url = GetStreamMode() == StreamType.Direct ? HttpLiveUrls[identifier] :
                    Url.Action(Enum.GetName(typeof(WebMediaType), type), new RouteValueDictionary() { 
                        { "item", itemId },
                        { "fileindex", fileindex },
                        { "transcoder", transcoder },
                        { "continuationId", continuationId }
                    });

                // iOS does not display poster images with relative paths
                string posterUrl = Url.AbsoluteArtwork(type, itemId);
                Log.Debug("HLS: Replying to explicit AJAX HLS start request for continuationId={0} with mode={1}; url={2}", continuationId, GetStreamMode(), url);
                return Json(new { Success = true, URL = url, Poster = posterUrl }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Succes = false }, JsonRequestBehavior.AllowGet);
            }
        }

        private ActionResult GenerateHttpLiveStream(WebMediaType type, string itemId, int fileindex, WebTranscoderProfile profile, int starttime, string continuationId)
        {
            string identifier = ActuallyStartHttpLiveStream(type, itemId, fileindex, profile, starttime, continuationId);
            if (identifier == null)
                return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);

            // Return the actual file contents
            GetStreamControl(type).AuthorizeRemoteHostForStreaming(HttpContext.Request.UserHostAddress);
            if (GetStreamMode() == StreamType.Direct)
            {
                Log.Debug("HLS: Using Direct streaming mode and redirecting to playlist at {0}", HttpLiveUrls[identifier]);
                return Redirect(HttpLiveUrls[identifier]);
            }
            else
            {
                ProxyHttpLiveIndex(identifier, HttpLiveUrls[identifier]);
                return new EmptyResult();
            }
        }

        private string ActuallyStartHttpLiveStream(WebMediaType type, string itemId, int fileindex, WebTranscoderProfile profile, int starttime, string continuationId)
        {
            // Get identifier and continuationId
            continuationId = continuationId ?? "hls-" + randomGenerator.Next(10000, 99999).ToString();
            bool alreadyRunning = RunningStreams.ContainsKey(continuationId);
            string identifier = alreadyRunning ? RunningStreams[continuationId] : "webmediaportal-" + randomGenerator.Next(10000, 99999);
            Log.Debug("HLS: Requested stream start for continuationId={0}; running={1}; identifier={2}", continuationId, alreadyRunning, identifier);

            // We only need to start the stream if this is the first request for this file
            string url;
            if (!alreadyRunning)
            {
                Log.Debug("HLS: Starting stream type={0}; itemId={1}; offset={2}; profile={3}; starttime={4}; continuationId={5}; identifier={6}",
                    type, itemId, fileindex, profile.Name, starttime, continuationId, identifier);
                Log.Debug("HLS: Stream is for user {0} from host {1}, has identifier {2} and timeout {3}s",
                    HttpContext.User.Identity.Name, Request.UserHostAddress, identifier, STREAM_TIMEOUT_HTTPLIVE);

                // Start the stream
                string clientDescription = String.Format("WebMediaPortal (user {0})", HttpContext.User.Identity.Name);
                using (var scope = WCFClient.EnterOperationScope(GetStreamControl(type)))
                {
                    WCFClient.SetHeader("forwardedFor", HttpContext.Request.UserHostAddress);
                    if (!GetStreamControl(type).InitStream((WebMediaType)type, GetProvider(type), itemId, fileindex, clientDescription, identifier, STREAM_TIMEOUT_HTTPLIVE))
                    {
                        Log.Error("HLS: InitStream failed");
                        return null;
                    }
                }

                // Get stream URL
                url = GetStreamControl(type).StartStream(identifier, profile.Name, starttime);
                if (String.IsNullOrEmpty(url))
                {
                    Log.Error("HLS: StartStream failed");
                    return null;
                }
                Log.Debug("HLS: Started stream successfully at {0}", url);
                RunningStreams[continuationId] = identifier;
                HttpLiveUrls[identifier] = url;
            }

            return identifier;
        }

        private void ProxyHttpLiveIndex(string identifier, string source)
        {
            WebRequest request = WebRequest.Create(source);
            request.Headers.Add("X-Forwarded-For", HttpContext.Request.UserHostAddress);
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string playlistContents = reader.ReadToEnd();

            string prefix = HttpLiveUrls[identifier].Substring(0, HttpLiveUrls[identifier].IndexOf("/stream/") + 8);
            string[] lines = playlistContents.Split('\n');
            var newPlaylist = new StringBuilder();
            string segmentUrl = String.Empty;
            foreach (var line in lines)
            {
                if (!line.Trim().StartsWith(prefix))
                {
                    newPlaylist.AppendLine(line.Trim());
                    continue;
                }

                var queryString = HttpUtility.ParseQueryString(new Uri(line.Trim()).Query);
                segmentUrl = Url.AbsoluteAction("ProxyHttpLiveSegment", "Stream", new RouteValueDictionary(new
                {
                    identifier = identifier,
                    ctdAction = queryString["action"],
                    parameters = queryString["parameters"]
                }));
                newPlaylist.AppendLine(segmentUrl);
            }

            Log.Debug("HLS: Proxying playlist for identifier={0} with source={1}; prefix={2}; lastSegment={3}", identifier, source, prefix, segmentUrl);

            try
            {
                Response.ContentType = response.ContentType;
                Response.Write(newPlaylist);
                Response.Flush();
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Exception while proxying HLS playlist from {0}", source), ex);
            }
        }

        public ActionResult ProxyHttpLiveSegment(string identifier, string ctdAction, string parameters)
        {
            if (!IsUserAuthenticated())
            {
                Log.Warn("User {0} (host {1}) requested a HLS segment but isn't authenticated - denying access", HttpContext.User.Identity.Name, Request.UserHostAddress);
                return new HttpUnauthorizedResult();
            }

            var queryString = HttpUtility.ParseQueryString(String.Empty);
            queryString["identifier"] = identifier;
            queryString["action"] = ctdAction;
            queryString["parameters"] = parameters;
            var uri = new UriBuilder(HttpLiveUrls[identifier].Substring(0, HttpLiveUrls[identifier].IndexOf("/stream/") + 8));
            uri.Path += "CustomTranscoderData";
            uri.Query = queryString.ToString();
            Log.Trace("HLS: Proxying segment for identifier={0}; ctdAction={1}; parameters={2} and uri={3}", identifier, ctdAction, parameters, uri);

            ProxyStream(uri.ToString());
            return new EmptyResult();
        }

        //
        // Stream wrapper URLs
        public ActionResult TV(string item, string transcoder, int starttime = 0, int fileindex = 0, string continuationId = null)
        {
            return GenerateStream(WebMediaType.TV, item, fileindex, transcoder, starttime, continuationId);
        }

        public ActionResult Movie(string item, string transcoder, int starttime = 0, int fileindex = 0, string continuationId = null)
        {
            return GenerateStream(WebMediaType.Movie, item, fileindex, transcoder, starttime, continuationId);
        }

        public ActionResult TVEpisode(string item, string transcoder, int starttime = 0, int fileindex = 0, string continuationId = null)
        {
            return GenerateStream(WebMediaType.TVEpisode, item, fileindex, transcoder, starttime, continuationId);
        }

        public ActionResult Recording(string item, string transcoder, int starttime = 0, int fileindex = 0, string continuationId = null)
        {
            return GenerateStream(WebMediaType.Recording, item, fileindex, transcoder, starttime, continuationId);
        }

        public ActionResult MusicTrack(string item, string transcoder, int starttime = 0, int fileindex = 0, string continuationId = null)
        {
            return GenerateStream(WebMediaType.MusicTrack, item, fileindex, transcoder, starttime, continuationId);
        }

        //
        // Player		
        protected ActionResult CreatePlayer(IWebStreamingService streamControl, PlayerViewModel model, List<StreamTarget> targets, WebTranscoderProfile profile, bool album)
        {
            // save stream request
            if (!PlayerOpenedBy.Contains(Request.UserHostAddress))
            {
                PlayerOpenedBy.Add(Request.UserHostAddress);
            }

            // get view properties
            VideoPlayer player = targets.First(x => profile.Targets.Contains(x.Name)).Player;
            string viewName = Enum.GetName(typeof(VideoPlayer), player) + (album ? "Album" : "") + "Player";

            // generate view
            var supportedTargets = Configuration.StreamingPlatforms.GetValidTargetsForUserAgent(Request.UserAgent).Intersect(targets.Select(x => x.Name));
            model.Transcoders = ProfileModel.GetProfilesForTargets(streamControl, supportedTargets).Select(x => x.Name);
            model.Transcoder = profile.Name;
            model.TranscoderProfile = profile;
            model.Player = player;
            model.PlayerViewName = viewName;
            Log.Debug("Created player with size={0} view={1} transcoder={2} url={3}", model.Size, viewName, profile.Name, model.URL);
            return PartialView("Player", model);
        }

        [ServiceAuthorize]
        public ActionResult Player(WebMediaType type, string itemId, int fileindex = 0)
        {
            PlayerViewModel model = new PlayerViewModel();
            model.MediaType = type;
            model.MediaId = itemId;
            model.ContinuationId = randomGenerator.Next(100000, 999999).ToString();

            // get profile
            var profile = GetProfile(GetStreamControl(type), GetDefaultProfile(type));

            // get size
            if (type == WebMediaType.TV)
            {
                // TODO: we should start the timeshifting through an AJAX call, and then load the player based upon the results
                // from that call. Also avoids timeouts of the player when initiating the timeshifting takes a long time.
                // HACK: currently there is no method in WSS to get the aspect ratio for streams with a fixed aspect ratio. 
                model.Size = GetStreamControl(type).GetStreamSize(type, null, "", 0, profile.Name);
            }
            else if (!StreamTarget.GetAllTargets().First(t => profile.Targets.Contains(t.Name)).HasVideo)
            {
                model.Size = new WebResolution() { Width = 600, Height = 100 };
            }
            else
            {
                model.Size = GetStreamControl(type).GetStreamSize(type, GetProvider(type), itemId, 0, profile.Name);
            }

            // generate url
            RouteValueDictionary parameters = new RouteValueDictionary();
            parameters["item"] = itemId;
            parameters["fileindex"] = fileindex;
            parameters["transcoder"] = profile.Name;
            parameters["continuationId"] = model.ContinuationId;
            model.URL = Url.Action(Enum.GetName(typeof(WebMediaType), type), parameters);

            // generic part
            var targets = type == WebMediaType.MusicTrack ? StreamTarget.GetAllTargets() : StreamTarget.GetVideoTargets();
            return CreatePlayer(GetStreamControl(type), model, targets, profile, false);
        }

        [ServiceAuthorize]
        public ActionResult MusicPlayer(string albumId)
        {
            AlbumPlayerViewModel model = new AlbumPlayerViewModel();
            model.MediaId = albumId;
            model.ContinuationId = "playlist-" + randomGenerator.Next(100000, 999999).ToString();
            WebTranscoderProfile profile = GetProfile(Connections.Current.MASStreamControl, 
                Configuration.StreamingPlatforms.GetDefaultProfileForUserAgent(StreamingProfileType.Audio, Request.UserAgent));
            model.Tracks = Connections.Current.MAS.GetMusicTracksDetailedForAlbum(Settings.ActiveSettings.MusicProvider, albumId);
            return CreatePlayer(Connections.Current.MASStreamControl, model, StreamTarget.GetAudioTargets(), profile, true);
        }

        [ServiceAuthorize]
        public ActionResult Playlist(WebMediaType type, string itemId, string transcoder = null)
        {
            if (!PlayerOpenedBy.Contains(Request.UserHostAddress))
                PlayerOpenedBy.Add(Request.UserHostAddress);

            var profile = GetProfile(GetStreamControl(type), transcoder ?? GetDefaultProfile(type));
            StringBuilder m3u = new StringBuilder();
            m3u.AppendLine("#EXTM3U");

            RouteValueDictionary parameters;
            string continuationId = "playlist-" + randomGenerator.Next(10000, 99999);
            string url;
            int filecount = 1;
            switch (type)
            {
                case WebMediaType.MusicAlbum:
                    // add all album tracks
                    foreach (WebMusicTrackBasic track in Connections.Current.MAS.GetMusicTracksBasicForAlbum(Settings.ActiveSettings.MusicProvider, itemId, sort: WebSortField.MusicTrackNumber))
                    {
                        parameters = new RouteValueDictionary();
                        parameters["item"] = track.Id;
                        parameters["transcoder"] = profile.Name;
                        parameters["continuationId"] = continuationId;
                        m3u.AppendLine(String.Format("#EXTINF:{0},{1}", track.Duration, track.Title));
                        url = Url.AbsoluteAction(Enum.GetName(typeof(WebMediaType), WebMediaType.MusicTrack), "Stream", parameters);
                        m3u.AppendLine(url);
                    }
                    break;

                case WebMediaType.MusicTrack:
                case WebMediaType.TVEpisode:
                case WebMediaType.Movie:
                    var mediaItem = Connections.Current.MAS.GetMediaItem(GetProvider(type), type, itemId);
                    filecount = mediaItem.Path.Count;
                    goto case WebMediaType.Recording; // really, Microsoft? Fall-through cases are useful. 
                case WebMediaType.TV:
                case WebMediaType.Recording:
                    for (int i = 0; i < filecount; i++)
                    {
                        parameters = new RouteValueDictionary();
                        parameters["item"] = itemId;
                        parameters["transcoder"] = profile.Name;
                        parameters["continuationId"] = continuationId;
                        parameters["fileindex"] = i;
                        url = Url.AbsoluteAction(Enum.GetName(typeof(WebMediaType), type), "Stream", parameters);
                        m3u.AppendLine("#EXTINF:-1, " + MediaName.GetMediaName(type, itemId));
                        m3u.AppendLine(url);
                    }
                    break;

                default:
                    Log.Error("Requested playlist for non-supported media type {0} with id {1}", type, itemId);
                    break;
            }

            // return it
            byte[] data = Encoding.UTF8.GetBytes(m3u.ToString());
            return File(data, "audio/x-mpegurl", "stream.m3u");
        }
    }
}