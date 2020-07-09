#region Copyright (C) 2012-2013 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class Direct : ITranscoder, IRetrieveHookTranscoder
    {
        public string Identifier { get; set; }
        public StreamContext Context { get; set; }

        private static Lazy<Dictionary<string, string>> mimeDatabase = new Lazy<Dictionary<string, string>>(CreateMimeDatabase, true);
        private static readonly string[] usedTypes = new[] { "video", "audio", "image" };
        private static readonly Regex bytes = new Regex(@"^bytes=(\d+)(?:-(\d+)?)?$", RegexOptions.Compiled);

        /// <summary>
        /// Get from MIME-Database in Registry
        /// </summary>
        /// <param name="extention"></param>
        /// <returns></returns>
        public static bool TryGetMime(string extention, out string mime)
        {
            return mimeDatabase.Value.TryGetValue(extention, out mime);
        }

        public string GetStreamURL()
        {
            return WCFUtil.GetCurrentRoot() + "StreamingService/stream/RetrieveStream?identifier=" + Identifier;
        }

        public void BuildPipeline()
        {
            // we ignore our arguments :)
            Context.TranscodingInfo.Supported = false;
            Context.Pipeline.AddDataUnit(Context.GetInputReaderUnit(), 1);
            return;
        }

        public void RetrieveStreamCalled()
        {
            string mime;
            if (!TryGetMime(Path.GetExtension(Context.Source.GetFileInfo().Name), out mime))
            {
                mime = "application/octet-stream";
            }
            WCFUtil.SetContentType(mime);
            WCFUtil.AddHeader("Last-Modified", Context.Source.GetFileInfo().LastModifiedTime.ToString("r", System.Globalization.CultureInfo.InvariantCulture));
            StreamLog.Info(Identifier, "*RetrieveStream Source={0}, MediaType={1}, ContenType={2}", Context.Source.GetFileInfo().Name, Context.Source.MediaType, mime);

            // Don't return a length when we're streaming TV, as we don't know the full length of the stream, since we are
            // reading from a timeshifting buffer. See dicussion in bug #394 and #319. 
            if (Context.Source.MediaType != WebMediaType.TV)
            {
                var stream = Context.Pipeline.GetFinalStream();
                string range = WebOperationContext.Current.IncomingRequest.Headers["Range"];
                if (range != null)
                {
                    //Test the Stream is Closed, Closed Why? WCF Close the Stream on EOF see DirectStream.cs
                    StreamLog.Info(Identifier, "Range requested: '{0}' - can {1}Seek", range, !stream.CanSeek ? "not " : string.Empty);
                    if (!stream.CanSeek)
                    {
                        WCFUtil.AddHeader("Accept-Ranges", "none");
                        WCFUtil.SetContentLength(Context.Source.GetFileInfo().Size);
                        return;
                    }

                    long totalLength = Context.Source.GetFileInfo().Size;
                    long sendCount = totalLength;
                    long start = 0L;
                    long end = totalLength - 1;

                    if (!TryGetRange(range, totalLength, ref start, ref end))
                    {
                        WCFUtil.SetResponseCode(HttpStatusCode.RequestedRangeNotSatisfiable);
                        WCFUtil.AddHeader("Connection", "close");
                        StreamLog.Debug(Identifier, "RangeNotSatisfiable: '{0}'", range);
                        return;
                    }

                    var contentrange = string.Format("bytes {0}-{1}/{2}", start, end, totalLength);
                    sendCount = end - start + 1;

                    StreamLog.Debug(Identifier, "Range set: {0}", contentrange);
                    StreamLog.Info(Identifier, "SeekTo: {0}", start);
                    stream.Position = start;

                    WCFUtil.AddHeader("Accept-Ranges", "bytes");
                    WCFUtil.AddHeader("Content-Range", contentrange);
                    WCFUtil.SetContentLength(sendCount);
                    WCFUtil.SetResponseCode(HttpStatusCode.PartialContent);
                }
            }
        }

        private bool TryGetRange(string range, long totalLength, ref long start, ref long end)
        {
            var m = bytes.Match(range);
            if (m.Success)
            {
                if (long.TryParse(m.Groups[1].Value, out start) && start >= 0)
                {
                    if (m.Groups.Count != 3 || !long.TryParse(m.Groups[2].Value, out end) || end <= start || end >= totalLength)
                    {
                        end = totalLength - 1;
                    }
                    return !(start >= totalLength);
                }
            }
            return false;
        }

        private static Dictionary<string, string> CreateMimeDatabase()
        {
            var mimeDatabase = new Dictionary<string, string>();

            using (RegistryKey database = Registry.ClassesRoot.OpenSubKey("MIME\\Database\\Content Type"))
            {
                WCFUtil.SetContentType(mime.ToString());
                if (database != null)
                {
                    foreach (string mimeType in database.GetSubKeyNames().Where(mt => usedTypes.Count(ut => mt.StartsWith(ut)) > 0))
                    {
                        using (RegistryKey mimeKey = database.OpenSubKey(mimeType))
                        {
                            var extension = mimeKey.GetValue("Extension") as string;
                            if (!string.IsNullOrWhiteSpace(extension))
                                mimeDatabase[extension] = mimeType;
                        }
                    }
                }
            }
            mimeDatabase.Add(".tsbuffer", "video/mp2ts");
            if (!mimeDatabase.ContainsKey(".ts"))
            {
                mimeDatabase.Add(".ts", "video/mp2ts");
            }

            return mimeDatabase;
        }
    }
}
