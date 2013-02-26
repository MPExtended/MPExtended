#region Copyright (C) 2011-2013 MPExtended, 2005-2011 Team MediaPortal
// Copyright (C) 2011-2013 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.StreamingService.MediaInfo
{
    internal static class MediaInfoWrapper
    {
        private static IMediaInfoCache persistentCache;
        private static Dictionary<string, Tuple<DateTime, WebMediaInfo>> tvCache;

        public static void LoadCache()
        {
            persistentCache = new XmlCache();
            tvCache = new Dictionary<string, Tuple<DateTime, WebMediaInfo>>();
        }

        public static WebMediaInfo GetMediaInfo(MediaSource source)
        {
            // Timeshiftings are a special case, as they can't be cached and need an additional path resolving step
            if (source.MediaType == WebMediaType.TV)
            {
                if (tvCache.ContainsKey(source.Id) && DateTime.Now - tvCache[source.Id].Item1 > TimeSpan.FromSeconds(60))
                    return tvCache[source.Id].Item2;

                TsBuffer tsBuffer = new TsBuffer(source.Id);
                Log.Debug("Using path {0} from TS buffer {1} as source for {2}", tsBuffer.GetCurrentFilePath(), source.Id, source.GetDebugName());
                WebMediaInfo info = LoadMediaInfo(tsBuffer.GetCurrentFilePath());
                tvCache[source.Id] = new Tuple<DateTime, WebMediaInfo>(DateTime.Now, info);
                return info;
            }

            if (source.NeedsImpersonation)
            {
                using (var context = new NetworkShareImpersonator())
                    return PerformHandling(source, context.RewritePath(source.GetPath()));
            }
            else
            {
                return PerformHandling(source, source.GetPath());
            }
        }

        private static WebMediaInfo PerformHandling(MediaSource source, string path)
        {
            // verify the file actually exists and is accessible over the local file system
            if (!source.Exists)
            {
                Log.Warn("Trying to load MediaInfo for {0}, which does not exist or is inaccessible", source.GetDebugName());
                return null;
            }
            else if (!source.SupportsDirectAccess)
            {
                Log.Warn("Loading MediaInfo for non-direct access source {0} isn't supported yet", source.GetDebugName());
                return null;
            }

            // if we got the file in the cache, return it if we have it and the file hasn't been changed
            var fileInfo = source.GetFileInfo();
            if (source.MediaType != WebMediaType.TV && persistentCache.HasForSource(source))
            {
                var cachedItem = persistentCache.GetForSource(source);
                if (cachedItem.Size == fileInfo.Size && cachedItem.CachedDate >= fileInfo.LastModifiedTime)
                    return cachedItem.Info;
            }

            var info = LoadMediaInfo(path);
            if (info != null)
                persistentCache.Save(source, new CachedInfoWrapper(info, fileInfo));

            return info;
        }

        private static WebMediaInfo LoadMediaInfo(string source)
        {
            try
            {
                if (source == null || !File.Exists(source))
                {
                    Log.Warn("GetMediaInfo: File {0} doesn't exist or is not accessible", source);
                    return null;
                }

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
                        Interlaced = info.Get(StreamKind.Video, i, "ScanType") == "Interlaced",
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

                try
                {
                    // language in subtitle filename
                    foreach (string file in Directory.EnumerateFiles(Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source) + ".*.srt"))
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
                }
                catch (UnauthorizedAccessException ex)
                {
                    // enumerating network shares could fail, not sure why though
                    Log.Debug(String.Format("Failed to enumerate files in {0}", Path.GetDirectoryName(source)), ex);
                }

                // return
                info.Close();
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
