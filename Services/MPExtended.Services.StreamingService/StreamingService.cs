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
using MPExtended.Libraries.Service.Network;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;
using System.Threading;

namespace MPExtended.Services.StreamingService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class StreamingService : IWebStreamingService, IStreamingService, IDisposable
    {
        private static Dictionary<string, WebVirtualCard> _timeshiftings = new Dictionary<string, WebVirtualCard>();
        private static List<string> _authorizedHosts = new List<string>();
        private const int API_VERSION = 5;

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
            bool hasTv = Connections.Current.HasTASConnection; // takes a while so don't execute it twice
            return new WebStreamServiceDescription()
            {
                SupportsMedia = Connections.Current.HasMASConnection,
                SupportsRecordings = hasTv,
                SupportsTV = hasTv,
                ServiceVersion = VersionUtil.GetVersionName(),
                ApiVersion = API_VERSION,
            };
        }

        private bool IsClientAuthorized()
        {
            return _authorizedHosts.Contains(WCFUtil.GetClientIPAddress()) ||
                        NetworkInformation.IsLocalAddress(WCFUtil.GetClientIPAddress()) ||
                        !Configuration.Authentication.Enabled ||
                        Configuration.Authentication.UnauthorizedStreams;
        }

        #region Profiles
        public List<WebTranscoderProfile> GetTranscoderProfiles()
        {
            return Configuration.StreamingProfiles.Transcoders.Select(x => x.ToWebTranscoderProfile()).ToList();
        }

        public List<WebTranscoderProfile> GetTranscoderProfilesForTarget(string target)
        {
            return GetTranscoderProfiles().Where(x => x.Targets.Contains(target)).ToList();
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
        public WebMediaInfo GetMediaInfo(WebMediaType type, int? provider, string itemId, int? offset)
        {
            if (type == WebMediaType.TV)
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

            return MediaInfoHelper.LoadMediaInfoOrSurrogate(new MediaSource(type, provider, itemId, offset));
        }

        public WebTranscodingInfo GetTranscodingInfo(string identifier, long? playerPosition)
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

        public WebResolution GetStreamSize(WebMediaType type, int? provider, string itemId, int? offset, string profile)
        {
            if (type == WebMediaType.TV)
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
                    return _stream.CalculateSize(Configuration.StreamingProfiles.GetTranscoderProfileByName(profile), MediaInfoHelper.DEFAULT_ASPECT_RATIO).ToWebResolution();
                }
            }

            return _stream.CalculateSize(Configuration.StreamingProfiles.GetTranscoderProfileByName(profile), new MediaSource(type, provider, itemId, offset)).ToWebResolution();
        }

        public WebBoolResult AuthorizeStreaming()
        {
            if (!_authorizedHosts.Contains(WCFUtil.GetClientIPAddress()))
            {
                _authorizedHosts.Add(WCFUtil.GetClientIPAddress());
            }
            return true;
        }

        public WebBoolResult AuthorizeRemoteHostForStreaming(string host)
        {
            if (!_authorizedHosts.Contains(host))
            {
                _authorizedHosts.Add(host);
            }
            return true;
        }

        public WebItemSupportStatus GetItemSupportStatus(WebMediaType type, int? provider, string itemId, int? offset)
        {
            // check if we actually now about this file
            MediaSource source = new MediaSource(type, provider, itemId, offset);
            string path = source.GetPath();
            if (path == null || path.Length == 0)
            {
                return new WebItemSupportStatus(false, "Cannot resolve item to a path");
            }

            // some checks based upon the file info. apparantly people have broken files in their connections
            var fileinfo = source.GetFileInfo();
            if (!fileinfo.Exists)
            {
                // add a special warning message for files that are on a network drive, as this often causes problems
                Uri uri = new Uri(path);
                if (uri.IsUnc && !NetworkInformation.IsLocalAddress(uri.Host))
                {
                    return new WebItemSupportStatus(false, "File is on an inaccessible network share");
                }

                return new WebItemSupportStatus(false, "File does not exists or is inaccessible");
            }
            if (fileinfo.Size == 0)
            {
                return new WebItemSupportStatus(false, "This file has a size of 0KB");
            }

            // we don't support some things yet
            if (path.EndsWith(".IFO"))
            {
                return new WebItemSupportStatus(false, "Streaming DVD files is not supported");
            }

            // while corrupt files may work, it's probably a better idea to warn early. check for a valid file using mediainfo
            if (MediaInfo.MediaInfoWrapper.GetMediaInfo(source) == null)
            {
                return new WebItemSupportStatus(false, "This file might be corrupt");
            }

            return new WebItemSupportStatus() { Supported = true };
        }
        #endregion

        #region Streaming
        public WebBoolResult InitStream(WebMediaType type, int? provider, string itemId, int? offset, string clientDescription, string identifier, int? idleTimeout)
        {
            AuthorizeStreaming();
            if (type == WebMediaType.TV)
            {
                int channelId = Int32.Parse(itemId);
                lock (_timeshiftings)
                {
                    Log.Info("Starting timeshifting on channel {0} for client {1} with identifier {2}", channelId, clientDescription, identifier);
                    var card = Connections.TAS.SwitchTVServerToChannelAndGetVirtualCard("mpextended-" + identifier, channelId);
                    if (card == null)
                    {
                        Log.Error("Failed to start timeshifting for stream with identifier {0}", identifier);
                        return false;
                    }
                    else
                    {
                        Log.Debug("Timeshifting started!");
                        _timeshiftings[identifier] = card;
                        itemId = card.TimeShiftFileName;
                    }
                }
                Thread.Sleep(5000);
            }

            Log.Info("Called InitStream with type={0}; provider={1}; itemId={2}; offset={3}; clientDescription={4}; identifier={5}; idleTimeout={6}", 
                type, provider, itemId, offset, clientDescription, identifier, idleTimeout);
            return _stream.InitStream(identifier, clientDescription, new MediaSource(type, provider, itemId, offset), idleTimeout.HasValue ? idleTimeout.Value : 5 * 60);
        }

        public WebStringResult StartStream(string identifier, string profileName, long startPosition)
        {
            Log.Debug("Called StartStream with ident={0}; profile={1}; start={2}", identifier, profileName, startPosition);
            _stream.EndStream(identifier); // first end previous stream, if any available
            return _stream.StartStream(identifier, Configuration.StreamingProfiles.GetTranscoderProfileByName(profileName), startPosition * 1000);
        }

        public WebStringResult StartStreamWithStreamSelection(string identifier, string profileName, long startPosition, int audioId, int subtitleId)
        {
            Log.Debug("Called StartStreamWithStreamSelection with ident={0}; profile={1}; start={2}; audioId={3}; subtitleId={4}",
                identifier, profileName, startPosition, audioId, subtitleId);
            _stream.EndStream(identifier); // first end previous stream, if any available
            return _stream.StartStream(identifier, Configuration.StreamingProfiles.GetTranscoderProfileByName(profileName), startPosition * 1000, audioId, subtitleId);
        }

        public WebBoolResult StopStream(string identifier)
        {
            Log.Debug("Called StopStream with identifier={0}", identifier);
            _stream.EndStream(identifier);
            return true;
        }

        public WebBoolResult FinishStream(string identifier)
        {
            Log.Debug("Called FinishStream with identifier={0}", identifier);
            _stream.KillStream(identifier);

            lock(_timeshiftings)
            {
                if (_timeshiftings.ContainsKey(identifier) && _timeshiftings[identifier] != null)
                {
                    Log.Info("Cancel timeshifting with identifier {0}",  identifier);
                    Connections.TAS.CancelCurrentTimeShifting("mpextended-" + identifier);
                    _timeshiftings.Remove(identifier);
                }
            }

            return true;
        }

        public Stream RetrieveStream(string identifier)
        {
            return _stream.RetrieveStream(identifier);
        }

        public Stream GetMediaItem(string clientDescription, WebMediaType type, int? provider, string itemId, long? startPosition)
        {
            if (!IsClientAuthorized())
            {
                Log.Warn("Host {0} isn't authorized to call GetMediaItem", WCFUtil.GetClientIPAddress());
                WCFUtil.SetResponseCode(HttpStatusCode.Unauthorized);
                return Stream.Null;
            }

            try
            {
                return _downloads.Download(clientDescription, type, provider, itemId, startPosition);
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

        public Stream DoStream(WebMediaType type, int? provider, string itemId, string clientDescription, string profileName, long startPosition, int? idleTimeout)
        {
            if (!IsClientAuthorized())
            {
                Log.Warn("Host {0} isn't authorized to call DoStream", WCFUtil.GetClientIPAddress());
                WCFUtil.SetResponseCode(HttpStatusCode.Unauthorized);
                return Stream.Null;
            }

            // calculate timeout, which is by default 5 minutes for direct streaming and 5 seconds for transcoded streams
            var profile = Configuration.StreamingProfiles.Transcoders.FirstOrDefault(x => x.Name == profileName);
            if(profile == null)
            {
                Log.Warn("Tried DoStream with non-existing profile {0}", profileName);
                return Stream.Null;
            }
            int timeout = profile.Transcoder == typeof(Transcoders.Direct).FullName ? 5 * 60 : 5;
            if (idleTimeout.HasValue)
                timeout = idleTimeout.Value;

            // This only works with profiles that actually return something in the RetrieveStream method (i.e. no RTSP or CustomTranscoderData)
            string identifier = String.Format("dostream-{0}", new Random().Next(10000, 99999));
            Log.Debug("DoStream: using identifier {0} and timeout={1}", identifier, timeout);

            if (!InitStream(type, provider, itemId, null, clientDescription, identifier, timeout))
            {
                Log.Info("DoStream: InitStream() failed");
                FinishStream(identifier);
                return Stream.Null;
            }

            if (String.IsNullOrEmpty(StartStream(identifier, profileName, startPosition)))
            {
                Log.Info("DoStream: StartStream failed");
                FinishStream(identifier);
                return Stream.Null;
            }

            Log.Debug("DoStream: succeeded, returning stream");
            return RetrieveStream(identifier);
        }
        #endregion

        #region Images
        public Stream ExtractImage(WebMediaType type, int? provider, string itemId, long position, string format = null)
        {
            return Images.ExtractImage(new MediaSource(type, provider, itemId), position, format);
        }

        public Stream ExtractImageResized(WebMediaType type, int? provider, string itemId, long position, int maxWidth, int maxHeight, string borders = null, string format = null)
        {
            int? calcMaxWidth = maxWidth == 0 ? null : (int?)maxWidth;
            int? calcMaxHeight = maxHeight == 0 ? null : (int?)maxHeight;
            return Images.ExtractImage(new MediaSource(type, provider, itemId), position, calcMaxWidth, calcMaxHeight, borders, format);
        }

        public Stream GetImage(WebMediaType type, int? provider, string id, string format = null)
        {
            return Images.GetImage(new ImageMediaSource(type, provider, id, WebFileType.Content, 0), format);
        }

        public Stream GetImageResized(WebMediaType type, int? provider, string id, int maxWidth, int maxHeight, string borders = null, string format = null)
        {
            int? calcMaxWidth = maxWidth == 0 ? null : (int?)maxWidth;
            int? calcMaxHeight = maxHeight == 0 ? null : (int?)maxHeight;
            return Images.GetResizedImage(new ImageMediaSource(type, provider, id, WebFileType.Content, 0), calcMaxWidth, calcMaxHeight, borders, format);
        }

        public Stream GetArtwork(WebMediaType mediatype, int? provider, string id, WebFileType artworktype, int offset, string format = null)
        {
            return Images.GetImage(new ImageMediaSource(mediatype, provider, id, artworktype, offset), format);
        }

        public Stream GetArtworkResized(WebMediaType mediatype, int? provider, string id, WebFileType artworktype, int offset, int maxWidth, int maxHeight, string borders = null, string format = null)
        {
            int? calcMaxWidth = maxWidth == 0 ? null : (int?)maxWidth;
            int? calcMaxHeight = maxHeight == 0 ? null : (int?)maxHeight;
            return Images.GetResizedImage(new ImageMediaSource(mediatype, provider, id, artworktype, offset), calcMaxWidth, calcMaxHeight, borders, format);
        }
        #endregion
    }
}