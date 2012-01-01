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
using System.Linq;
using System.Text;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal class MediaInfoHelper
    {
        public static WebMediaInfo LoadMediaInfoOrSurrogate(MediaSource source)
        {
            WebMediaInfo info = MediaInfoWrapper.GetMediaInfo(source);
            if (info != null)
            {
                return info;
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
                    DisplayAspectRatio = 16 / 9, // this class is primarily used for TV data and that's mostly 16:9 these days afaik
                    DisplayAspectRatioString = "16:9",
                    ID = 2,
                    Index = 0,
                    Height = 1280, // gives this any problems?
                    Width = 720
                }
            };
            return surr;
        }
    }
}
