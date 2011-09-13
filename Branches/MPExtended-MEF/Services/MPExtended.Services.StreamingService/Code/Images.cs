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
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ServiceModel.Web;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Services.StreamingService.Interfaces;

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

            // execute it
            string tempFile = Path.GetTempFileName(); // FIXME: maybe we need to clean this up?
            ProcessStartInfo info = new ProcessStartInfo();
            info.Arguments = String.Format("-ss {0} -vframes 1 -i \"{1}\" {2} -f image2 {3}", startPosition, source.GetPath(), ffmpegResize, tempFile);
            info.FileName = Config.GetFFMpegPath();
            info.UseShellExecute = false;
            Process proc = new Process();
            proc.StartInfo = info;
            proc.Start();
            proc.WaitForExit();

            return new FileStream(tempFile, FileMode.Open, FileAccess.Read);
        }

        public static Stream GetImage(MediaSource source)
        {
            // TODO: check filetype
            Log.Info("Streaming image of media source {0}", source);
            if (source.IsLocalFile)
            {
                return StreamImage(source.GetPath());
            }
            else
            {
                // TODO: set MIME
                return source.Retrieve();
            }
        }

        public static Stream GetImageResized(MediaSource source, int maxWidth, int maxHeight)
        {
            // create temporary directory if non-existent
            string tmpDir = Path.Combine(Path.GetTempPath(), "MPExtended", "imagecache");
            if (!Directory.Exists(tmpDir))
                Directory.CreateDirectory(tmpDir);

            // generate cache name
            string fullpath = ((int)source.MediaType).ToString() + "_" + source.Id;
            foreach (char c in Path.GetInvalidFileNameChars())
                fullpath = fullpath.Replace(c, '_');
            if (fullpath.Length > 125)
                fullpath = fullpath.Substring(fullpath.Length - 125);
            string cachedPath = Path.Combine(tmpDir, String.Format("{0}_{1}_{2}.{3}", fullpath, maxWidth, maxHeight, "jpg"));

            if (!File.Exists(cachedPath))
            {
                // TODO: check filetype
                // get source
                Log.Info("Streaming resized image of media source {0}", source);
                Image img;
                if (source.IsLocalFile)
                {
                    img = System.Drawing.Image.FromFile(source.GetPath());
                }
                else
                {
                    img = System.Drawing.Image.FromStream(source.Retrieve());
                }
                
                if (!ResizeImage(img, cachedPath, maxWidth, maxHeight))
                {
                    return null;
                }
            }

            return StreamImage(cachedPath);
        }

        private static Stream StreamImage(string path)
        {
            Dictionary<string, string> commonMimeTypes = new Dictionary<string, string>() {
                { ".jpeg", "image/jpeg" },
                { ".jpg", "image/jpeg" },
                { ".png", "image/png" },
                { ".gif", "image/gif" },
                { ".bmp", "image/x-ms-bmp" },
            };
            FileInfo info = new FileInfo(path);
            string mime = commonMimeTypes.ContainsKey(info.Extension) ? commonMimeTypes[info.Extension] : "application/octet-stream";

            FileStream fs = File.OpenRead(path);
            WebOperationContext.Current.OutgoingResponse.ContentType = mime;
            return fs;
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
