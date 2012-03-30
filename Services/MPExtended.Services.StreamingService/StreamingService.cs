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
using System.Reflection;
using System.ServiceModel;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Services.StreamingService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class StreamingService : IWebStreamingService, IStreamingService, IDisposable
    {
        private static Dictionary<string, WebVirtualCard> _timeshiftings = new Dictionary<string, WebVirtualCard>();
        private static List<string> _authorizedHosts = new List<string>();
        private const int API_VERSION = 2;

        private Streaming _stream;
        private Downloads _downloads;

        public StreamingService()
        {
            _stream = new Streaming(this);
            _downloads = new Downloads();
        }

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }

        public WebStreamServiceDescription GetServiceDescription()
        {
            AuthorizeStreaming();
            bool hasTv = MPEServices.HasTASConnection; // takes a while so don't execute it twice
            return new WebStreamServiceDescription()
            {
                SupportsMedia = MPEServices.HasMASConnection,
                SupportsRecordings = hasTv,
                SupportsTV = hasTv,
                ServiceVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion,
                ApiVersion = API_VERSION,
            };
        }

        #region Profiles
        public List<WebTranscoderProfile> GetTranscoderProfiles()
        {
            return Configuration.Streaming.Transcoders.Select(x => x.ToWebTranscoderProfile()).ToList();
        }

        public List<WebTranscoderProfile> GetTranscoderProfilesForTarget(string target)
        {
            return GetTranscoderProfiles().Where(x => x.Target == target).ToList();
        }

        public WebTranscoderProfile GetTranscoderProfileByName(string name)
        {
            var list = GetTranscoderProfiles().Where(x => x.Name == name);
            if(list.Count() == 0)
                return null;

            return list.First();
        }
        #endregion

        #region Info methods
        public WebMediaInfo GetMediaInfo(WebStreamMediaType type, int? provider, string itemId)
        {
            if (type == WebStreamMediaType.TV)
            {
                try
                {
                    itemId = _timeshiftings[itemId].TimeShiftFileName;
                }
                catch (KeyNotFoundException)
                {
                    Log.Error("Client tried to get mediainfo for non-existing timeshifting {0}", itemId);
                    return null;
                }
            }

            return MediaInfoHelper.LoadMediaInfoOrSurrogate(new MediaSource(type, provider, itemId));
        }

        public WebTranscodingInfo GetTranscodingInfo(string identifier, int? playerPosition)
        {
            if (playerPosition.HasValue)
            {
                _stream.SetPlayerPosition(identifier, playerPosition.Value * 1000);
            }
            return _stream.GetEncodingInfo(identifier);
        }

        public List<WebStreamingSession> GetStreamingSessions()
        {
            return _stream.GetStreamingSessions()
                .Concat(_downloads.GetActiveSessions())
                .ToList();
        }

        public WebResolution GetStreamSize(WebStreamMediaType type, int? provider, string itemId, string profile)
        {
            if (type == WebStreamMediaType.TV)
            {
                try
                {
                    itemId = _timeshiftings[itemId].TimeShiftFileName;
                }
                catch (KeyNotFoundException)
                {
                    // WebMP requests the default stream size with an empty name. This shouldn't flood the logs with warnings as people misinterpret them,
                    // and it's a TODO item in WebMP anyway. 
                    if (itemId != String.Empty)
                    {
                        Log.Warn("Client tried to get stream size for non-existing timeshifting {0}, using default aspectratio", itemId);
                    }
                    return _stream.CalculateSize(Configuration.Streaming.GetTranscoderProfileByName(profile), MediaInfoHelper.DEFAULT_ASPECT_RATIO).ToWebResolution();
                }
            }

            return _stream.CalculateSize(Configuration.Streaming.GetTranscoderProfileByName(profile), new MediaSource(type, provider, itemId)).ToWebResolution();
        }

        public bool AuthorizeStreaming()
        {
            if (!_authorizedHosts.Contains(WCFUtil.GetClientIPAddress()))
            {
                _authorizedHosts.Add(WCFUtil.GetClientIPAddress());
            }
            return true;
        }

        public bool AuthorizeRemoteHostForStreaming(string host)
        {
            if (!_authorizedHosts.Contains(host))
            {
                _authorizedHosts.Add(host);
            }
            return true;
        }

        public WebItemSupportStatus GetItemSupportStatus(WebStreamMediaType type, int? provider, string itemId)
        {
            MediaSource source = new MediaSource(type, provider, itemId);
            string path = source.GetPath();
            if (path == null || path.Length == 0)
            {
                return new WebItemSupportStatus(false, "Cannot resolve item to a path");
            }

            if (!source.Exists)
            {
                return new WebItemSupportStatus(false, "File does not exists or is inaccessible");
            }

            if (path.EndsWith(".IFO"))
            {
                return new WebItemSupportStatus(false, "Streaming DVD files is not supported");
            }

            return new WebItemSupportStatus() { Supported = true };
        }
        #endregion

        #region Streaming
        public bool InitStream(WebStreamMediaType type, int? provider, string itemId, string clientDescription, string identifier, int? idleTimeout)
        {
            AuthorizeStreaming();
            if (type == WebStreamMediaType.TV)
            {
                int channelId = Int32.Parse(itemId);
                lock (_timeshiftings)
                {
                    Log.Info("Starting timeshifting on channel {0} for client {1} with identifier {2}", channelId, clientDescription, identifier);
                    var card = MPEServices.TAS.SwitchTVServerToChannelAndGetVirtualCard("mpextended-" + identifier, channelId);
                    Log.Debug("Timeshifting started!");
                    _timeshiftings[identifier] = card;
                    itemId = card.TimeShiftFileName;
                }
            }

            Log.Info("Called InitStream with type={0}; provider={1}; itemId={2}; clientDescription={3}; identifier={4}; idleTimeout={5}", 
                type, provider, itemId, clientDescription, identifier, idleTimeout);
            return _stream.InitStream(identifier, clientDescription, new MediaSource(type, provider, itemId), idleTimeout.HasValue ? idleTimeout.Value : 5 * 60);
        }

        public string StartStream(string identifier, string profileName, int startPosition)
        {
            Log.Debug("Called StartStream with ident={0}; profile={1}; start={2}", identifier, profileName, startPosition);
            _stream.EndStream(identifier); // first end previous stream, if any available
            return _stream.StartStream(identifier, Configuration.Streaming.GetTranscoderProfileByName(profileName), startPosition * 1000);
        }

        public string StartStreamWithStreamSelection(string identifier, string profileName, int startPosition, int audioId, int subtitleId)
        {
            Log.Debug("Called StartStreamWithStreamSelection with ident={0}; profile={1}; start={2}; audioId={3}; subtitleId={4}",
                identifier, profileName, startPosition, audioId, subtitleId);
            _stream.EndStream(identifier); // first end previous stream, if any available
            return _stream.StartStream(identifier, Configuration.Streaming.GetTranscoderProfileByName(profileName), startPosition * 1000, audioId, subtitleId);
        }

        public bool FinishStream(string identifier)
        {
            Log.Debug("Called FinishStream with ident={0}", identifier);
            _stream.KillStream(identifier);

            lock(_timeshiftings)
            {
                if (_timeshiftings.ContainsKey(identifier) && _timeshiftings[identifier] != null)
                {
                    Log.Info("Cancel timeshifting with identifier {0}",  identifier);
                    MPEServices.TAS.CancelCurrentTimeShifting("mpextended-" + identifier);
                    _timeshiftings.Remove(identifier);
                }
            }

            return true;
        }

        public Stream RetrieveStream(string identifier)
        {
            return _stream.RetrieveStream(identifier);
        }

        public Stream GetMediaItem(string clientDescription, WebStreamMediaType type, int? provider, string itemId)
        {
            if (!_authorizedHosts.Contains(WCFUtil.GetClientIPAddress()) && !NetworkInformation.IsLocalAddress(WCFUtil.GetClientIPAddress()))
            {
                Log.Warn("Host {0} isn't authorized to call GetMediaItem", WCFUtil.GetClientIPAddress());
                WCFUtil.SetResponseCode(HttpStatusCode.Unauthorized);
                return Stream.Null;
            }

            try
            {
                return _downloads.Download(clientDescription, type, provider, itemId);
            }
            catch (Exception ex)
            {
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                Log.Info(String.Format("GetMediaItem() failed for type={0}; provider={1}; itemId={2}", type, provider, itemId), ex);
                return Stream.Null;
            }
        }

        public Stream CustomTranscoderData(string identifier, string action, string parameters)
        {
            return _stream.CustomTranscoderData(identifier, action, parameters);
        }

        public Stream DoStream(WebStreamMediaType type, int? provider, string itemId, string clientDescription, string profileName, int startPosition)
        {
            if (!_authorizedHosts.Contains(WCFUtil.GetClientIPAddress()) && !NetworkInformation.IsLocalAddress(WCFUtil.GetClientIPAddress()))
            {
                Log.Warn("Host {0} isn't authorized to call DoStream", WCFUtil.GetClientIPAddress());
                WCFUtil.SetResponseCode(HttpStatusCode.Unauthorized);
                return Stream.Null;
            }

            // This only works with profiles that actually return something in the RetrieveStream method (i.e. no RTSP or CustomTranscoderData)
            string identifier = String.Format("dostream-{0}", new Random().Next(10000, 99999));
            Log.Debug("DoStream: using identifier {0}", identifier);

            if (!InitStream(type, provider, itemId, clientDescription, identifier, 2))
            {
                Log.Info("DoStream: InitStream() failed");
                FinishStream(identifier);
                return Stream.Null;
            }

            if (StartStream(identifier, profileName, startPosition).Length == 0)
            {
                Log.Info("DoStream: StartStream failed");
                FinishStream(identifier);
                return Stream.Null;
            }

            Log.Trace("DoStream: succeeded, returning stream");
            return RetrieveStream(identifier);
        }
        #endregion

        #region Images
        public Stream ExtractImage(WebStreamMediaType type, int? provider, string itemId, int position)
        {
            return Images.ExtractImage(new MediaSource(type, provider, itemId), position, null, null);
        }

        public Stream ExtractImageResized(WebStreamMediaType type, int? provider, string itemId, int position, int maxWidth, int maxHeight)
        {
            int? calcMaxWidth = maxWidth == 0 ? null : (int?)maxWidth;
            int? calcMaxHeight = maxHeight == 0 ? null : (int?)maxHeight;
            return Images.ExtractImage(new MediaSource(type, provider, itemId), position, calcMaxWidth, calcMaxHeight);
        }

        public Stream GetImage(WebStreamMediaType type, int? provider, string id)
        {
            return Images.GetImage(new ImageMediaSource(type, provider, id, WebArtworkType.Content, 0));
        }

        public Stream GetImageResized(WebStreamMediaType type, int? provider, string id, int maxWidth, int maxHeight)
        {
            int? calcMaxWidth = maxWidth == 0 ? null : (int?)maxWidth;
            int? calcMaxHeight = maxHeight == 0 ? null : (int?)maxHeight;
            return Images.GetResizedImage(new ImageMediaSource(type, provider, id, WebArtworkType.Content, 0), calcMaxWidth, calcMaxHeight);
        }

        public Stream GetArtwork(WebStreamMediaType mediatype, int? provider, string id, WebArtworkType artworktype, int offset)
        {
            return Images.GetImage(new ImageMediaSource(mediatype, provider, id, artworktype, offset));
        }

        public Stream GetArtworkResized(WebStreamMediaType mediatype, int? provider, string id, WebArtworkType artworktype, int offset, int maxWidth, int maxHeight)
        {
            int? calcMaxWidth = maxWidth == 0 ? null : (int?)maxWidth;
            int? calcMaxHeight = maxHeight == 0 ? null : (int?)maxHeight;
            return Images.GetResizedImage(new ImageMediaSource(mediatype, provider, id, artworktype, offset), calcMaxWidth, calcMaxHeight);
        }
        #endregion
    }
}