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
using System.ServiceModel;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
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
            return _stream.StartStream(identifier, Config.GetTranscoderProfileByName(profileName), startPosition, null, null);
        }

        public string StartStreamWithStreamSelection(string identifier, string profileName, int startPosition, int audioId, int subtitleId)
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

            lock(_timeshiftings)
            {
                if (_timeshiftings.ContainsKey(identifier) && _timeshiftings[identifier] != null)
                {
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
            return Images.ExtractImage(new MediaSource(type, itemId), position, null, null);
        }

        public Stream ExtractImageResized(WebStreamMediaType type, string itemId, int position, int maxWidth, int maxHeight)
        {
            return Images.ExtractImage(new MediaSource(type, itemId), position, maxWidth, maxHeight);
        }

        public Stream GetImage(WebStreamMediaType type, string id)
        {
            return Images.GetImage(new MediaSource(type, id));
        }

        public Stream GetImageResized(WebStreamMediaType type, string id, int maxWidth, int maxHeight)
        {
            return Images.GetImageResized(new MediaSource(type, id), maxWidth, maxHeight);
        }

        public Stream GetArtwork(WebArtworkType artworktype, WebArtworkSource source, string id, int offset, int maxWidth, int maxHeight)
        {
            // TODO: proper implementation
            var items = new[] {
                new { Source = WebArtworkSource.Movie,    Artwork = WebArtworkType.Backdrop, Method = "GetMovieBackdrop" },
                new { Source = WebArtworkSource.Movie,    Artwork = WebArtworkType.Cover,    Method = "GetMovieCover"    },
                new { Source = WebArtworkSource.Music,    Artwork = WebArtworkType.Backdrop, Method = "GetMusicBackdrop" },
                new { Source = WebArtworkSource.Music,    Artwork = WebArtworkType.Cover,    Method = "GetMusicCover"    },
                new { Source = WebArtworkSource.TVShow,   Artwork = WebArtworkType.Backdrop, Method = "GetTVShowBackdrop" },
                new { Source = WebArtworkSource.TVShow,   Artwork = WebArtworkType.Banner,   Method = "GetTVShowBanner" },
                new { Source = WebArtworkSource.TVShow,   Artwork = WebArtworkType.Poster,   Method = "GetTVShowPoster" },
                //new { Source = WebArtworkSource.TVSeason, Artwork = WebArtworkType.Backdrop, Method = "GetTVSeasonBackdrop" },
                //new { Source = WebArtworkSource.TVSeason, Artwork = WebArtworkType.Banner,   Method = "GetTVSeasonBanner" },
                //new { Source = WebArtworkSource.TVSeason, Artwork = WebArtworkType.Poster,   Method = "GetTVSeasonPoster" },
            };

            var valid = items.Select(x => x.Source == source && x.Artwork == artworktype);
            if (valid.Count() == 0)
                return null;

            Type t = typeof(IMediaAccessService);
            Stream input = (Stream)t.InvokeMember(items.First().Method, BindingFlags.InvokeMethod, null, MPEServices.NetPipeMediaAccessService, new object[] { id, offset });

            return Images.GetResizedImageFromStream(input, maxWidth, maxHeight);
        }
        #endregion
    }
}
