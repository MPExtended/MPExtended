#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Web;
using System.Web.Script.Serialization;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public abstract class MediaItemModel
    {
        private WebFileInfo fileInfo;
        private WebMediaInfo mediaInfo;

        [ScriptIgnore]
        protected abstract WebMediaItem Item { get; }

        [ScriptIgnore]
        public string Id { get { return Item.Id; } }

        // Most of these properties below probably violate the design guidelines by being a property: they are too slow. However,
        // if I make them a method they won't be properly serialized by the JavaScriptSerializer, so I have to do it this way.
        // (the alternative, a field, is even worse as it has to be public). 

        public WebFileInfo FileInfo
        {
            get
            {
                if (fileInfo == null)
                    fileInfo = Connections.Current.MAS.GetFileInfo(Item.PID, Item.Type, WebFileType.Content, Item.Id, 0);

                return fileInfo;
            }
        }

        public WebMediaInfo MediaInfo
        {
            get
            {
                if (mediaInfo == null)
                    mediaInfo = Connections.Current.MASStreamControl.GetMediaInfo(Item.Type, Item.PID, Item.Id, 0);

                return mediaInfo;
            }
        }

        public bool Accessible
        {
            get
            {
                return Connections.Current.MASStreamControl.GetItemSupportStatus(Item.Type, Item.PID, Item.Id, 0).Supported;
            }
        }

        public string Quality
        {
            get
            {
                return MediaInfoFormatter.GetShortQualityName(Accessible, MediaInfo);
            }
        }

        public string FullQuality
        {
            get
            {
                return MediaInfoFormatter.GetFullInfoString(Accessible, MediaInfo, FileInfo);
            }
        }
    }
}