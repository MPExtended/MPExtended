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
using System.IO;
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.Code;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class FFMpegHTTPLiveStreamer : HTTPLiveStreamer
    {
        private string indexUrl;

        public FFMpegHTTPLiveStreamer(string identifier, StreamContext context)
            : base(identifier, context)
        {
            indexUrl = WCFUtil.GetCurrentRoot() + "StreamingService/stream/CustomTranscoderData?identifier=" + identifier + "&action=segment&parameters=";            
        }

        public override Stream ProvideCustomActionFile(string action, string param)
        {
            if (action == "playlist")
            {
                WCFUtil.SetContentType("application/vnd.apple.mpegurl");
                WCFUtil.AddHeader("Access-Control-Allow-Origin","*");
                string playlistPath = Path.Combine(TemporaryDirectory, "index.m3u8");
                if (!File.Exists(playlistPath))
                {
                    StreamLog.Warn(Identifier, "HTTPLiveStreamer: Client requested index.m3u8 that doesn't exist for identifier '{0}'", Identifier);
                    return Stream.Null;
                }

                // FFMpeg outputs the local segment paths in the playlist, replace with url
                string playlist = ReplacePathsWithURLs(playlistPath);
                if (playlist == string.Empty)
                {
                    // The playlist file is empty until the first segment has finished being encoded,
                    // wait for next segment to begin and retry. 
                    string segmentPath = Path.Combine(TemporaryDirectory, "000001.ts");
                    while (!File.Exists(segmentPath))
                        System.Threading.Thread.Sleep(100);
                    playlist = ReplacePathsWithURLs(playlistPath);
                }
                return new MemoryStream(Encoding.UTF8.GetBytes(playlist));
            }

            return base.ProvideCustomActionFile(action, param);
        }

        private string ReplacePathsWithURLs(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);            
            string playlist = "";
            using (StreamReader reader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line != string.Empty && !line.StartsWith("#"))
                    {
                        // Ensure that segment path has been completely written so it is correctly replaced and we don't expose system paths to web
                        if (!line.EndsWith(".ts"))
                            break;
                        
                        //Older versions of ffmpeg write the absolute path, newer versions (beginning from 2.2) only the relative path to the playist file
                        if(line.StartsWith(TemporaryDirectory))
                        {
                            // Replace local path with url
                            line = indexUrl + line.Replace(TemporaryDirectory, "").Replace("\\", "");
                        }
                        else
                        {
                            line = indexUrl + line;
                        }
                        
                        
                    }
                    playlist += line + "\n";
                }
            }

            return playlist;
        }
    }
}
