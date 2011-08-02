using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Util;

namespace MPExtended.Services.StreamingService.MediaInfo
{
    internal static class MediaInfoWrapper
    {
        private static Dictionary<string, WebMediaInfo> Cache = new Dictionary<string, WebMediaInfo>();

        public static WebMediaInfo GetMediaInfo(TsBuffer buffer)
        {
            string path = buffer.GetCurrentFilePath();
            return GetMediaInfo(path, true);
        }

        public static WebMediaInfo GetMediaInfo(string source)
        {
            return GetMediaInfo(source, false);
        }

        public static WebMediaInfo GetMediaInfo(string source, bool ignoreCache) 
        {
            if (source == null || !File.Exists(source))
            {
                Log.Warn("GetMediaInfo: File " + source + " doesn't exist or is not accessible");
                return null;
            }

            // check cache
            if (!ignoreCache && Cache.ContainsKey(source))
                return Cache[source];

            /* Loosely based upon MediaInfoWrapper.cs (mediaportal/Core/Player) from MediaPortal trunk r27491 as of 15 June 2011
             * 
             * Using the whole wrapper from MediaPortal is quite much porting work as it's cluttered with calls to other MP code. Referencing
             * MediaPortal.Core.dll also isn't an option as that pulls in a big tree of dependencies. 
             * 
             * TODO: Needs error handling
             * TODO: No support for DVDs yet
             * TODO: Aspect ratio doesn't work properly yet
             */
            MediaInfo info = new MediaInfo();
            info.Option("ParseSpeed", "0.3");
            info.Open(source);
            WebMediaInfo retinfo = new WebMediaInfo();
            int index = 0;

            // video
            retinfo.VideoStreams = new List<WebVideoStream>();
            int videoStreams = info.Count_Get(StreamKind.Video);
            for (int i = 0; i < videoStreams; i++)
            {
                retinfo.Duration = retinfo.Duration == 0 ? (long)StringToInt(info.Get(StreamKind.Video, i, "Duration")) : retinfo.Duration;
                retinfo.VideoStreams.Add(new WebVideoStream()
                {
                    Codec = info.Get(StreamKind.Video, i, "Codec/String"),
                    DisplayAspectRatio = Decimal.Parse(info.Get(StreamKind.Video, i, "DisplayAspectRatio"), System.Globalization.CultureInfo.InvariantCulture),
                    DisplayAspectRatioString = info.Get(StreamKind.Video, i, "DisplayAspectRatio/String"),
                    Width = StringToInt(info.Get(StreamKind.Video, i, "Width")),
                    Height = StringToInt(info.Get(StreamKind.Video, i, "Height")),
                    ID = StringToInt(info.Get(StreamKind.Video, i, "ID")),
                    Index = index++
                });
            }

            // audio codecs
            retinfo.AudioStreams = new List<WebAudioStream>();
            int audioStreams = info.Count_Get(StreamKind.Audio);
            for (int i = 0; i < audioStreams; i++)
            {
                retinfo.Duration = retinfo.Duration == 0 ? (long)StringToInt(info.Get(StreamKind.Audio, i, "Duration")) : retinfo.Duration;
                retinfo.AudioStreams.Add(new WebAudioStream()
                {
                    Channels = StringToInt(info.Get(StreamKind.Audio, i, "Channels")),
                    Codec = info.Get(StreamKind.Audio, i, "Codec/String"),
                    ID = StringToInt(info.Get(StreamKind.Audio, i, "ID")),
                    Language = info.Get(StreamKind.Audio, i, "Language"),
                    LanguageFull = info.Get(StreamKind.Audio, i, "Language/String"),
                    Title = info.Get(StreamKind.Audio, i, "Title"),
                    Index = index++
                });
            }

            // subtitle codecs
            retinfo.SubtitleStreams = new List<WebSubtitleStream>();
            int subtitleStreams = info.Count_Get(StreamKind.Text);
            for (int i = 0; i < subtitleStreams; i++)
            {
                retinfo.SubtitleStreams.Add(new WebSubtitleStream()
                {
                    Language = info.Get(StreamKind.Text, i, "Language"),
                    LanguageFull = info.Get(StreamKind.Text, i, "Language/String"),
                    Index = index++
                });
            }

            // return
            info.Close();
            Cache[source] = retinfo;
            return retinfo;
        }

        private static int StringToInt(string data)
        {
            if (String.IsNullOrEmpty(data))
                return 0;

            int result;
            if (!Int32.TryParse(data, out result))
                return 0;

            return result;
        }
    }
}
