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
using System.Text;
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Code
{
    internal class MediaSource
    {
        public WebStreamMediaType MediaType { get; set; }
        public string Id { get; set; } // path to tsbuffer for TV
        public int? Provider { get; set; } // for MAS
        public int Offset { get; set; }

        private WebFileInfo fileInfoCache;
        private WebFileInfo FileInfo
        {
            get
            {
                if (fileInfoCache == null)
                {
                    fileInfoCache = MPEServices.MAS.GetFileInfo(Provider, (WebMediaType)MediaType, WebFileType.Content, Id, Offset);
                }

                return fileInfoCache;
            }
        }

        public bool IsLocalFile
        {
            get
            {
                if (MediaType == WebStreamMediaType.Recording)
                {
                    return true;
                }

                if (MediaType == WebStreamMediaType.TV)
                {
                    return false;
                }

                return FileInfo.IsLocalFile;
            }
        }

        public MediaSource(WebMediaType type, int? provider, string id)
        {
            this.MediaType = (WebStreamMediaType)type;
            this.Id = id;
            this.Offset = 0;
            this.Provider = provider;
        }

        public MediaSource(WebMediaType type, int? provider, string id, int offset)
            : this(type, provider, id)
        {
            this.Offset = offset;
        }

        public MediaSource(WebStreamMediaType type, int? provider, string id)
        {
            this.MediaType = type;
            this.Id = id;
            this.Offset = 0;
            this.Provider = provider;
        }

        public MediaSource(WebStreamMediaType type, int? provider, string id, int offset)
            : this(type, provider, id)
        {
            this.Offset = offset;
        }

        public string GetPath()
        {
            if (MediaType == WebStreamMediaType.Recording)
            {
                return MPEServices.TAS.GetRecordings().Where(r => r.Id == Int32.Parse(Id)).Select(r => r.FileName).FirstOrDefault();
            }

            if (MediaType == WebStreamMediaType.TV)
            {
                return Id;
            }

            return FileInfo.Path;
            //return MPEServices.MAS.GetMediaItem(Provider, (WebMediaType)MediaType, Id).Path[Offset];
        }

        public Stream Retrieve()
        {
            if (IsLocalFile)
            {
                return new FileStream(GetPath(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }

            if (MediaType == WebStreamMediaType.TV)
            {
                return new TsBuffer(Id);
            }

            return MPEServices.MAS.RetrieveFile(Provider, (WebMediaType)MediaType, WebFileType.Content, Id, Offset);
        }

        public IProcessingUnit GetInputReaderUnit()
        {
            if (IsLocalFile || MediaType == WebStreamMediaType.TV)
            {
                return new InputUnit(GetPath());
            }

            return new InjectStreamUnit(Retrieve());
        }

        public string GetDisplayName()
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
                        return MPEServices.TAS.GetChannelBasicById(Int32.Parse(Id)).DisplayName;
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

        public string GetUniqueIdentifier()
        {
            return String.Format("{0}-{1}-{2}", MediaType, Provider, Id);
        }

        public string GetDebugName()
        {
            return String.Format("mediatype={0} provider={1} id={2} offset={3}", MediaType, Provider, Id, Offset);
        }

        public override string ToString()
        {
            return GetDisplayName();
        }
    }
}
