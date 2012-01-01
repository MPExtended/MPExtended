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
    public class StreamingService : IWebStreamingService, IStreamingService, IDisposable
    {
        private static Dictionary<string, WebVirtualCard> _timeshiftings = new Dictionary<string, WebVirtualCard>();
        private const int API_VERSION = 2;

        private Streaming _stream;

        public StreamingService()
        {
            _stream = new Streaming(this);
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
            return Configuration.Streaming.Transcoders.Select(x => x.CopyToWebTranscoderProfile()).ToList();
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

            return MediaInfo.MediaInfoWrapper.GetMediaInfo(new MediaSource(type, provider, itemId));
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
            return _stream.GetStreamingSessions();
        }

        public WebResolution GetStreamSize(WebStreamMediaType type, int? provider, string itemId, string profile)
        {
            return _stream.CalculateSize(Configuration.Streaming.GetTranscoderProfileByName(profile), new MediaSource(type, provider, itemId)).ToWebResolution();
        }
        #endregion

        #region Streaming
        public bool InitStream(WebStreamMediaType type, int? provider, string itemId, string clientDescription, string identifier)
        {
            if (type == WebStreamMediaType.TV)
            {
                int channelId = Int32.Parse(itemId);
                lock (_timeshiftings)
                {
                    Log.Info("Starting timeshifting on channel {0} for client {1} with identifier {2}", channelId, clientDescription, identifier);
                    var card = MPEServices.TAS.SwitchTVServerToChannelAndGetVirtualCard("webstreamingservice-" + identifier, channelId);
                    Log.Debug("Timeshifting started!");
                    _timeshiftings[identifier] = card;
                    itemId = card.TimeShiftFileName;
                }
            }

            Log.Info("Called InitStream with type={0}; provider={1}; itemId={2}; clientDescription={3}; identifier={4}", type, provider, itemId, clientDescription, identifier);
            return _stream.InitStream(identifier, clientDescription, new MediaSource(type, provider, itemId));
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
                    MPEServices.TAS.CancelCurrentTimeShifting("webstreamingservice-" + identifier);
                    _timeshiftings.Remove(identifier);
                }
            }

            return true;
        }

        public Stream RetrieveStream(string identifier)
        {
            return _stream.RetrieveStream(identifier);
        }

        public Stream GetMediaItem(WebStreamMediaType type, int? provider, string itemId)
        {
            MediaSource source = new MediaSource(type, provider, itemId);
            try
            {
                if (!source.Exists)
                {
                    throw new FileNotFoundException();
                }
                
                WCFUtil.AddHeader("Content-Disposition", "attachment; filename=\"" + source.GetFileInfo().Name + "\"");

                string mime = ContentTypeUtil.GetContentTypeFromExtension(Path.GetExtension(source.GetFileInfo().Name));
                if (mime != null)
                {
                    WCFUtil.AddHeader("Content-Type", mime);
                }

                return source.Retrieve();
            }
            catch (Exception ex)
            {
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                Log.Info(String.Format("GetMediaItem() failed for {0}", source.GetDebugName()), ex);
                return Stream.Null;
            }
        }

        public Stream CustomTranscoderData(string identifier, string action, string parameters)
        {
            return _stream.CustomTranscoderData(identifier, action, parameters);
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