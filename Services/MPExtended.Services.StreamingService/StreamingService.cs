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
using System.Reflection;
using System.ServiceModel;
using MPExtended.Libraries.General;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Services.StreamingService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class StreamingService : IWebStreamingService, IStreamingService
    {
        private static Dictionary<string, WebVirtualCard> _timeshiftings = new Dictionary<string, WebVirtualCard>();
        private const int API_VERSION = 2;

        private Streaming _stream;

        public StreamingService()
        {
            _stream = new Streaming();
            WcfUsernameValidator.Init();
        }

        public WebStreamServiceDescription GetServiceDescription()
        {
            bool hasTv = MPEServices.HasTVAccessConnection; // takes a while so don't execute it twice
            return new WebStreamServiceDescription()
            {
                SupportsMedia = MPEServices.HasMediaAccessConnection,
                SupportsRecordings = hasTv,
                SupportsTV = hasTv,
                ServiceVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion,
                ApiVersion = API_VERSION,
            };
        }

        #region Profiles
        public List<WebTranscoderProfile> GetTranscoderProfiles()
        {
            // apparantly you can't serialize a derived classes to a parent class. That sucks.
            return Config.GetTranscoderProfiles().Select(x => x.CopyToWebTranscoderProfile()).ToList();
        }

        public List<WebTranscoderProfile> GetTranscoderProfilesForTarget(string target)
        {
            return Config.GetTranscoderProfiles().Where(s => s.Target == target).Select(x => x.CopyToWebTranscoderProfile()).ToList();
        }

        public WebTranscoderProfile GetTranscoderProfileByName(string name)
        {
            TranscoderProfile profile = Config.GetTranscoderProfileByName(name);
            if (profile == null)
                return null;

            return profile.CopyToWebTranscoderProfile();
        }
        #endregion

        #region Info methods
        public WebMediaInfo GetMediaInfo(WebStreamMediaType type, string itemId)
        {
            if (type == WebStreamMediaType.TV)
            {
                itemId = _timeshiftings[itemId].TimeShiftFileName;
            }

            return MediaInfo.MediaInfoWrapper.GetMediaInfo(new MediaSource(type, itemId));
        }

        public WebTranscodingInfo GetTranscodingInfo(string identifier)
        {
            return _stream.GetEncodingInfo(identifier);
        }

        public List<WebStreamingSession> GetStreamingSessions()
        {
            return _stream.GetStreamingSessions();
        }

        public WebResolution GetStreamSize(WebStreamMediaType type, string itemId, string profile)
        {
            return _stream.CalculateSize(Config.GetTranscoderProfileByName(profile), new MediaSource(type, itemId)).ToWebResolution();
        }
        #endregion

        #region Streaming
        public bool InitStream(WebStreamMediaType type, string itemId, string clientDescription, string identifier)
        {
            if (type == WebStreamMediaType.TV)
            {
                int channelId = Int32.Parse(itemId);
                lock (_timeshiftings)
                {
                    Log.Info("Starting timeshifting on channel {0} for client {1} with identifier {2}", channelId, clientDescription, identifier);
                    var card = MPEServices.NetPipeTVAccessService.SwitchTVServerToChannelAndGetVirtualCard("webstreamingservice-" + identifier, channelId);
                    Log.Debug("Timeshifting started!");
                    _timeshiftings[identifier] = card;
                    itemId = card.TimeShiftFileName;
                }
            }

            return _stream.InitStream(identifier, clientDescription, new MediaSource(type, itemId));
        }

        public string StartStream(string identifier, string profileName, int startPosition)
        {
            Log.Debug("Called StartStream with ident={0}; profile={1}; start={2}", identifier, profileName, startPosition);
            _stream.EndStream(identifier); // first end previous stream, if any available
            return _stream.StartStream(identifier, Config.GetTranscoderProfileByName(profileName), startPosition);
        }

        public string StartStreamWithStreamSelection(string identifier, string profileName, int startPosition, int audioId, int subtitleId)
        {
            Log.Debug("Called StartStreamWithStreamSelection with ident={0}; profile={1}; start={2}; audioId={3}; subtitleId={4}",
                identifier, profileName, startPosition, audioId, subtitleId);
            _stream.EndStream(identifier); // first end previous stream, if any available
            return _stream.StartStream(identifier, Config.GetTranscoderProfileByName(profileName), startPosition, audioId, subtitleId);
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
                    MPEServices.NetPipeTVAccessService.CancelCurrentTimeShifting("webstreamingservice-" + identifier);
                    _timeshiftings.Remove(identifier);
                }
            }

            return true;
        }

        public Stream RetrieveStream(string identifier)
        {
            return _stream.RetrieveStream(identifier);
        }

        public Stream GetMediaItem(WebStreamMediaType type, string itemId)
        {
            MediaSource source = new MediaSource(type, itemId);
            return source.Retrieve();
        }

        public Stream CustomTranscoderData(string identifier, string action, string parameters)
        {
            return _stream.CustomTranscoderData(identifier, action, parameters);
        }
        #endregion

        #region Images
        public Stream ExtractImage(WebStreamMediaType type, string itemId, int position)
        {
            return WrapInErrorHandler(delegate()
            {
                return Images.ExtractImage(new MediaSource(type, itemId), position, null, null);
            });
        }

        public Stream ExtractImageResized(WebStreamMediaType type, string itemId, int position, int maxWidth, int maxHeight)
        {
            return WrapInErrorHandler(delegate()
            {
                return Images.ExtractImage(new MediaSource(type, itemId), position, maxWidth, maxHeight);
            });
        }

        public Stream GetImage(WebStreamMediaType type, string id)
        {
            return WrapInErrorHandler(delegate()
            {
                return Images.GetImage(type, id);
            });
        }

        public Stream GetImageResized(WebStreamMediaType type, string id, int maxWidth, int maxHeight)
        {
            return WrapInErrorHandler(delegate()
            {
                return Images.GetResizedImage(type, id, maxWidth, maxHeight);
            });
        }

        public Stream GetArtwork(WebStreamMediaType mediatype, WebArtworkType artworktype, string id, int offset)
        {
            return WrapInErrorHandler(delegate() 
            { 
                return Images.GetImage(mediatype, artworktype, id, offset); 
            });
        }

        public Stream GetArtworkResized(WebStreamMediaType mediatype, WebArtworkType artworktype, string id, int offset, int maxWidth, int maxHeight)
        {
            return WrapInErrorHandler(delegate()
            {
                return Images.GetResizedImage(mediatype, artworktype, id, offset, maxWidth, maxHeight);
            });
        }

        private delegate Stream DoOperation();
        private Stream WrapInErrorHandler(DoOperation operation)
        {
            try
            {
                return operation.Invoke();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Info("Didn't found image", ex);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return null;
            }
            catch (Exception ex)
            {
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.InternalServerError);
                Log.Warn("Exception while trying to do image operation", ex);
                return null;
            }
        }
        #endregion
    }
}
