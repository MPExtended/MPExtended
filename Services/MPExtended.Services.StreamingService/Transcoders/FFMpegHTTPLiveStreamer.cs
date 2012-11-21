using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.StreamingService.Code;
using System.IO;
using MPExtended.Libraries.Service;

namespace MPExtended.Services.StreamingService.Transcoders
{
    class FFMpegHTTPLiveStreamer : HTTPLiveStreamer
    {
        string indexUrl;
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
                string playlistPath = Path.Combine(TemporaryDirectory, "index.m3u8");
                if (!File.Exists(playlistPath))
                {
                    Log.Warn("HTTPLiveStreamer: Client requested index.m3u8 that doesn't exist for identifier '{0}'", Identifier);
                    return Stream.Null;
                }
                //FFMpeg outputs the local segment paths in the playlist, replace with url
                string playlist = replacePathsWithURLs(playlistPath);
                if (playlist == string.Empty)
                {
                    //the playlist file is empty until the first segment has finished being encoded,
                    //wait for next segment to begin and retry
                    string segmentPath = Path.Combine(TemporaryDirectory, "000001.ts");
                    while (!File.Exists(segmentPath))
                        System.Threading.Thread.Sleep(100);
                    playlist = replacePathsWithURLs(playlistPath);
                }
                return new MemoryStream(Encoding.UTF8.GetBytes(playlist));
            }

            return base.ProvideCustomActionFile(action, param);
        }

        string replacePathsWithURLs(string path)
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
                        //ensure that segment path has been completely written so it is correctly replaced and we don't expose system paths to web
                        if (!line.StartsWith(TemporaryDirectory))
                            break;
                        //replace local path with url
                        line = indexUrl + line.Replace(TemporaryDirectory, "").Replace("\\", "");
                    }
                    playlist += line + "\n";
                }
            }
            return playlist;
        }
    }
}
