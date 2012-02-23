#region Copyright (C) 2011-2012 MPExtended, 2005-2011 Team MediaPortal
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
// Copyright (C) 2005-2011 Team MediaPortal, http://www.team-mediaportal.com/
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.StreamingService.MediaInfo
{
    internal static class MediaInfoWrapper
    {
        private static IMediaInfoCache persistentCache = new XmlCache();
        private static Dictionary<string, WebMediaInfo> memoryCache = new Dictionary<string, WebMediaInfo>();
        private static Dictionary<string, Tuple<DateTime, WebMediaInfo>> tvCache = new Dictionary<string, Tuple<DateTime, WebMediaInfo>>();

        public static WebMediaInfo GetMediaInfo(MediaSource source)
        {
            // we can't cache it for TV, unfortunately
            if (source.MediaType == WebStreamMediaType.TV)
            {
                // cache tv files for 60 seconds
                if (tvCache.ContainsKey(source.Id) && DateTime.Now - tvCache[source.Id].Item1 > TimeSpan.FromSeconds(60))
                {
                    // cache is valid, use it
                    return tvCache[source.Id].Item2;
                }

                // get media info and save it to the cache
                TsBuffer buf = new TsBuffer(source.Id);
                string path = buf.GetCurrentFilePath();
                Log.Debug("Using path {0} from TS buffer {1} as source for {2}", path, source.Id, source.GetDebugName());
                WebMediaInfo info = DoLoadMediaInfo(buf.GetCurrentFilePath(), true);
                tvCache[source.Id] = new Tuple<DateTime, WebMediaInfo>(DateTime.Now, info);
                return info;
            }

            // load this item from cache, if possible
            if (persistentCache.HasForSource(source))
            {
                return persistentCache.GetForSource(source);
            }

            // some checks that only matter when we are actually going to load it from disk
            if (!source.Exists)
            {
                Log.Warn("Trying to load mediainfo for {0}, which doesn't seem to exist", source.GetDebugName());
                throw new FileNotFoundException();
            }
            else if (!source.SupportsDirectAccess)
            {
                // not (yet?) supported
                Log.Warn("Loading mediainfo for non-direct access source {0} isn't supported yet", source.GetDebugName());
                throw new NotSupportedException();
            }
        
            // actually load it
            WebMediaInfo outInfo;
            using (var impersonator = source.GetImpersonator())
            {
                outInfo = DoLoadMediaInfo(source.GetPath(), false);
            }
            persistentCache.Save(source, outInfo);
            return outInfo;
        }

        private static WebMediaInfo DoLoadMediaInfo(string source, bool ignoreMemoryCache)
        {
            try
            {
                if (source == null || !File.Exists(source))
                {
                    Log.Warn("GetMediaInfo: File {0} doesn't exist or is not accessible", source);
                    return null;
                }

                // check cache
                if (!ignoreMemoryCache && memoryCache.ContainsKey(source))
                    return memoryCache[source];

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
                retinfo.Container = info.Get(StreamKind.General, 0, "Format");

                // video
                retinfo.VideoStreams = new List<WebVideoStream>();
                int videoStreams = info.Count_Get(StreamKind.Video);
                for (int i = 0; i < videoStreams; i++)
                {
                    retinfo.Duration = retinfo.Duration == 0 ? (long)StringToInt(info.Get(StreamKind.Video, i, "Duration")) : retinfo.Duration;

                    string val = info.Get(StreamKind.Video, i, "DisplayAspectRatio");
                    retinfo.VideoStreams.Add(new WebVideoStream()
                    {
                        Codec = info.Get(StreamKind.Video, i, "Codec/String"),
                        DisplayAspectRatio = StringToDecimal(info.Get(StreamKind.Video, i, "DisplayAspectRatio")),
                        DisplayAspectRatioString = info.Get(StreamKind.Video, i, "DisplayAspectRatio/String"),
                        Width = StringToInt(info.Get(StreamKind.Video, i, "Width")),
                        Height = StringToInt(info.Get(StreamKind.Video, i, "Height")),
                        ID = StringToInt(info.Get(StreamKind.Video, i, "ID")),
                        Index = i
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
                        Index = i
                    });
                }

                // subtitle codecs
                retinfo.SubtitleStreams = new List<WebSubtitleStream>();
                int subtitleStreams = info.Count_Get(StreamKind.Text);
                int scodecnr = 0;
                for (scodecnr = 0; scodecnr < subtitleStreams; scodecnr++)
                {
                    retinfo.SubtitleStreams.Add(new WebSubtitleStream()
                    {
                        Language = info.Get(StreamKind.Text, scodecnr, "Language"),
                        LanguageFull = info.Get(StreamKind.Text, scodecnr, "Language/String"),
                        ID = StringToInt(info.Get(StreamKind.Text, scodecnr, "ID")),
                        Index = scodecnr,
                        Filename = null
                    });
                }

                // get max subtitle id
                var list = retinfo.SubtitleStreams.Select(x => x.ID);
                int lastId = list.Count() == 0 ? 0 : list.Max();

                // standard name of external subtitle files
                string subfile = Path.Combine(Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source) + ".srt");
                if (File.Exists(subfile))
                {
                    retinfo.SubtitleStreams.Add(new WebSubtitleStream()
                    {
                        Language = "ext",
                        LanguageFull = "External subtitle file",
                        ID = ++lastId, // a file with so many streams seems quite unlikely to me
                        Index = ++scodecnr,
                        Filename = subfile
                    });
                }

                // language in subtitle filename
                var files = Directory.GetFiles(Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source) + ".*.srt");
                foreach (var file in files)
                {
                    string basename = Path.GetFileName(file).Substring(Path.GetFileNameWithoutExtension(source).Length);
                    string tag = basename.Substring(1, basename.Length - 5);
                    retinfo.SubtitleStreams.Add(new WebSubtitleStream()
                    {
                        Language = LookupCountryCode(tag),
                        LanguageFull = tag,
                        ID = ++lastId,
                        Index = ++scodecnr,
                        Filename = file
                    });
                }

                
                // return
                info.Close();

                if (!memoryCache.ContainsKey(source))
                {
                    memoryCache.Add(source, retinfo);
                }
                else
                {
                    memoryCache[source] = retinfo;
                }

                return retinfo;
            }
            catch (Exception ex)
            {
                Log.Error("Error parsing MediaInfo for " + source, ex);
                return null;
            }
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

        private static Decimal StringToDecimal(string data)
        {
            if (String.IsNullOrEmpty(data))
                return 0;

            Decimal result;
            if (!Decimal.TryParse(data, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return 0;

            return result;
        }

        private static string LookupCountryCode(string languageName)
        {
            var language = CultureDatabase.GetLanguage(languageName);
            return language != null ? language.TwoLetterISOLanguageName : "ext";
        }
    }
}
