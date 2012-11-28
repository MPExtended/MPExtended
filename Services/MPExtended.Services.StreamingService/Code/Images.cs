#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Services.StreamingService.Code
{
    internal static class Images
    {
        private static ImageCache cache = new ImageCache();

        public static Stream ExtractImage(MediaSource source, long position, string format)
        {
            return Images.ExtractImage(source, position, null, null, null, format);
        }

        public static Stream ExtractImage(MediaSource source, long position, int? maxWidth, int? maxHeight, string borders, string format)
        {
            if (!source.Exists)
            {
                Log.Warn("ExtractImage: Source {0} (resolved to path {1}) doesn't exists", source.GetDebugName(), source.GetPath());
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return Stream.Null;
            }

            if (!source.SupportsDirectAccess)
            {
                Log.Warn("ExtractImage: Extracting images from remote sources isn't supported yet", source.MediaType, source.Id);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotImplemented);
                return Stream.Null;
            }

            // get temporary filename
            string filename = String.Format("extract_{0}_{1}.jpg", source.GetUniqueIdentifier(), position);
            string tempFile = cache.GetPath(filename);

            // maybe it exists in cache, return that then
            if (cache.Contains(filename))
            {
                return StreamPostprocessedImage(new ImageMediaSource(tempFile), maxWidth, maxHeight, borders, format);
            }

            // execute it
            ProcessStartInfo info = new ProcessStartInfo();
            using (NetworkShareImpersonator impersonator = new NetworkShareImpersonator(source.NeedsImpersonation))
            {
                info.Arguments = String.Format("-ss {0} -i \"{1}\" -vframes 1 -f image2 {2}", position, source.GetPath(), tempFile);
                info.FileName = Configuration.StreamingProfiles.FFMpegPath;
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                Process proc = new Process();
                proc.StartInfo = info;
                proc.Start();
                proc.WaitForExit();
            }

            // log when failed
            if (!File.Exists(tempFile))
            {
                Log.Warn("Failed to extract image to temporary file {0} with command {1}", tempFile, info.Arguments);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.InternalServerError);
                return Stream.Null;
            }

            return StreamPostprocessedImage(new ImageMediaSource(tempFile), maxWidth, maxHeight, borders, format);
        }

        public static Stream GetImage(ImageMediaSource src, string format)
        {
            return StreamPostprocessedImage(src, null, null, null, format);
        }

        public static Stream GetResizedImage(ImageMediaSource src, int? maxWidth, int? maxHeight, string borders, string format)
        {
            return StreamPostprocessedImage(src, maxWidth, maxHeight, borders, format);
        }

        private static Stream StreamPostprocessedImage(ImageMediaSource src, int? maxWidth, int? maxHeight, string borders, string format)
        {
            if (!src.Exists)
            {
                Log.Info("Tried to stream image from non-existing source {0}", src.GetDebugName());
                WCFUtil.SetResponseCode(HttpStatusCode.NotFound);
                return Stream.Null;
            }

            if (borders != null && !maxWidth.HasValue && !maxHeight.HasValue)
            {
                Log.Error("ResizeImage() called with a broders value but width or height is null");
                WCFUtil.SetResponseCode(HttpStatusCode.BadRequest);
                return Stream.Null;
            }

            if (format == null)
                format = src.Extension.Substring(1);

            // return from cache if possible
            string filename = String.Format("stream_{0}_{1}_{2}_{3}.{4}", src.GetUniqueIdentifier(), maxWidth, maxHeight, borders, format);
            if (cache.Contains(filename))
            {
                WCFUtil.AddHeader(HttpResponseHeader.CacheControl, "public, max-age=5184000, s-maxage=5184000"); // not really sure why 2 months exactly
                WCFUtil.SetContentType(Path.GetExtension(filename));
                return new FileStream(cache.GetPath(filename), FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            try
            {
                bool hasToResize = maxWidth.HasValue || maxHeight.HasValue;
                bool hasToRecode = format != src.Extension.Substring(1);

                if (!hasToResize && !hasToRecode)
                {
                    WCFUtil.AddHeader(HttpResponseHeader.CacheControl, "public, max-age=5184000, s-maxage=5184000");
                    WCFUtil.SetContentType(GetMime(src.Extension));
                    return src.Retrieve();
                }

                // save image to cache
                var image = hasToResize ? ResizeImage(src, maxWidth, maxHeight, borders) : Image.FromStream(src.Retrieve());
                string path = cache.GetPath(String.Format("stream_{0}_{1}_{2}_{3}.{4}", src.GetUniqueIdentifier(), maxWidth, maxHeight, borders, format));
                SaveImageToFile(image, path, format);
                image.Dispose();

                // return image to client
                WCFUtil.AddHeader(HttpResponseHeader.CacheControl, "public, max-age=5184000, s-maxage=5184000");
                WCFUtil.SetContentType(GetCodecInfo(format).MimeType);
                return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to post-process and stream image {0}", src.GetDebugName()), ex);
                return Stream.Null;
            }
        }

        private static string GetMime(string extension)
        {
            string lowerExtension = extension.Substring(1).ToLower();
            Dictionary<string, string> commonMimeTypes = new Dictionary<string, string>() 
            {
                { "jpeg", "image/jpeg" },
                { "jpg", "image/jpeg" },
                { "png", "image/png" },
                { "gif", "image/gif" },
                { "bmp", "image/x-ms-bmp" },
            };
            return commonMimeTypes.ContainsKey(lowerExtension) ? commonMimeTypes[lowerExtension] : "application/octet-stream";
        }

        private static Image ResizeImage(ImageMediaSource src, int? maxWidth, int? maxHeight, string borders)
        {
            using (var origImage = Image.FromStream(src.Retrieve()))
            {
                Resolution newSize = Resolution.Calculate(origImage.Width, origImage.Height, maxWidth, maxHeight, 1);
                int bitmapWidth = !String.IsNullOrEmpty(borders) ? maxWidth.Value : newSize.Width;
                int bitmapHeight = !String.IsNullOrEmpty(borders) ? maxHeight.Value : newSize.Height;

                Bitmap newImage = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format32bppArgb);
                using (Graphics graphic = Graphics.FromImage(newImage))
                {
                    graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphic.SmoothingMode = SmoothingMode.HighQuality;
                    graphic.CompositingQuality = CompositingQuality.HighQuality;
                    graphic.CompositingMode = CompositingMode.SourceCopy;
                    graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    if (!String.IsNullOrEmpty(borders))
                        graphic.FillRectangle(new SolidBrush(ColorTranslator.FromHtml("#" + borders)), 0, 0, bitmapWidth, bitmapHeight);

                    int leftOffset = !String.IsNullOrEmpty(borders) ? (maxWidth.Value - newSize.Width) / 2 : 0;
                    int heightOffset = !String.IsNullOrEmpty(borders) ? (maxHeight.Value - newSize.Height) / 2 : 0;
                    graphic.DrawImage(origImage, leftOffset, heightOffset, newSize.Width, newSize.Height);
                }

                return newImage;
            }
        }

        private static ImageCodecInfo GetCodecInfo(string format)
        {
            switch (format.ToLower())
            {
                case "png":
                    return ImageCodecInfo.GetImageEncoders().First(enc => enc.FormatID == ImageFormat.Png.Guid);
                case "jpeg":
                case "jpg":
                    return ImageCodecInfo.GetImageEncoders().First(enc => enc.FormatID == ImageFormat.Jpeg.Guid);
                case "gif":
                    return ImageCodecInfo.GetImageEncoders().First(enc => enc.FormatID == ImageFormat.Gif.Guid);
                case "bmp":
                    return ImageCodecInfo.GetImageEncoders().First(enc => enc.FormatID == ImageFormat.Bmp.Guid);
                default:
                    Log.Warn("Requested invalid file format '{0}'", format);
                    throw new ArgumentException(String.Format("Invalid file format '{0}'", format));
            }
        }

        private static void SaveImageToFile(Image image, string path, string format)
        {
            switch (format.ToLower())
            {
                case "png":
                    image.Save(path, ImageFormat.Png);
                    break;

                case "gif":
                    image.Save(path, ImageFormat.Gif);
                    break;

                case "bmp":
                    image.Save(path, ImageFormat.Bmp);
                    break;

                case "jpeg":
                case "jpg":
                    var jpegInfo = GetCodecInfo(format);
                    var jpegParameters = new EncoderParameters(1);
                    jpegParameters.Param[0] = new EncoderParameter(Encoder.Quality, 95L);
                    image.Save(path, jpegInfo, jpegParameters);
                    break;

                default:
                    Log.Warn("Requested invalid file format '{0}'", format);
                    throw new ArgumentException(String.Format("Invalid file format '{0}'", format));
            }
        }
    }
}
