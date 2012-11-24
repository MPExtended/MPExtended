#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Applications.WebMediaPortal.Strings;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public static class MediaInfoFormatter
    {
        private static string[] units = new string[] { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };

        public static string GetShortQualityName(WebMediaInfo info)
        {
            if (!info.VideoStreams.Any())
                return UIStrings.Unknown;

            WebVideoStream vidStream = info.VideoStreams.First();

            if (vidStream.Width >= 1920 || vidStream.Height >= 1080)
                return "1080p";
            if (vidStream.Width >= 1280 || vidStream.Height >= 720)
                return "720p";
            return "SD";
        }

        public static string GetFullInfoString(WebMediaInfo info, WebFileInfo fileInfo)
        {
            return GetFullInfoString(info, fileInfo.Size);
        }

        public static string GetFullInfoString(WebMediaInfo info, WebRecordingFileInfo fileInfo)
        {
            return GetFullInfoString(info, fileInfo.Size);
        }

        private static string GetFullInfoString(WebMediaInfo info, long fileSize)
        {
            int index = 0;
            double realSize = fileSize;
            while(realSize > 1024)
            {
                index++;
                realSize /= 1024;
            }
            return String.Format("{0}, {1:#.#} {2}", GetShortQualityName(info), realSize, units[index]);
        }
    }
}