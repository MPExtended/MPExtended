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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Shared;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal class ImageMediaSource : MediaSource
    {
        private static ChannelLogos _logos = null;
        private string path = null;

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

        public override WebFileInfo GetFileInfo()
        {
            if ((MediaType == WebMediaType.TV || MediaType == WebMediaType.Recording) && FileType == WebFileType.Logo)
            {
                if (_logos == null)
                    _logos = new ChannelLogos();

                // get display name
                int idChannel = MediaType == WebMediaType.TV ?
                    Int32.Parse(Id) :
                    Connections.TAS.GetRecordingById(Int32.Parse(Id)).ChannelId;
                var channel = Connections.TAS.GetChannelBasicById(idChannel);
                string location = _logos.FindLocation(channel.Title);
                if(location == null)
                {
                    Log.Debug("Did not find tv logo for channel {0} with id {1}", channel.Title, idChannel);
                    return new WebFileInfo() { Exists = false };
                }

                // great, return it
                return new WebFileInfo(location);
            }

            return path != null ? new WebFileInfo(path) : base.GetFileInfo();
        }
    }
}
