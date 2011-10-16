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
using MPExtended.Services.MediaAccessService.Interfaces.Shared;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal class ImageSource
    {
        public Stream Data { get; set; }
        public string Path { get; set; }

        public ImageSource(string path) {
            this.Path = path;
        }

        public ImageSource(Stream data)
        {
            this.Data = data;
        }

        public ImageSource(string path, Stream data)
        {
            this.Path = path;
            this.Data = data;
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
            if (!source.IsLocalFile)
            {
                Log.Warn("ExtractImage: Source type={0} id={1} is not supported yet", source.MediaType, source.Id);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotImplemented);
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
            string filename = String.Format("ex_{0}_{1}_{2}_{3}.jpg", source.GetInternalName(), startPosition, 
                maxWidth == null ? "null" : maxWidth.ToString(), maxHeight == null ? "null" : maxHeight.ToString());
            string tempFile = Path.Combine(tempDir, filename);

            // maybe it exists
            if (File.Exists(tempFile))
            {
                return StreamImage(new ImageSource(tempFile));
            }

            // execute it
            ProcessStartInfo info = new ProcessStartInfo();
            info.Arguments = String.Format("-ss {0} -vframes 1 -i \"{1}\" {2} -f image2 {3}", startPosition, source.GetPath(), ffmpegResize, tempFile);
            info.FileName = Config.GetFFMpegPath();
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            Process proc = new Process();
            proc.StartInfo = info;
            proc.Start();
            proc.WaitForExit();

            // log when failed
            if (!File.Exists(tempFile))
            {
                Log.Warn("Failed to extract image to temporary file {0}", tempFile);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.InternalServerError);
                return null;
            }
            return StreamImage(new ImageSource(tempFile));
        }

        public static Stream GetResizedImage(WebStreamMediaType mediatype, string id, int maxWidth, int maxHeight)
        {
            return GetResizedImage(mediatype, WebArtworkType.Content, id, 0, maxWidth, maxHeight);
        }

        public static Stream GetResizedImage(WebStreamMediaType mediatype, WebArtworkType artworktype, string id, int offset, int maxWidth, int maxHeight)
        {
            // load file
            ImageSource src = ConvertToImageSource(mediatype, artworktype, id, offset);
            if (src == null)
            {
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return null;
            }

            // create cache path
            string tmpDir = Path.Combine(Path.GetTempPath(), "MPExtended", "imagecache");
            if (!Directory.Exists(tmpDir))
                Directory.CreateDirectory(tmpDir);
            string cachedPath = Path.Combine(tmpDir, String.Format("rs_{0}_{1}_{2}_{3}_{4}.jpg", mediatype, artworktype, id, maxWidth, maxHeight));

            // check for existence on disk
            if (!File.Exists(cachedPath))
            {
                Image orig = Image.FromStream(src.GetDataStream());
                if (!ResizeImage(orig, cachedPath, maxWidth, maxHeight))
                {
                    return null;
                }
            }

            return StreamImage(new ImageSource(cachedPath));
        }

        public static Stream GetImage(WebStreamMediaType mediatype, string id)
        {
            return GetImage(mediatype, WebArtworkType.Content, id, 0);
        }

        public static Stream GetImage(WebStreamMediaType mediatype, WebArtworkType artworktype, string id, int offset)
        {
            ImageSource source = ConvertToImageSource(mediatype, artworktype, id, offset);
            if (source == null)
            {
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return null;
            }

            return StreamImage(source);
        }

        private static ImageSource ConvertToImageSource(WebStreamMediaType mediatype, WebArtworkType artworktype, string id, int offset)
        {
            // handle tv and recordings specially
            if (mediatype == WebStreamMediaType.TV || mediatype == WebStreamMediaType.Recording)
            {
                if (artworktype != WebArtworkType.Logo)
                {
                    Log.Info("Requested invalid artwork mediatype={0} artworktype={1}", mediatype, artworktype);
                    return null;
                }

                // get display name
                int idChannel = mediatype == WebStreamMediaType.TV ? 
                    Int32.Parse(id) : 
                    MPEServices.NetPipeTVAccessService.GetRecordingById(Int32.Parse(id)).IdChannel;
                string channelName = MPEServices.NetPipeTVAccessService.GetChannelBasicById(idChannel).DisplayName;

                // find directory
                string tvLogoDir = Config.GetTVLogoDirectory();
                if (!Directory.Exists(tvLogoDir))
                {
                    Log.Warn("TV logo directory {0} does not exists", tvLogoDir);
                    return null;
                }

                // find image
                DirectoryInfo dirinfo = new DirectoryInfo(Config.GetTVLogoDirectory());
                var matched = dirinfo.GetFiles().Where(x => Path.GetFileNameWithoutExtension(x.Name).ToLowerInvariant() == channelName.ToLowerInvariant());
                if(matched.Count() == 0)
                {
                    Log.Debug("Did not find tv logo for channel {0}", channelName);
                    return null;
                }

                // great, return it
                return new ImageSource(matched.First().FullName);
            }

            // handle all 'standard' media cases
            // validate arguments
            var pathlist = MPEServices.NetPipeMediaAccessService.GetPathList((WebMediaType)mediatype, (WebFileType)artworktype, id);
            if (pathlist == null || pathlist.Count <= offset)
            {
                Log.Info("Requested unavailable artwork (offset not found) mediatype={0} artworktype={1} id={2} offset={3}", mediatype, artworktype, id, offset);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return null;
            }
            string path = pathlist.ElementAt(offset);

            Stream data = null;
            WebFileInfo info = MPEServices.NetPipeMediaAccessService.GetFileInfo((WebMediaType)mediatype, (WebFileType)artworktype, id, offset);
            if (!info.Exists)
            {
                Log.Info("Requested unavailable artwork (file not found) mediatype={0} artworktype={1} id={2} offset={3}", mediatype, artworktype, id, offset);
                return null;
            }
            else if (!info.IsLocalFile)
            {
                data = MPEServices.NetPipeMediaAccessService.RetrieveFile((WebMediaType)mediatype, (WebFileType)artworktype, id, offset);
                return new ImageSource(data);
            }
            else
            {
                return new ImageSource(path);
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
            string extension = src.Path != null ? Path.GetExtension(src.Path) : "";
            string mime = commonMimeTypes.ContainsKey(extension) ? commonMimeTypes[extension] : "application/octet-stream";
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
