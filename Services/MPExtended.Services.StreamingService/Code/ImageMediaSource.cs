#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Shared;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal class ImageMediaSource : MediaSource
    {
        private static ChannelLogos _logos = null;
        private string path = null;
        private WebFileInfo fileInfoCache = null;

        public string Extension
        {
            get
            {
                return IsCustomized() ? Path.GetExtension(GetPath()) : GetFileInfo().Extension;
            }
        }

        public override bool Exists
        {
            get
            {
                return IsCustomized() ? File.Exists(GetPath()) : base.Exists;
            }
        }

        public override bool SupportsDirectAccess
        {
            get
            {
                return IsCustomized() ? true : base.SupportsDirectAccess;
            }
        }

        public ImageMediaSource(string path)
            : base(WebMediaType.File, null, "")
        {
            this.path = path;
        }

        public ImageMediaSource(WebMediaType type, int? provider, string id, WebFileType filetype, int offset)
            : base (type, provider, id, filetype, offset)
        {
        }

        protected override bool CheckArguments(WebMediaType mediatype, WebFileType filetype)
        {
            if ((mediatype == WebMediaType.TV || mediatype == WebMediaType.Recording) && filetype == WebFileType.Logo)
                return true;
            return base.CheckArguments(mediatype, filetype);
        }

        protected bool IsCustomized()
        {
            return path != null || ((MediaType == WebMediaType.TV || MediaType == WebMediaType.Recording) && FileType == WebFileType.Logo);
        }

        public override string GetUniqueIdentifier()
        {
            if (MediaType == WebMediaType.File && path != null)
                return String.Format("path-{0}", path.GetHashCode());
            return base.GetUniqueIdentifier();
        }

        public override WebFileInfo GetFileInfo()
        {
            if ((MediaType == WebMediaType.TV || MediaType == WebMediaType.Recording || MediaType == WebMediaType.Radio) && FileType == WebFileType.Logo)
            {
                if (fileInfoCache != null)
                    return fileInfoCache;

                if (_logos == null)
                    _logos = new ChannelLogos();

                // get display name
                int idChannel = MediaType == WebMediaType.TV || MediaType == WebMediaType.Radio ?
                    Int32.Parse(Id) :
                    Connections.TAS.GetRecordingById(Int32.Parse(Id)).ChannelId;
                var channel = Connections.TAS.GetChannelBasicById(idChannel);
                string location = _logos.FindLocation(channel.Title);
                if(location == null)
                {
                    Log.Debug("Did not find tv logo for channel {0} with id {1}", channel.Title, idChannel);
                    fileInfoCache = new WebFileInfo() { Exists = false };
                    return fileInfoCache;
                }

                // great, return it
                fileInfoCache = new WebFileInfo(location);
                return fileInfoCache;
            }

            return path != null ? new WebFileInfo(path) : base.GetFileInfo();
        }
    }
}
