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
using System.Text;
using System.IO;
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Code
{
    internal abstract class HTTPLiveStreamer
    {
        protected string Identifier { get; set; }
        protected StreamContext Context { get; set; }
        public string TemporaryDirectory { get; protected set; }

        private int keepSegments;

        public HTTPLiveStreamer(string identifier, StreamContext context)
        {
            Identifier = identifier;
            Context = context;

            // create temporary directory
            TemporaryDirectory = Path.Combine(Path.GetTempPath(), "MPExtended", "httplivestreaming-" + identifier + "-" + new Random().Next(100000, 999999));
            if (Directory.Exists(TemporaryDirectory))
            {
                Directory.Delete(TemporaryDirectory, true);
            }
            Directory.CreateDirectory(TemporaryDirectory);

            // parse the <httpLiveRemoveOld> config option now for a tiny performance boost
            if (Context.Profile.TranscoderParameters.ContainsKey("httpLiveRemoveOld"))
                Int32.TryParse(Context.Profile.TranscoderParameters["httpLiveRemoveOld"], out keepSegments);
        }

        public void AppendPipeline()
        {
            string indexFile = Path.Combine(TemporaryDirectory, "index.m3u8");
            Context.Pipeline.AddDataUnit(new HTTPLiveStreamUnit(indexFile), 10);
        }

        public string GetStreamURL()
        {
            return WCFUtil.GetCurrentRoot() + "StreamingService/stream/CustomTranscoderData?identifier=" + Identifier + "&action=playlist&parameters=index.m3u8";
        }

        public virtual Stream ProvideCustomActionFile(string action, string param)
        {
            switch (action)
            {
                case "segment":
                    if (keepSegments > 0)
                        RemoveOldSegments(param);
                    WCFUtil.SetContentType("video/MP2T");
                    string segmentPath = Path.Combine(TemporaryDirectory, Path.GetFileName(param));
                    if (!File.Exists(segmentPath))
                    {
                        Log.Warn("HTTPLiveStreamer: Client requested non-existing segment file {0}", segmentPath);
                        return Stream.Null;
                    }
                    return new FileStream(segmentPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

                case "playlist":
                    WCFUtil.SetContentType("application/vnd.apple.mpegurl");
                    string playlistPath = Path.Combine(TemporaryDirectory, "index.m3u8");
                    if(!File.Exists(playlistPath))
                    {
                        Log.Warn("HTTPLiveStreamer: Client requested index.m3u8 that doesn't exist for identifier '{0}'", Identifier);
                        return Stream.Null;
                    }

                    // Having CRLF instead of LF in the playlist is allowed by the spec, but causes problems for VLC during playback, so strip them.
                    return StripCarriageReturn(playlistPath);

                default:
                    Log.Warn("HTTPLiveStreamer: Request invalid action '{0}' with param '{1}'", action, param);
                    return Stream.Null;
            }
        }

        private Stream StripCarriageReturn(string path)
        {
            // open original file, but retry if we fail with an IOException, which might happen because VLC is still writing to it
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            }
            catch (IOException)
            {
                System.Threading.Thread.Sleep(20);
                fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            }

            // read contents of original file
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

        private void RemoveOldSegments(string currentRequest)
        {
            int currentSegmentNumber;
            if (!Int32.TryParse(currentRequest.Replace(".ts", ""), out currentSegmentNumber) || currentSegmentNumber - keepSegments <= 0)
                return;

            string filename = (currentSegmentNumber - keepSegments).ToString().PadLeft(currentRequest.Length - 3, '0') + ".ts";
            string removeFileName = Path.Combine(TemporaryDirectory, filename);
            if (File.Exists(removeFileName))
            {
                Log.Trace("HTTPLiveStreamer: Remove old segment {0} during request for {1}", filename, currentRequest);
                File.Delete(removeFileName);
            }
        }
    }
}
