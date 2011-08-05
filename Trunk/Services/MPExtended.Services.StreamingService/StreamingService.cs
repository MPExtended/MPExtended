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
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Util;
using MASInterfaces = MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.StreamingService
{
    public class StreamingService : IWebStreamingService, IStreamingService
    {
        private Streaming _stream;
        private const int API_VERSION = 2;

        public StreamingService()
        {
            _stream = new Streaming();
            WcfUsernameValidator.Init();
        }

        private string ResolvePath(WebMediaType type, string itemId)
        {
            if (type != WebMediaType.RecordingItem)
            {
                return WebServices.Media.GetPath((MASInterfaces.MediaItemType)type, itemId);
            }
            else
            {
                int id = Int32.Parse(itemId);
                return WebServices.TV.GetRecordings().Where(r => r.IdRecording == id).Select(r => r.FileName).FirstOrDefault();
            }
        }

        public WebServiceDescription GetServiceDescription()
        {
            bool hasTv = WebServices.HasTVConnection; // takes a while so don't execute it twice
            return new WebServiceDescription()
            {
                SupportsMedia = WebServices.HasMediaConnection,
                SupportsRecordings = hasTv,
                SupportsTV = hasTv,
                ServiceVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion,
                ApiVersion = API_VERSION,
            };
        }

        #region Profiles
        public List<WebTranscoderProfile> GetTranscoderProfiles()
        {
            return Config.GetTranscoderProfiles().Select(s => s.ToWebTranscoderProfile()).ToList();
        }

        public List<WebTranscoderProfile> GetTranscoderProfilesForTarget(string target)
        {
            return Config.GetTranscoderProfiles().Where(s => s.Target == target).Select(s => s.ToWebTranscoderProfile()).ToList();
        }

        public WebTranscoderProfile GetTranscoderProfileByName(string name)
        {
            TranscoderProfile profile = Config.GetTranscoderProfileByName(name);
            if (profile == null)
                return null;

            return profile.ToWebTranscoderProfile();
        }
        #endregion

        #region Info methods
        public WebMediaInfo GetMediaInfo(WebMediaType type, string itemId)
        {
            string path = ResolvePath(type, itemId);
            if (path == null)
            {
                Log.Warn("GetMediaInfo called with unknown path; type={0}; itemId={1}", type, itemId);
                return null;
            }
            return MediaInfo.MediaInfoWrapper.GetMediaInfo(ResolvePath(type, itemId));
        }

        public WebMediaInfo GetTVMediaInfo(string identifier)
        {
            TsBuffer buffer = new TsBuffer(WebServices.GetTimeshifting(identifier).TimeShiftFileName);
            return MediaInfo.MediaInfoWrapper.GetMediaInfo(buffer);
        }

        public WebTranscodingInfo GetTranscodingInfo(string identifier)
        {
            EncodingInfo info = _stream.GetEncodingInfo(identifier);
            if (info != null)
                return info.ToWebTranscodingInfo();
            return null;
        }

        public List<WebStreamingSession> GetStreamingSessions()
        {
            return _stream.GetStreamingSessions();
        }

        public WebResolution GetStreamSize(WebMediaType type, string itemId, string profile)
        {
            return _stream.CalculateSize(Config.GetTranscoderProfileByName(profile), ResolvePath(type, itemId), false).ToWebResolution();
        }

        public WebResolution GetTVStreamSize(int channelId, string profile)
        {
            return _stream.CalculateSize(Config.GetTranscoderProfileByName(profile), null, true).ToWebResolution();
        }
        #endregion

        #region Streaming
        public bool InitTVStream(int channelId, string clientDescription, string identifier)
        {
            Log.Info("Starting timeshifting on channel {0} for client {1} with identifier {2}", channelId, clientDescription, identifier);
            var card = WebServices.TV.SwitchTVServerToChannelAndGetVirtualCard("webstreamingservice-" + identifier, channelId);
            Log.Debug("Timeshifting started!");
            WebServices.SaveTimeshifting(identifier, card);
            return _stream.InitStream(identifier, clientDescription, card.TimeShiftFileName);
        }

        public bool InitStream(WebMediaType type, string itemId, string clientDescription, string identifier)
        {
            string path = ResolvePath(type, itemId);
            if (path == null)
            {
                Log.Warn("Called InitStream with invalid path: type={0}, itemId={1}", type, itemId);
                return false;
            }

            return _stream.InitStream(identifier, clientDescription, path);
        }

        public bool StartStream(string identifier, string profileName, int startPosition)
        {
            Log.Debug("Called StartStream with ident={0}; profile={1}; start={2}", identifier, profileName, startPosition);
            _stream.EndStream(identifier); // first end previous stream, if any available
            return _stream.StartStream(identifier, Config.GetTranscoderProfileByName(profileName), startPosition, null, null);
        }

        public bool StartStreamWithStreamSelection(string identifier, string profileName, int startPosition, int audioId, int subtitleId)
        {
            Log.Debug("Called StartStreamWithStreamSelection with ident={0}; profile={1}; start={2}; audioId={3}; subtitleId={4}",
                identifier, profileName, startPosition, audioId, subtitleId);
            _stream.EndStream(identifier); // first end previous stream, if any available
            return _stream.StartStream(identifier, Config.GetTranscoderProfileByName(profileName), startPosition,
                audioId == -1 ? (int?)null : audioId, subtitleId == -1 ? (int?)null : subtitleId);
        }

        public bool FinishStream(string identifier)
        {
            Log.Debug("Called FinishStream with ident={0}", identifier);
            _stream.KillStream(identifier);
            if (WebServices.GetTimeshifting(identifier) != null)
            {
                WebServices.TV.CancelCurrentTimeShifting("webstreamingservice-" + identifier);
                WebServices.SaveTimeshifting(identifier, null);
            }
            return true;
        }

        public Stream RetrieveStream(string identifier)
        {
            return _stream.RetrieveStream(identifier);
        }

        public Stream GetMediaItem(WebMediaType type, string itemId)
        {
            string path = ResolvePath(type, itemId);
            if (path == null)
            {
                Log.Warn("Called GetMediaItem with invalid path: type={0}, itemId={1}", type, itemId);
                return null;
            }

            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
        #endregion

        #region Images
        public Stream ExtractImage(WebMediaType type, string itemId, int position)
        {
            return Images.ExtractImage(ResolvePath(type, itemId), position, null, null);
        }

        public Stream ExtractImageResized(WebMediaType type, string itemId, int position, int maxWidth, int maxHeight)
        {
            return Images.ExtractImage(ResolvePath(type, itemId), position, maxWidth, maxHeight);
        }

        public Stream GetImage(string path)
        {
            return Images.GetImage(path);
        }

        public Stream GetImageResized(string path, int maxWidth, int maxHeight)
        {
            return Images.GetImageResized(path, maxWidth, maxHeight);
        }
        #endregion
    }
}
