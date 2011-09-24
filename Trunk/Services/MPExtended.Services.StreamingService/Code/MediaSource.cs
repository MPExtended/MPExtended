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
using System.Linq;
using System.Text;
using System.IO;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Code
{
    internal class MediaSource
    {
        public WebStreamMediaType MediaType { get; set; }
        public string Id { get; set; }
        public int Offset { get; set; }

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

                return MPEServices.NetPipeMediaAccessService.IsLocalFile((WebMediaType)MediaType, WebFileType.Content, Id, Offset);
            }
        }

        public MediaSource(WebMediaType type, string id)
        {
            this.MediaType = (WebStreamMediaType)type;
            this.Id = id;
            this.Offset = 0;
        }

        public MediaSource(WebMediaType type, string id, int offset)
            : this(type, id)
        {
            this.Offset = offset;
        }

        public MediaSource(WebStreamMediaType type, string id)
        {
            this.MediaType = type;
            this.Id = id;
            this.Offset = 0;
        }

        public MediaSource(WebStreamMediaType type, string id, int offset)
            : this(type, id)
        {
            this.Offset = offset;
        }

        public string GetPath()
        {
            if (MediaType == WebStreamMediaType.Recording)
            {
                return MPEServices.NetPipeTVAccessService.GetRecordings().Where(r => r.Id == Int32.Parse(Id)).Select(r => r.FileName).FirstOrDefault();
            }

            if (MediaType == WebStreamMediaType.TV)
            {
                return new TsBuffer(Id).GetCurrentFilePath();
            }

            return MPEServices.NetPipeMediaAccessService.GetMediaItem((WebMediaType)MediaType, Id).Path[Offset];
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

            return MPEServices.NetPipeMediaAccessService.RetrieveFile((WebMediaType)MediaType, WebFileType.Content, Id, Offset);
        }

        public IProcessingUnit GetInputReaderUnit()
        {
            if (IsLocalFile || MediaType == WebStreamMediaType.TV)
            {
                return new InputUnit(GetPath());
            }

            return new InjectStreamUnit(Retrieve());
        }

        public override string ToString()
        {
            return "(" + Enum.GetName(typeof(WebStreamMediaType), MediaType) + ", " + Id + ")";
        }
    }
}
