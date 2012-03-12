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
    internal class VLCHTTPLiveStreamer
    {
        private string identifier;
        private StreamContext context;
        private string temporaryDirectory;

        public VLCHTTPLiveStreamer(string identifier, StreamContext context)
        {
            this.identifier = identifier;
            this.context = context;

            this.temporaryDirectory = Path.Combine(Path.GetTempPath(), "MPExtended", "httplivestreaming-" + identifier);
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, true);
            }
            Directory.CreateDirectory(temporaryDirectory);
        }

        public void AppendPipeline()
        {
            string indexFile = Path.Combine(temporaryDirectory, "index.m3u8");
            context.Pipeline.AddDataUnit(new HTTPLiveStreamUnit(indexFile), 10);
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

            string indexUrl = WCFUtil.GetCurrentRoot() + "StreamingService/stream/CustomTranscoderData?identifier=" + identifier + "&action=segment&parameters=######.ts";
            string liveHttpOptions = context.Profile.CodecParameters["httpLiveOptions"] + ",index=" + Path.Combine(temporaryDirectory, "index.m3u8") + ",index-url=" + indexUrl;
            string destination = Path.Combine(temporaryDirectory, "######.ts");
            return ":standard{access=livehttp{" + liveHttpOptions + "},mux=" + context.Profile.CodecParameters["muxer"] + ",dst=" + destination;
        }

        public string GetStreamURL()
        {
            return WCFUtil.GetCurrentRoot() + "StreamingService/stream/CustomTranscoderData?identifier=" + identifier + "&action=playlist&parameters=index.m3u8";
        }

        public Stream ProvideCustomActionFile(string action, string param)
        {
            switch (action)
            {
                case "segment":
                    WCFUtil.SetContentType("video/MP2T");
                    string segmentPath = Path.Combine(temporaryDirectory, Path.GetFileName(param));
                    if (!File.Exists(segmentPath))
                    {
                        Log.Warn("VLCHTTPLiveStreamer: Client requested non-existing segment file {0}", segmentPath);
                        return Stream.Null;
                    }
                    return new FileStream(segmentPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

                case "playlist":
                    WCFUtil.SetContentType("application/vnd.apple.mpegurl");
                    string playlistPath = Path.Combine(temporaryDirectory, "index.m3u8");
                    if(!File.Exists(playlistPath))
                    {
                        Log.Warn("VLCHTTPLiveStreamer: Client requested index.m3u8 that doesn't exist for identifier '{0}'", identifier);
                        return Stream.Null;
                    }

                    // Having CRLF instead of LF in the playlist is allowed by the spec, but causes problems for VLC, so strip them.
                    return StripCarriageReturn(playlistPath);

                default:
                    Log.Warn("VLCHTTPLiveStreamer: Request invalid action '{0}' with param '{1}'", action, param);
                    return Stream.Null;
            }
        }

        private Stream StripCarriageReturn(string path)
        {
            // read original file
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            string text;
            using (StreamReader reader = new StreamReader(fileStream, Encoding.UTF8))
            {
                text = reader.ReadToEnd();
            }

            // remove all carriage returns
            text = text.Replace("\r", "");

            // return
            return new MemoryStream(Encoding.UTF8.GetBytes(text));
        }
    }
}
