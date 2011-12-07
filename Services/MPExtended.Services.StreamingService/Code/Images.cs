#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal class ImageSource
    {
        public Stream Data { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }

        public ImageSource(string path) 
        {
            this.Path = path;
            this.Extension = System.IO.Path.GetExtension(path);
        }

        public ImageSource(Stream data)
        {
            this.Data = data;
        }

        public ImageSource(Stream data, string extension) : this(data)
        {
            this.Extension = extension;
        }

        public Stream GetDataStream()
        {
            if (Data == null)
            {
                return File.OpenRead(Path);
            }

            return Data;
        }
    }

    internal static class Images
    {
        public static Stream ExtractImage(MediaSource source, int startPosition, int? maxWidth, int? maxHeight)
        {
            if (!source.Exists)
            {
                Log.Warn("ExtractImage: Source {0} (resolved {1}) doesn't exists", source.GetDebugName(), source.GetPath());
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return Stream.Null;
            }

            if (!source.IsLocalFile)
            {
                Log.Warn("ExtractImage: Source type={0} id={1} is not supported yet", source.MediaType, source.Id);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotImplemented);
                return Stream.Null;
            }

            // calculate size
            string ffmpegResize = "";
            if (maxWidth != null && maxHeight != null)
            {
                try
                {
                    decimal resolution = MediaInfoWrapper.GetMediaInfo(source).VideoStreams.First().DisplayAspectRatio;
                    ffmpegResize = "-s " + Resolution.Calculate(resolution, new Resolution(maxWidth.Value, maxHeight.Value)).ToString();
                }
                catch (Exception ex)
                {
                    Log.Error("Error while getting resolution of video stream, not resizing", ex);
                }
            }
            
            // get temporary filename
            string tempDir = Path.Combine(Path.GetTempPath(), "MPExtended", "imagecache");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }
            string filename = String.Format("ex_{0}_{1}_{2}_{3}.jpg", source.GetUniqueIdentifier(), startPosition, 
                maxWidth == null ? "null" : maxWidth.ToString(), maxHeight == null ? "null" : maxHeight.ToString());
            string tempFile = Path.Combine(tempDir, filename);

            // maybe it exists in cache, return that then
            if (File.Exists(tempFile))
            {
                return StreamImage(new ImageSource(tempFile));
            }

            // execute it
            using (var impersonator = source.GetImpersonator())
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.Arguments = String.Format("-ss {0} -vframes 1 -i \"{1}\" {2} -f image2 {3}", startPosition, source.GetPath(), ffmpegResize, tempFile);
                info.FileName = Configuration.Streaming.FFMpegPath;
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
                Log.Warn("Failed to extract image to temporary file {0}", tempFile);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.InternalServerError);
                return Stream.Null;
            }

            return StreamImage(new ImageSource(tempFile));
        }

        public static Stream GetResizedImage(MediaSource source, int maxWidth, int maxHeight)
        {
            // load file
            ImageSource src = ConvertToImageSource(source);
            if (src == null)
            {
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return Stream.Null;
            }

            // create cache path
            string tmpDir = Path.Combine(Path.GetTempPath(), "MPExtended", "imagecache");
            if (!Directory.Exists(tmpDir))
                Directory.CreateDirectory(tmpDir);
            string cachedPath = Path.Combine(tmpDir, String.Format("rs_{0}_{1}_{2}.jpg", source.GetUniqueIdentifier(), maxWidth, maxHeight));

            // check for existence on disk
            if (!File.Exists(cachedPath))
            {
                Image orig;
                using (var impersonator = source.GetImpersonator())
                {
                    orig = Image.FromStream(src.GetDataStream());
                }

                if (!ResizeImage(orig, cachedPath, maxWidth, maxHeight))
                {
                    WCFUtil.SetResponseCode(System.Net.HttpStatusCode.InternalServerError);
                    return Stream.Null;
                }
            }

            return StreamImage(new ImageSource(cachedPath));
        }

        public static Stream GetImage(MediaSource msource)
        {
            ImageSource source = ConvertToImageSource(msource);
            if (source == null)
            {
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return Stream.Null;
            }

            return StreamImage(source);
        }

        private static ImageSource ConvertToImageSource(MediaSource source)
        {
            // handle tv and recordings specially
            if (source.MediaType == WebStreamMediaType.TV || source.MediaType == WebStreamMediaType.Recording)
            {
                if (source.FileType != WebArtworkType.Logo)
                {
                    Log.Info("Requested invalid artwork mediatype={0} artworktype={1}", source.MediaType, source.FileType);
                    return null;
                }

                // get display name
                int idChannel = source.MediaType == WebStreamMediaType.TV ? 
                    Int32.Parse(source.Id) : 
                    MPEServices.TAS.GetRecordingById(Int32.Parse(source.Id)).IdChannel;
                string channelFileName = PathUtil.StripInvalidCharacters(MPEServices.TAS.GetChannelBasicById(idChannel).DisplayName, '_');

                // find directory
                string tvLogoDir = Configuration.Streaming.TVLogoDirectory;
                if (!Directory.Exists(tvLogoDir))
                {
                    Log.Warn("TV logo directory {0} does not exists", tvLogoDir);
                    return null;
                }

                // find image
                DirectoryInfo dirinfo = new DirectoryInfo(tvLogoDir);
                var matched = dirinfo.GetFiles().Where(x => Path.GetFileNameWithoutExtension(x.Name).ToLowerInvariant() == channelFileName.ToLowerInvariant());
                if(matched.Count() == 0)
                {
                    Log.Debug("Did not find tv logo {0}", channelFileName);
                    return null;
                }

                // great, return it
                return new ImageSource(matched.First().FullName);
            }

            // handle all 'standard' media cases
            if (!source.Exists)
            {
                Log.Info("Requested unavailable artwork (file not found) {0}", source.GetDebugName());
                return null;
            }
            else 
            {
                return new ImageSource(source.Retrieve(), source.FileInfo.Extension);
            }
        }

        private static Stream StreamImage(ImageSource src) 
        {
            Dictionary<string, string> commonMimeTypes = new Dictionary<string, string>() {
                { ".jpeg", "image/jpeg" },
                { ".jpg", "image/jpeg" },
                { ".png", "image/png" },
                { ".gif", "image/gif" },
                { ".bmp", "image/x-ms-bmp" },
            };
            string mime = commonMimeTypes.ContainsKey(src.Extension) ? commonMimeTypes[src.Extension] : "application/octet-stream";
            WebOperationContext.Current.OutgoingResponse.ContentType = mime;

            return src.GetDataStream();
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
