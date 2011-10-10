#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Libraries.ServiceLib;

namespace MPExtended.Services.StreamingService.Code
{
    internal static class Images
    {
        public static Stream ExtractImage(MediaSource source, int startPosition, int? maxWidth, int? maxHeight)
        {
            if (!source.IsLocalFile)
            {
                Log.Warn("ExtractImage: Source " + source + " is not supported");
                return null;
            }

            // calculate size
            string ffmpegResize = "";
            if (maxWidth != null && maxHeight != null)
            {
                decimal resolution = MediaInfoWrapper.GetMediaInfo(source).VideoStreams.First().DisplayAspectRatio;
                ffmpegResize = "-s " + Resolution.Calculate(resolution, new Resolution(maxWidth.Value, maxHeight.Value)).ToString();
            }
            
            // get temporary filename
            string tempDir = Path.Combine(Path.GetTempPath(), "MPExtended", "imagecache");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            string tempFile = Path.Combine(tempDir, Guid.NewGuid().ToString() + ".jpg");

            // execute it
            ProcessStartInfo info = new ProcessStartInfo();
            info.Arguments = String.Format("-ss {0} -vframes 1 -i \"{1}\" {2} -f image2 {3}", startPosition, source.GetPath(), ffmpegResize, tempFile);
            info.FileName = Config.GetFFMpegPath();
            info.UseShellExecute = false;
            Process proc = new Process();
            proc.StartInfo = info;
            proc.Start();
            proc.WaitForExit();

            return StreamImage(tempFile);
        }

        public static Stream GetResizedImage(WebStreamMediaType mediatype, string id, int maxWidth, int maxHeight)
        {
            return GetResizedImage(mediatype, WebArtworkType.Content, id, 0, maxWidth, maxHeight);
        }

        public static Stream GetResizedImage(WebStreamMediaType mediatype, WebArtworkType artworktype, string id, int offset, int maxWidth, int maxHeight)
        {
            // create cache path
            string tmpDir = Path.Combine(Path.GetTempPath(), "MPExtended", "imagecache");
            if (!Directory.Exists(tmpDir))
                Directory.CreateDirectory(tmpDir);
            string cachedPath = Path.Combine(tmpDir, String.Format("{0}_{1}_{2}_{3}_{4}.jpg", mediatype, artworktype, id, maxWidth, maxHeight));

            // check for existence on disk
            if (!File.Exists(cachedPath))
            {
                Image orig = Image.FromStream(MPEServices.NetPipeMediaAccessService.RetrieveFile((WebMediaType)mediatype, (WebFileType)artworktype, id, offset));
                if (!ResizeImage(orig, cachedPath, maxWidth, maxHeight))
                {
                    return null;
                }
            }

            return StreamImage(cachedPath);
        }

        public static Stream GetImage(WebStreamMediaType mediatype, string id)
        {
            return GetImage(mediatype, WebArtworkType.Content, id, 0);
        }

        public static Stream GetImage(WebStreamMediaType mediatype, WebArtworkType artworktype, string id, int offset)
        {
            // validate arguments
            var pathlist = MPEServices.NetPipeMediaAccessService.GetPathList((WebMediaType)mediatype, (WebFileType)artworktype, id);
            if (pathlist == null || pathlist.Count <= offset)
            {
                Log.Info("Requested unavailable artwork mediatype={0} artworktype={1} id={2} offset={3}", mediatype, artworktype, id, offset);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return null;
            }
            string path = pathlist.ElementAt(offset);

            Stream data = null;
            WebFileInfo info = MPEServices.NetPipeMediaAccessService.GetFileInfo((WebMediaType)mediatype, (WebFileType)artworktype, id, offset);
            if (!info.Exists)
            {
                Log.Info("Requested unavailable artwork mediatype={0} artworktype={1} id={2} offset={3}", mediatype, artworktype, id, offset);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return null;
            }
            else if (!info.IsLocalFile)
            {
                data = MPEServices.NetPipeMediaAccessService.RetrieveFile((WebMediaType)mediatype, (WebFileType)artworktype, id, offset);
            }

            return StreamImage(path, data);
        }

        private static Stream StreamImage(string path, Stream data = null)
        {
            Dictionary<string, string> commonMimeTypes = new Dictionary<string, string>() {
                { ".jpeg", "image/jpeg" },
                { ".jpg", "image/jpeg" },
                { ".png", "image/png" },
                { ".gif", "image/gif" },
                { ".bmp", "image/x-ms-bmp" },
            };
            string extension = Path.GetExtension(path);
            string mime = commonMimeTypes.ContainsKey(extension) ? commonMimeTypes[extension] : "application/octet-stream";
            WebOperationContext.Current.OutgoingResponse.ContentType = mime;

            if (data == null)
            {
                FileStream fs = File.OpenRead(path);
                return fs;
            }
            else
            {
                return data;
            }
        }

        private static bool ResizeImage(Image origImage, string newFile, int maxWidth, int maxHeight)
        {
            try
            {
                int sourceWidth = origImage.Width;
                int sourceHeight = origImage.Height;

                float nPercent = 0;
                float nPercentW = 0;
                float nPercentH = 0;

                nPercentW = ((float)maxWidth / (float)sourceWidth);
                nPercentH = ((float)maxHeight / (float)sourceHeight);

                if (nPercentH < nPercentW)
                    nPercent = nPercentH;
                else
                    nPercent = nPercentW;

                int destWidth = (int)(sourceWidth * nPercent);
                int destHeight = (int)(sourceHeight * nPercent);

                Bitmap newImage = new Bitmap(destWidth, destHeight);

                Graphics g = Graphics.FromImage((Image)newImage);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.DrawImage(origImage, 0, 0, destWidth, destHeight);
                g.Dispose();

                // Save resized picture
                newImage.Save(newFile);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn("Couldn't resize image", ex);
            }
            return false;
        }
    }
}
