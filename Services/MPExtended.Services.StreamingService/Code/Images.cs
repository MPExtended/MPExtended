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
using System.ServiceModel.Web;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Shared;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal static class Images
    {
        private static ImageCache cache = new ImageCache();

        public static Stream ExtractImage(MediaSource source, long startPosition, int? maxWidth, int? maxHeight)
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

            // calculate size
            string ffmpegResize = "";
            if (maxWidth != null || maxHeight != null)
            {
                try
                {
                    decimal resolution = MediaInfoWrapper.GetMediaInfo(source).VideoStreams.First().DisplayAspectRatio;
                    ffmpegResize = "-s " + Resolution.Calculate(resolution, maxWidth, maxHeight).ToString();
                }
                catch (Exception ex)
                {
                    Log.Error("Error while getting resolution of video stream, not resizing", ex);
                }
            }
            
            // get temporary filename
            string filename = String.Format("extract_{0}_{1}_{2}_{3}.jpg", source.GetUniqueIdentifier(), startPosition, 
                maxWidth == null ? "null" : maxWidth.ToString(), maxHeight == null ? "null" : maxHeight.ToString());
            string tempFile = cache.GetPath(filename);

            // maybe it exists in cache, return that then
            if (cache.Contains(filename))
            {
                return StreamImage(new ImageMediaSource(tempFile));
            }

            // execute it
            ProcessStartInfo info = new ProcessStartInfo();
            using (var impersonator = source.GetImpersonator())
            {
                info.Arguments = String.Format("-ss {0} -i \"{1}\" {2} -vframes 1 -f image2 {3}", startPosition, source.GetPath(), ffmpegResize, tempFile);
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

            return StreamImage(new ImageMediaSource(tempFile));
        }

        public static Stream GetResizedImage(ImageMediaSource src, int? maxWidth, int? maxHeight, string borders)
        {
            // load file
            if (!src.Exists)
            {
                Log.Info("Requested resized image for non-existing source {0}", src.GetDebugName());
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return Stream.Null;
            }

            // create cache path
            string filename = String.Format("resize_{0}_{1}_{2}_{3}.jpg", src.GetUniqueIdentifier(), maxWidth, maxHeight, borders);

            // check for existence on disk
            if (!cache.Contains(filename))
            {
                if (!ResizeImage(src.Retrieve(), cache.GetPath(filename), ImageFormat.Jpeg, maxWidth, maxHeight, borders))
                {
                    WCFUtil.SetResponseCode(System.Net.HttpStatusCode.InternalServerError);
                    return Stream.Null;
                }
            }

            return StreamImage(new ImageMediaSource(cache.GetPath(filename)));
        }

        public static Stream GetImage(ImageMediaSource source)
        {
            return StreamImage(source);
        }

        private static Stream StreamImage(ImageMediaSource src) 
        {
            if (!src.Exists)
            {
                Log.Info("Tried to stream image from non-existing source {0}", src.GetDebugName());
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return Stream.Null;
            }

            Dictionary<string, string> commonMimeTypes = new Dictionary<string, string>() {
                { ".jpeg", "image/jpeg" },
                { ".jpg", "image/jpeg" },
                { ".png", "image/png" },
                { ".gif", "image/gif" },
                { ".bmp", "image/x-ms-bmp" },
            };
            string mime = commonMimeTypes.ContainsKey(src.Extension) ? commonMimeTypes[src.Extension] : "application/octet-stream";
            WCFUtil.SetContentType(mime);

            return src.Retrieve();
        }

        private static bool ResizeImage(Stream inputStream, string newFile, ImageFormat outputFormat, int? maxWidth, int? maxHeight, string borders)
        {
            using (var image = Image.FromStream(inputStream))
            {
                return ResizeImage(Image.FromStream(inputStream), newFile, outputFormat, maxWidth, maxHeight, borders);
            }
        }

        private static bool ResizeImage(Image origImage, string newFile, ImageFormat outputFormat, int? maxWidth, int? maxHeight, string borders)
        {
            try
            {
                if (!String.IsNullOrEmpty(borders) && (!maxWidth.HasValue || !maxHeight.HasValue))
                {
                    Log.Error("ResizeImage() called with a broders value but width or height is null");
                    throw new ArgumentException("Both width and height parameters need to be specified when requesting borders");
                }

                Resolution newSize = Resolution.Calculate(origImage.Width, origImage.Height, maxWidth, maxHeight, 1);
                int bitmapWidth = !String.IsNullOrEmpty(borders) ? maxWidth.Value : newSize.Width;
                int bitmapHeight = !String.IsNullOrEmpty(borders) ? maxHeight.Value : newSize.Height;
                using (Bitmap newImage = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format32bppArgb))
                {
                    using (Graphics graphic = Graphics.FromImage(newImage))
                    {
                        graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphic.SmoothingMode = SmoothingMode.HighQuality;
                        graphic.CompositingQuality = CompositingQuality.HighQuality;
                        graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        if (!String.IsNullOrEmpty(borders))
                            graphic.FillRectangle(new SolidBrush(ColorTranslator.FromHtml("#" + borders)), 0, 0, bitmapWidth, bitmapHeight);

                        int leftOffset = !String.IsNullOrEmpty(borders) ? (maxWidth.Value - newSize.Width) / 2 : 0;
                        int heightOffset = !String.IsNullOrEmpty(borders) ? (maxHeight.Value - newSize.Height) / 2 : 0;
                        graphic.DrawImage(origImage, leftOffset, heightOffset, newSize.Width, newSize.Height);
                    }

                    var ici = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == outputFormat.Guid);
                    var encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)95);
                    newImage.Save(newFile, ici, encoderParams);
                }
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
