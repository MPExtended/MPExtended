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
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal class MediaInfoHelper
    {
        public static decimal DEFAULT_ASPECT_RATIO = (decimal)16 / 9; // most new material is 16:9 these days

        public static WebMediaInfo LoadMediaInfoOrSurrogate(MediaSource source)
        {
            WebMediaInfo info;
            try
            {
                info = MediaInfoWrapper.GetMediaInfo(source);
                if (info != null)
                {
                    return info;
                }
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to load MediaInfo for {0}", source.GetDebugName()), ex);
            }

            WebMediaInfo surr = new WebMediaInfo();
            surr.Duration = 0;
            surr.SubtitleStreams = new List<WebSubtitleStream>();
            surr.AudioStreams = new List<WebAudioStream>()
            {
                new WebAudioStream() 
                {
                    Channels = 2,
                    Codec = "Unknown",
                    ID = 1,
                    Index = 0,
                    Language = "und", // yes, that's valid ISO 639 (I think)
                    LanguageFull = "Unknown",
                }
            };
            surr.VideoStreams = new List<WebVideoStream>()
            {
                new WebVideoStream()
                {
                    Codec = "Unknown",
                    DisplayAspectRatio = DEFAULT_ASPECT_RATIO, 
                    DisplayAspectRatioString = "16:9",
                    ID = 2,
                    Index = 0,
                    Height = 720,
                    Width = 1280
                }
            };
            return surr;
        }
    }
}
