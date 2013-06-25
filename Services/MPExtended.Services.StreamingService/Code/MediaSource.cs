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
using System.Text;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal class MediaSource
    {
        private WebFileInfo fileInfoCache;

        public WebMediaType MediaType { get; private set; }
        public WebFileType FileType { get; private set; }
        public string Id { get; private set; } // path to tsbuffer for TV
        public int? Provider { get; private set; } // for MAS
        public int Offset { get; private set; }

        public virtual bool Exists
        {
            get
            {
                return MediaType == WebMediaType.TV && FileType == WebFileType.Content ? File.Exists(Id) : GetFileInfo().Exists;
            }
        }

        public virtual bool SupportsDirectAccess
        {
            get
            {
                if (MediaType == WebMediaType.TV || MediaType == WebMediaType.Recording)
                {
                    return Exists && Connections.IsTASLocal && GetFileInfo().IsLocalFile;
                }
                else
                {
                    return Exists && Connections.IsMASLocal && GetFileInfo().IsLocalFile;
                }
            }
        }

        public virtual bool NeedsInputReaderUnit
        {
            get
            {
                return (MediaType == WebMediaType.TV && FileType == WebFileType.Content) || !SupportsDirectAccess;
            }
        }

        public virtual bool NeedsImpersonation
        {
            get
            {
                bool impersonationEnabled = Configuration.Services.NetworkImpersonation.IsEnabled();
                return impersonationEnabled && SupportsDirectAccess && GetFileInfo().OnNetworkDrive && !FileUtil.IsAccessible(GetPath());
            }
        }

        public MediaSource(WebMediaType type, int? provider, string id, WebFileType filetype, int offset)
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

        public MediaSource(WebMediaType type, int? provider, string id, WebFileType filetype)
            : this(type, provider, id, filetype, 0)
        {
        }

        public MediaSource(WebMediaType type, int? provider, string id, int? offset)
            : this(type, provider, id, WebFileType.Content, offset ?? 0)
        {
        }

        public MediaSource(WebMediaType type, int? provider, string id)
            : this(type, provider, id, WebFileType.Content, 0)
        {
        }

        protected virtual bool CheckArguments(WebMediaType mediatype, WebFileType filetype)
        {
            return !(
                        (mediatype == WebMediaType.TV && FileType != WebFileType.Content) ||
                        (mediatype == WebMediaType.Recording && FileType != WebFileType.Content)
                    );
        }

        public virtual WebFileInfo GetFileInfo()
        {
            if (fileInfoCache != null)
            {
                return fileInfoCache;
            }

            if (MediaType == WebMediaType.Recording && FileType == WebFileType.Content)
            {
                WebRecordingFileInfo info = Connections.TAS.GetRecordingFileInfo(Int32.Parse(Id));
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
                    Path = PathUtil.StripFileProtocolPrefix(info.Path),
                    PID = -1,
                    Size = info.Size
                };
                return fileInfoCache;
            }

            if (MediaType == WebMediaType.TV && FileType == WebFileType.Content)
            {
                fileInfoCache = new WebFileInfo(new FileInfo(Id))
                {
                    Exists = true,
                    IsLocalFile = true,
                    IsReadOnly = true,
                    OnNetworkDrive = false,
                    PID = -1,
                    // This field should not be read for the TV mediatype, since we can't know the size of timeshiftings. However, since
                    // there might slip a usage through and this has broken live TV streaming over WAN in the Direct profile for months
                    // in the past, let's assume they're infinitely large for practical purposes here.
                    // TODO: Maybe use a childclass of WebFileInfo that throws in the Size.get accessor? At least we get clear logs then.
                    Size = Int64.MaxValue
                };
                fileInfoCache.Path = PathUtil.StripFileProtocolPrefix(fileInfoCache.Path);
                return fileInfoCache;
            }

            fileInfoCache = Connections.MAS.GetFileInfo(Provider, MediaType, FileType, Id, Offset);
            return fileInfoCache;
        }

        public string GetPath()
        {
            return MediaType == WebMediaType.TV && FileType == WebFileType.Content ? PathUtil.StripFileProtocolPrefix(Id) : GetFileInfo().Path;
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
            if (MediaType == WebMediaType.TV && FileType == WebFileType.Content)
            {
                return new TsBuffer(Id);
            }
            else if (SupportsDirectAccess)
            {
                using (NetworkShareImpersonator impersonator = new NetworkShareImpersonator(NeedsImpersonation))
                {
                    return new FileStream(GetPath(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
            }
            else if (MediaType == WebMediaType.Recording)
            {
                return Connections.TAS.ReadRecordingFile(Int32.Parse(Id));
            }
            else
            {
                return Connections.MAS.RetrieveFile(Provider, MediaType, FileType, Id, Offset);
            }
        }

        public virtual string GetMediaDisplayName()
        {
            try
            {
                switch (MediaType)
                {
                    case WebMediaType.File:
                        return Connections.MAS.GetFileSystemFileBasicById(Provider, Id).Title;
                    case WebMediaType.Movie:
                        return Connections.MAS.GetMovieBasicById(Provider, Id).Title;
                    case WebMediaType.MusicAlbum:
                        return Connections.MAS.GetMusicAlbumBasicById(Provider, Id).Title;
                    case WebMediaType.MusicTrack:
                        return Connections.MAS.GetMusicTrackBasicById(Provider, Id).Title;
                    case WebMediaType.Picture:
                        return Connections.MAS.GetPictureBasicById(Provider, Id).Title;
                    case WebMediaType.Recording:
                        return Connections.TAS.GetRecordingById(Int32.Parse(Id)).Title;
                    case WebMediaType.TV:
                        int channelId;
                        if (!Int32.TryParse(Id, out channelId))
                        {
                            var cards = Connections.TAS.GetActiveCards().ToList();
                            if (!cards.Any(x => x.TimeShiftFileName == Id))
                            {
                                Log.Info("Cannot find card for timeshift buffer {0} (but did find {1}), what's happening?!", 
                                    Id, String.Join(", ", cards.Select(x => x.TimeShiftFileName)));
                                return String.Empty;
                            }
                            channelId = cards.First(x => x.TimeShiftFileName == Id).ChannelId;
                        }
                        return Connections.TAS.GetChannelBasicById(channelId).Title;
                    case WebMediaType.TVEpisode:
                        var ep = Connections.MAS.GetTVEpisodeBasicById(Provider, Id);
                        var season = Connections.MAS.GetTVSeasonBasicById(Provider, ep.SeasonId);
                        var show = Connections.MAS.GetTVShowBasicById(Provider, ep.ShowId);
                        return String.Format("{0} ({1} {2}x{3})", ep.Title, show.Title, season.SeasonNumber, ep.EpisodeNumber);
                    case WebMediaType.TVSeason:
                        var season2 = Connections.MAS.GetTVSeasonDetailedById(Provider, Id);
                        var show2 = Connections.MAS.GetTVShowBasicById(Provider, season2.ShowId);
                        return String.Format("{0} season {1}", show2.Title, season2.SeasonNumber);
                    case WebMediaType.TVShow:
                        return Connections.MAS.GetTVShowBasicById(Provider, Id).Title;
                }
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Could not load display name of media {0}", GetDebugName()), ex);
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