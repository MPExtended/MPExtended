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

namespace MPExtended.Services.StreamingService.Util
{
    internal class EncodingInfo
    {
        public decimal CurrentBitrate { get; set; }
        public int CurrentTime { get; set; }
        public int EncodedFrames { get; set; }
        public int EncodingFPS { get; set; }
        public int EncodedKb { get; set; }
        public bool FinishedSuccessfully { get; set; }
        public EncodingErrors EncodingErrors { get; set; }
    }

    // this is mianly here for legacy reasons, you can ignore it
    [Flags]
    internal enum EncodingErrors
    {
        NonMonotonicallyIncreasingDts = 1,

        /// <summary>
        /// start time is not set in av_estimate_timings_from_pts
        /// leads to transcoding failing with: [buffer @ 01A4FD80] Invalid pixel format string '-1' 
        /// </summary>
        StartTimeNotSetInEstimateTimingsFromPts = 2,

        /// <summary>
        /// [mpegts @ 036973C0] h264 bitstream malformated, no startcode found, use -vbsf h264_mp4toannexb
        /// leads to av_interleaved_write_frame(): Operation not permitted 
        /// </summary>
        UseVbsfH264Mp4ToAnnexb = 4
    }
}
