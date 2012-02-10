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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal class MediaSource
    {
        private WebFileInfo fileInfoCache;

        public WebStreamMediaType MediaType { get; private set; }
        public WebArtworkType FileType { get; private set; }
        public string Id { get; private set; } // path to tsbuffer for TV
        public int? Provider { get; private set; } // for MAS
        public int Offset { get; private set; }

        public virtual bool Exists
        {
            get
            {
                return MediaType == WebStreamMediaType.TV && FileType == WebArtworkType.Content ? File.Exists(Id) : GetFileInfo().Exists;
            }
        }

        public virtual bool SupportsDirectAccess
        {
            get
            {
                if (MediaType == WebStreamMediaType.TV || MediaType == WebStreamMediaType.Recording)
                {
                    return Exists && MPEServices.IsTASLocal && GetFileInfo().IsLocalFile;
                }
                else
                {
                    return Exists && MPEServices.IsMASLocal && GetFileInfo().IsLocalFile;
                }
            }
        }

        public virtual bool NeedsInputReaderUnit
        {
            get
            {
                return (MediaType == WebStreamMediaType.TV && FileType == WebArtworkType.Content) || !SupportsDirectAccess;
            }
        }

        public virtual bool NeedsImpersonation
        {
            get
            {
                bool impersonationEnabled = Configuration.Services.NetworkImpersonation.IsEnabled();
                return impersonationEnabled && SupportsDirectAccess && GetFileInfo().OnNetworkDrive && !File.Exists(GetPath());
            }
        }

        public MediaSource(WebStreamMediaType type, int? provider, string id, WebArtworkType filetype, int offset)
        {
            this.MediaType = type;
            this.Id = id;
            this.Provider = provider;
            this.Offset = offset;
            this.FileType = filetype;

            if (!CheckArguments(type, filetype))
            {
                throw new ArgumentException("Invalid combination of mediatype and filetype");
            }
        }

        public MediaSource(WebStreamMediaType type, int? provider, string id, WebArtworkType filetype)
            : this(type, provider, id, filetype, 0)
        {
        }

        public MediaSource(WebStreamMediaType type, int? provider, string id)
            : this(type, provider, id, WebArtworkType.Content, 0)
        {
        }

        public MediaSource(WebMediaType type, int? provider, string id, WebArtworkType filetype, int offset)
            : this((WebStreamMediaType)type, provider, id, filetype, offset)
        {
        }

        public MediaSource(WebMediaType type, int? provider, string id, WebArtworkType filetype)
            : this((WebStreamMediaType)type, provider, id, filetype, 0)
        {
        }

        public MediaSource(WebMediaType type, int? provider, string id)
            : this((WebStreamMediaType)type, provider, id, WebArtworkType.Content, 0)
        {
        }

        protected virtual bool CheckArguments(WebStreamMediaType mediatype, WebArtworkType filetype)
        {
            return !(
                        (mediatype == WebStreamMediaType.TV && FileType != WebArtworkType.Content) ||
                        (mediatype == WebStreamMediaType.Recording && FileType != WebArtworkType.Content)
                    );
        }

        public virtual WebFileInfo GetFileInfo()
        {
            if(fileInfoCache != null)
            {
                return fileInfoCache;
            }

            if (MediaType == WebStreamMediaType.Recording && FileType == WebArtworkType.Content)
            {
                WebRecordingFileInfo info = MPEServices.TAS.GetRecordingFileInfo(Int32.Parse(Id));
                fileInfoCache = new WebFileInfo()
                {
                    Exists = info.Exists,
                    Extension = info.Extension,
                    IsLocalFile = info.IsLocalFile,
                    IsReadOnly = info.IsReadOnly,
                    LastAccessTime = info.LastAccessTime,
                    LastModifiedTime = info.LastModifiedTime,
                    Name = info.Name,
                    OnNetworkDrive = info.OnNetworkDrive,
                    Path = info.Path,
                    PID = -1,
                    Size = info.Size
                };
                return fileInfoCache;
            }

            if (MediaType == WebStreamMediaType.TV && FileType == WebArtworkType.Content)
            {
                fileInfoCache = new WebFileInfo(new FileInfo(Id))
                {
                    Exists = true,
                    IsLocalFile = true,
                    IsReadOnly = true,
                    OnNetworkDrive = false,
                    PID = -1
                };
                return fileInfoCache;
            }

            fileInfoCache = MPEServices.MAS.GetFileInfo(Provider, (WebMediaType)MediaType, (WebFileType)FileType, Id, Offset);
            return fileInfoCache;
        }

        public string GetPath()
        {
            return MediaType == WebStreamMediaType.TV && FileType == WebArtworkType.Content ? Id : GetFileInfo().Path;
        }

        public IProcessingUnit GetInputReaderUnit()
        {
            if (SupportsDirectAccess)
            {
                // TV always has NeedsImpersonation = false and SupportsDirectAccess = true, so gets redirect to InputUnit
                return NeedsImpersonation ? (IProcessingUnit)(new ImpersonationInputUnit(GetPath())) : (IProcessingUnit)(new InputUnit(GetPath()));
            }
            else
            {
                return new InjectStreamUnit(Retrieve());
            }
        }

        public Stream Retrieve()
        {
            if (MediaType == WebStreamMediaType.TV && FileType == WebArtworkType.Content)
            {
                return new TsBuffer(Id);
            }
            else if (SupportsDirectAccess)
            {
                using (var impersonator = GetImpersonator())
                {
                    return new FileStream(GetPath(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
            }
            else if (MediaType == WebStreamMediaType.Recording)
            {
                return MPEServices.TAS.ReadRecordingFile(Int32.Parse(Id));
            }
            else
            {
                return MPEServices.MAS.RetrieveFile(Provider, (WebMediaType)MediaType, (WebFileType)FileType, Id, Offset);
            }
        }

        public NetworkShareImpersonator GetImpersonator()
        {
            return new NetworkShareImpersonator(NeedsImpersonation);
        }

        public virtual string GetMediaDisplayName()
        {
            try
            {
                switch (MediaType)
                {
                    case WebStreamMediaType.File:
                        return MPEServices.MAS.GetFileSystemFileBasicById(Provider, Id).Title;
                    case WebStreamMediaType.Movie:
                        return MPEServices.MAS.GetMovieBasicById(Provider, Id).Title;
                    case WebStreamMediaType.MusicAlbum:
                        return MPEServices.MAS.GetMusicAlbumBasicById(Provider, Id).Title;
                    case WebStreamMediaType.MusicTrack:
                        return MPEServices.MAS.GetMusicTrackBasicById(Provider, Id).Title;
                    case WebStreamMediaType.Picture:
                        return MPEServices.MAS.GetPictureBasicById(Provider, Id).Title;
                    case WebStreamMediaType.Recording:
                        return MPEServices.TAS.GetRecordingById(Int32.Parse(Id)).Title;
                    case WebStreamMediaType.TV:
                        int channelId;
                        if (!Int32.TryParse(Id, out channelId))
                        {
                            channelId = MPEServices.TAS.GetActiveCards().First(x => x.TimeShiftFileName == Id).IdChannel;
                        }
                        return MPEServices.TAS.GetChannelBasicById(channelId).DisplayName;
                    case WebStreamMediaType.TVEpisode:
                        var ep = MPEServices.MAS.GetTVEpisodeBasicById(Provider, Id);
                        var season = MPEServices.MAS.GetTVSeasonBasicById(Provider, ep.SeasonId);
                        var show = MPEServices.MAS.GetTVShowBasicById(Provider, ep.ShowId);
                        return String.Format("{0} ({1} {2}x{3})", ep.Title, show.Title, season.SeasonNumber, ep.EpisodeNumber);
                    case WebStreamMediaType.TVSeason:
                        var season2 = MPEServices.MAS.GetTVSeasonDetailedById(Provider, Id);
                        var show2 = MPEServices.MAS.GetTVShowBasicById(Provider, season2.ShowId);
                        return String.Format("{0} season {1}", show2.Title, season2.SeasonNumber);
                    case WebStreamMediaType.TVShow:
                        return MPEServices.MAS.GetTVShowBasicById(Provider, Id).Title;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Could not load display name of media", ex);
            }
            return "";
        }

        public virtual string GetUniqueIdentifier()
        {
            string ident = String.Format("{0}-{1}-{2}-{3}-{4}", MediaType, Provider, Id, FileType, Offset);
            return PathUtil.StripInvalidCharacters(ident);
        }

        public virtual string GetDebugName()
        {
            string pathResult = GetPath();
            string path = pathResult == null || pathResult.Length == 0 ? "(unknown)" : pathResult;
            return String.Format("mediatype={0} provider={1} id={2} filetype={3} offset={4} path={5}", MediaType, Provider, Id, FileType, Offset, path);
        }

        public override string ToString()
        {
            return GetMediaDisplayName();
        }
    }
}