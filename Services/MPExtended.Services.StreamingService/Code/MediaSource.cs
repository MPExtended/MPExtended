#region Copyright (C) 2011-2013 MPExtended, 2010 MovingPictures
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2010 MovingPictures, http://code.google.com/p/moving-pictures/
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
using System.Security.Cryptography;
using System.Text;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Extensions;
using MPExtended.Libraries.Service.Network;
using MPExtended.Libraries.Service.Strings;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
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
                    return Exists && Connections.IsTASLocal && Configuration.Services.NetworkImpersonation.ReadInStreamingService && GetFileInfo().IsLocalFile;
                }
                else
                {
                    return Exists && Connections.IsMASLocal && Configuration.Services.NetworkImpersonation.ReadInStreamingService && GetFileInfo().IsLocalFile;
                }
            }
        }

        public virtual bool NeedsImpersonation
        {
            get
            {
                bool impersonationEnabled = Configuration.Services.NetworkImpersonation.IsEnabled();
                return SupportsDirectAccess && GetFileInfo().OnNetworkDrive && !FileUtil.IsAccessible(GetPath()) && impersonationEnabled;
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

        /// <returns>Error if there is a problem with the MediaSource, null otherwise</returns>
        public string CheckAvailability()
        {
            string path = GetPath();
            if (path == null || path.Length == 0)
                return UI.StreamErrorUnknownPath;

            // some checks based upon the file info. apparantly people have broken files in their collection.
            var fileinfo = GetFileInfo();
            if (!fileinfo.Exists)
            {
                // add a special warning message for files that are on a network drive, as this often causes problems
                Uri uri = new Uri(path);
                if (uri.IsUnc && !NetworkInformation.IsLocalAddress(uri.Host))
                    return UI.StreamErrorInaccessibleNetworkShare;

                return UI.StreamErrorFileDoesntExists;
            }

            if (MediaType != WebMediaType.TV && fileinfo.Size == 0)
                return UI.StreamErrorFileIsEmpty;

            // we don't support some things yet
            if (path.EndsWith(".IFO"))
                return UI.StreamErrorDVDsNotSupported;

            // while corrupt files may work, it's probably a better idea to warn early. check for a valid file using mediainfo
            if (MediaInfo.MediaInfoWrapper.GetMediaInfo(this) == null)
                return UI.StreamErrorFileIsCorrupt;

            return null;
        }

        public virtual WebFileInfo GetFileInfo()
        {
            if (fileInfoCache != null)
                return fileInfoCache;

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

        public INetworkContext CreateNetworkContext()
        {
            return NetworkContextFactory.Create(NeedsImpersonation);
        }

        public Stream Retrieve()
        {
            if (MediaType == WebMediaType.TV && FileType == WebFileType.Content)
            {
                return new TsBuffer(Id);
            }
            else if (SupportsDirectAccess)
            {
                using (var context = CreateNetworkContext())
                    return new FileStream(context.RewritePath(GetPath()), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
                    case WebMediaType.MobileVideo:
                        return Connections.MAS.GetMobileVideoBasicById(Provider, Id).Title;
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

        /// <summary>
        /// Calculates a unique hash for the contents of the file, in a smart way that avoids
        /// reading the whole file from disk. Use this method to compute hashes of large files.
        /// 
        /// Taken from MovingPictures source: Cornerstone/Extension/IO/FileInfoExtensions.cs.
        /// </summary>
        public string ComputeSmartHash()
        {
            try
            {
                using (Stream input = Retrieve())
                {
                    long streamsize = input.Length;
                    ulong lhash = (ulong)streamsize;

                    long i = 0;
                    byte[] buffer = new byte[sizeof(long)];
                    input.Position = 0;
                    while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
                    {
                        i++;
                        unchecked { lhash += BitConverter.ToUInt64(buffer, 0); }
                    }

                    input.Position = Math.Max(0, streamsize - 65536);
                    i = 0;
                    while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
                    {
                        i++;
                        unchecked { lhash += BitConverter.ToUInt64(buffer, 0); }
                    }

                    return BitConverter.GetBytes(lhash).ToHexString();
                }
            }
            catch (Exception e)
            {
                Log.Warn("Error computing smart hash", e);
                return null;
            }
        }

        public string ComputeFullHash()
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = Retrieve())
                {
                    return md5.ComputeHash(stream).ToHexString();
                }
            }
        }
    }
}