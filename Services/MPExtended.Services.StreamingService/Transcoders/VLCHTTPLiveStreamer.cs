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
using System.Text;
using System.IO;
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class VLCHTTPLiveStreamer : HTTPLiveStreamer
    {
        public VLCHTTPLiveStreamer(string identifier, StreamContext context)
            : base (identifier, context)
        {
        }

        public string GetFullMuxerString()
        {
            /* In the end, we should return something like this:
             * :standard{access=livehttp{seglen=10,delsegs=false,numsegs=0,index=C:\dir\index.m3u8,index-url=http://ip/###.ts},mux=ts{use-key-frames},dst=C:\dir\###.ts}
             * 
             * As input we get this:
             * context.Profile.CodecParameters["muxer"] = ts{use-key-frames}
             * context.Profile.CodecParameters["httpLiveOptions"] = seglen=10,delsegs=false,numsegs=0
             */

            string indexUrl = WCFUtil.GetCurrentRoot() + "StreamingService/stream/CustomTranscoderData?identifier=" + Identifier + "&action=segment&parameters=######.ts";
            string liveHttpOptions = Context.Profile.CodecParameters["httpLiveOptions"] + ",index=" + Path.Combine(TemporaryDirectory, "index.m3u8") + ",index-url=" + indexUrl;
            string destination = Path.Combine(TemporaryDirectory, "######.ts");
            return ":standard{access=livehttp{" + liveHttpOptions + "},mux=" + Context.Profile.CodecParameters["muxer"] + ",dst=" + destination + "}";
        }
    }
}
