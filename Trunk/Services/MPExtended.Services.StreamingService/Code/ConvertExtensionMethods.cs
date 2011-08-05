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
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Util;

namespace MPExtended.Services.StreamingService.Code
{
    internal static class EncodingInfoExtensionMethods
    {
        public static WebTranscodingInfo ToWebTranscodingInfo(this EncodingInfo info)
        {
            return new WebTranscodingInfo() {
                CurrentBitrate = info.CurrentBitrate,
                CurrentTime = info.CurrentTime,
                EncodedFrames = info.EncodedFrames,
                EncodingFPS = info.EncodingFPS,
            };
        }
    }

    internal static class ResolutionExtensionMethods
    {
        public static WebResolution ToWebResolution(this Resolution res)
        {
            return new WebResolution()
            {
                Width = res.Width,
                Height = res.Height
            };
        }
    }
}
