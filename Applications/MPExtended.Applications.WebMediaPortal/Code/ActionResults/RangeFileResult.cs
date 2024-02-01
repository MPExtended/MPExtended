#region Copyright (C) 2013 TPeczek, 2013 MPExtended (Ms-PL)
// Copyright (C) 2013 MPExtended Developers, http://mpextended.github.io/
// Copyright (C) 2013 TPeczek, http://tpeczek.codeplex.com/
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Code.ActionResults
{
    /// <summary>
    /// Represents a base class that is used to send binary file content to the range response.
    /// </summary>
    public abstract class RangeFileResult : ActionResult
    {
        #region Fields
        private static readonly char[] _commaSplitArray = new char[] { ',' };
        private static readonly char[] _dashSplitArray = new char[] { '-' };
        private static readonly string[] _httpDateFormats = new string[] { "r", "dddd, dd-MMM-yy HH':'mm':'ss 'GMT'", "ddd MMM d HH':'mm':'ss yyyy" };
        #endregion

        #region Properties
        /// <summary>
        /// Gets the content type to use for the response.
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// Gets the file name to use for the response.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets the file modification date to use for the response.
        /// </summary>
        public DateTime FileModificationDate { get; private set; }

        private DateTime HttpModificationDate { get; set; }

        /// <summary>
        /// Gets the file length to use for the response.
        /// </summary>
        public long FileLength { get; private set; }

        private string EntityTag { get; set; }

        private long[] RangesStartIndexes { get; set; }

        private long[] RangesEndIndexes { get; set; }

        private bool RangeRequest { get; set; }

        private bool MultipartRequest { get; set; }

        private String DownloadName { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the RangeFileResult class.
        /// </summary>
        /// <param name="contentType">The content type to use for the response.</param>
        /// <param name="fileName">The file name to use for the response.</param>
        /// <param name="modificationDate">The file modification date to use for the response.</param>
        /// <param name="fileLength">The file length to use for the response.</param>
        protected RangeFileResult(string contentType, string fileName, DateTime modificationDate, long fileLength, String downloadName)
        {
            if (String.IsNullOrEmpty(contentType))
                throw new ArgumentNullException("contentType");
            Log.Debug("Download name: " + downloadName);
            ContentType = contentType;
            FileName = fileName;
            FileModificationDate = modificationDate;
            HttpModificationDate = modificationDate.ToUniversalTime();
            HttpModificationDate = new DateTime(HttpModificationDate.Year, HttpModificationDate.Month, HttpModificationDate.Day, HttpModificationDate.Hour, HttpModificationDate.Minute, HttpModificationDate.Second, DateTimeKind.Utc);
            FileLength = fileLength;
            DownloadName = downloadName;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Generates the entity tag for file
        /// </summary>
        /// <param name="context">The context within which the result is executed.</param>
        /// <returns></returns>
        protected virtual string GenerateEntityTag(ControllerContext context)
        {
            byte[] entityTagBytes = Encoding.ASCII.GetBytes(String.Format("{0}|{1}", FileName, FileModificationDate));
            return Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(entityTagBytes));
        }

        /// <summary>
        /// Writes the entire file to the response.
        /// </summary>
        /// <param name="response">The response from context within which the result is executed.</param>
        protected abstract void WriteEntireEntity(HttpResponseBase response);

        /// <summary>
        /// Writes the file range to the response.
        /// </summary>
        /// <param name="response">The response from context within which the result is executed.</param>
        /// <param name="rangeStartIndex">Range start index</param>
        /// <param name="rangeEndIndex">Range end index</param>
        protected abstract void WriteEntityRange(HttpResponseBase response, long rangeStartIndex, long rangeEndIndex);

        /// <summary>
        /// Enables processing of the result of an action method by a custom type that inherits from the ActionResult class. (Overrides ActionResult.ExecuteResult(ControllerContext).)
        /// </summary>
        /// <param name="context">The context within which the result is executed.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            EntityTag = GenerateEntityTag(context);
            GetRanges(context.HttpContext.Request);

            if (ValidateRanges(context.HttpContext.Response) && ValidateModificationDate(context.HttpContext.Request, context.HttpContext.Response) && ValidateEntityTag(context.HttpContext.Request, context.HttpContext.Response))
            {
                context.HttpContext.Response.AddHeader("Last-Modified", FileModificationDate.ToString("r"));
                context.HttpContext.Response.AddHeader("ETag", String.Format("\"{0}\"", EntityTag));
                context.HttpContext.Response.AddHeader("Accept-Ranges", "bytes");
                context.HttpContext.Response.AddHeader("Content-Disposition", String.Format("attachment; filename=\"{0}\"", DownloadName));

                if (!RangeRequest)
                {
                    context.HttpContext.Response.AddHeader("Content-Length", FileLength.ToString());
                    context.HttpContext.Response.ContentType = ContentType;
                    context.HttpContext.Response.StatusCode = 200;
                    if (!context.HttpContext.Request.HttpMethod.Equals("HEAD"))
                        WriteEntireEntity(context.HttpContext.Response);
                }
                else
                {
                    string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

                    context.HttpContext.Response.AddHeader("Content-Length", GetContentLength(boundary).ToString());
                    if (!MultipartRequest)
                    {
                        context.HttpContext.Response.AddHeader("Content-Range", String.Format("bytes {0}-{1}/{2}", RangesStartIndexes[0], RangesEndIndexes[0], FileLength));
                        context.HttpContext.Response.ContentType = ContentType;
                    }
                    else
                        context.HttpContext.Response.ContentType = String.Format("multipart/byteranges; boundary={0}", boundary);
                    context.HttpContext.Response.StatusCode = 206;
                    if (!context.HttpContext.Request.HttpMethod.Equals("HEAD"))
                    {
                        for (int i = 0; i < RangesStartIndexes.Length; i++)
                        {
                            if (MultipartRequest)
                            {
                                context.HttpContext.Response.Write(String.Format("--{0}\r\n", boundary));
                                context.HttpContext.Response.Write(String.Format("Content-Type: {0}\r\n", ContentType));
                                context.HttpContext.Response.Write(String.Format("Content-Range: bytes {0}-{1}/{2}\r\n\r\n", RangesStartIndexes[i], RangesEndIndexes[i], FileLength));
                            }

                            if (context.HttpContext.Response.IsClientConnected)
                            {
                                WriteEntityRange(context.HttpContext.Response, RangesStartIndexes[i], RangesEndIndexes[i]);
                                if (MultipartRequest)
                                {
                                    context.HttpContext.Response.Write("\r\n");
                                    context.HttpContext.Response.Flush();
                                }
                            }
                            else
                                return;
                        }
                        if (MultipartRequest)
                            context.HttpContext.Response.Write(String.Format("--{0}--", boundary));
                    }
                }
            }
        }

        private string GetHeader(HttpRequestBase request, string header, string defaultValue = "")
        {
            return String.IsNullOrEmpty(request.Headers[header]) ? defaultValue : request.Headers[header].Replace("\"", String.Empty);
        }

        private void GetRanges(HttpRequestBase request)
        {
            string rangesHeader = GetHeader(request, "Range");
            string ifRangeHeader = GetHeader(request, "If-Range", EntityTag);
            DateTime ifRangeHeaderDate;
            bool isIfRangeHeaderDate = DateTime.TryParseExact(ifRangeHeader, _httpDateFormats, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out ifRangeHeaderDate);

            if (String.IsNullOrEmpty(rangesHeader) || (!isIfRangeHeaderDate && ifRangeHeader != EntityTag) || (isIfRangeHeaderDate && HttpModificationDate > ifRangeHeaderDate))
            {
                RangesStartIndexes = new long[] { 0 };
                RangesEndIndexes = new long[] { FileLength - 1 };
                RangeRequest = false;
                MultipartRequest = false;
            }
            else
            {
                string[] ranges = rangesHeader.Replace("bytes=", String.Empty).Split(_commaSplitArray);

                RangesStartIndexes = new long[ranges.Length];
                RangesEndIndexes = new long[ranges.Length];
                RangeRequest = true;
                MultipartRequest = (ranges.Length > 1);

                for (int i = 0; i < ranges.Length; i++)
                {
                    string[] currentRange = ranges[i].Split(_dashSplitArray);

                    if (String.IsNullOrEmpty(currentRange[1]))
                        RangesEndIndexes[i] = FileLength - 1;
                    else
                        RangesEndIndexes[i] = Int64.Parse(currentRange[1]);

                    if (String.IsNullOrEmpty(currentRange[0]))
                    {
                        RangesStartIndexes[i] = FileLength - 1 - RangesEndIndexes[i];
                        RangesEndIndexes[i] = FileLength - 1;
                    }
                    else
                        RangesStartIndexes[i] = Int64.Parse(currentRange[0]);
                }
            }
        }

        private int GetContentLength(string boundary)
        {
            int contentLength = 0;

            for (int i = 0; i < RangesStartIndexes.Length; i++)
            {
                contentLength += Convert.ToInt32(RangesEndIndexes[i] - RangesStartIndexes[i]) + 1;

                if (MultipartRequest)
                    contentLength += boundary.Length + ContentType.Length + RangesStartIndexes[i].ToString().Length + RangesEndIndexes[i].ToString().Length + FileLength.ToString().Length + 49;
            }

            if (MultipartRequest)
                contentLength += boundary.Length + 4;

            return contentLength;
        }

        private bool ValidateRanges(HttpResponseBase response)
        {
            for (int i = 0; i < RangesStartIndexes.Length; i++)
            {
                if (RangesStartIndexes[i] > FileLength - 1 || RangesEndIndexes[i] > FileLength - 1 || RangesStartIndexes[i] < 0 || RangesEndIndexes[i] < 0 || RangesEndIndexes[i] < RangesStartIndexes[i])
                {
                    Log.Warn("Failed to validate ranges");
                    response.StatusCode = 400;
                    return false;
                }
            }

            return true;
        }

        private bool ValidateModificationDate(HttpRequestBase request, HttpResponseBase response)
        {
            return true;
            string modifiedSinceHeader = GetHeader(request, "If-Modified-Since");
            if (!String.IsNullOrEmpty(modifiedSinceHeader))
            {
                DateTime modifiedSinceDate;
                DateTime.TryParseExact(modifiedSinceHeader, _httpDateFormats, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out modifiedSinceDate);

                if (HttpModificationDate <= modifiedSinceDate)
                {
                    Log.Info("If-Modified-Since check failed");
                    response.StatusCode = 304;
                    return false;
                }
            }

            string unmodifiedSinceHeader = GetHeader(request, "If-Unmodified-Since", GetHeader(request, "Unless-Modified-Since"));
            if (!String.IsNullOrEmpty(unmodifiedSinceHeader))
            {
                DateTime unmodifiedSinceDate;
                bool unmodifiedSinceDateParsed = DateTime.TryParse(unmodifiedSinceHeader, out unmodifiedSinceDate);
                if (unmodifiedSinceDateParsed)
                {
                    Log.Warn("Couldn't parse: " + unmodifiedSinceHeader);
                }

                if (unmodifiedSinceDateParsed && HttpModificationDate > unmodifiedSinceDate)
                {
                    Log.Info("If-Unmodified-Since check failed (" + unmodifiedSinceDate.ToString() + " vs " + HttpModificationDate.ToString());
                    response.StatusCode = 412;
                    return false;
                }
            }

            return true;
        }

        private bool ValidateEntityTag(HttpRequestBase request, HttpResponseBase response)
        {
            string matchHeader = GetHeader(request, "If-Match");
            if (!String.IsNullOrEmpty(matchHeader) && matchHeader != "*")
            {
                string[] entitiesTags = matchHeader.Split(_commaSplitArray);
                int entitieTagIndex;
                for (entitieTagIndex = 0; entitieTagIndex < entitiesTags.Length; entitieTagIndex++)
                {
                    if (EntityTag == entitiesTags[entitieTagIndex])
                        break;
                }

                if (entitieTagIndex >= entitiesTags.Length)
                {
                    Log.Info("If-Match failed");
                    response.StatusCode = 412;
                    return false;
                }
            }

            string noneMatchHeader = GetHeader(request, "If-None-Match");
            if (!String.IsNullOrEmpty(noneMatchHeader))
            {
                if (noneMatchHeader == "*")
                {
                    response.StatusCode = 412;
                    return false;
                }

                string[] entitiesTags = noneMatchHeader.Split(_commaSplitArray);
                foreach (string entityTag in entitiesTags)
                {
                    if (EntityTag == entityTag)
                    {
                        Log.Info("If-None-Match failed");
                        response.AddHeader("ETag", String.Format("\"{0}\"", entityTag));
                        response.StatusCode = 304;
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion
    }
}