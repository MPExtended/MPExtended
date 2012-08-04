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
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.MediaInfo;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal class ImageMediaSource : MediaSource
    {
        private string path = null;

        public string Extension
        {
            get
            {
                return IsCustomized() ? Path.GetExtension(GetPath()) : GetFileInfo().Extension;
            }
        }

        public override bool Exists
        {
            get
            {
                return IsCustomized() ? File.Exists(GetPath()) : base.Exists;
            }
        }

        public override bool SupportsDirectAccess
        {
            get
            {
                return IsCustomized() ? true : base.SupportsDirectAccess;
            }
        }

        public ImageMediaSource(string path)
            : base(WebMediaType.File, null, "")
        {
            this.path = path;
        }

        public ImageMediaSource(WebMediaType type, int? provider, string id, WebFileType filetype, int offset)
            : base (type, provider, id, filetype, offset)
        {
        }

        protected override bool CheckArguments(WebMediaType mediatype, WebFileType filetype)
        {
            if ((mediatype == WebMediaType.TV || mediatype == WebMediaType.Recording) && filetype == WebFileType.Logo)
                return true;
            return base.CheckArguments(mediatype, filetype);
        }

        protected bool IsCustomized()
        {
            return path != null || ((MediaType == WebMediaType.TV || MediaType == WebMediaType.Recording) && FileType == WebFileType.Logo);
        }

        public override WebFileInfo GetFileInfo()
        {
            if ((MediaType == WebMediaType.TV || MediaType == WebMediaType.Recording) && FileType == WebFileType.Logo)
            {
                // get display name
                int idChannel = MediaType == WebMediaType.TV ?
                    Int32.Parse(Id) :
                    MPEServices.TAS.GetRecordingById(Int32.Parse(Id)).IdChannel;
                string channelFileName = PathUtil.StripInvalidCharacters(MPEServices.TAS.GetChannelBasicById(idChannel).DisplayName, '_');

                // find directory
                string tvLogoDir = Configuration.Streaming.TVLogoDirectory;
                if (!Directory.Exists(tvLogoDir))
                {
                    Log.Warn("TV logo directory {0} does not exists", tvLogoDir);
                    return new WebFileInfo() { Exists = false };
                }

                // find image
                DirectoryInfo dirinfo = new DirectoryInfo(tvLogoDir);
                var matched = dirinfo.GetFiles().Where(x => Path.GetFileNameWithoutExtension(x.Name).ToLowerInvariant() == channelFileName.ToLowerInvariant());
                if (matched.Count() == 0)
                {
                    Log.Debug("Did not find tv logo {0}", channelFileName);
                    return new WebFileInfo() { Exists = false };
                }

                // great, return it
                return new WebFileInfo(matched.First().FullName);
            }

            return path != null ? new WebFileInfo(path) : base.GetFileInfo();
        }
    }

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
                Log.Warn("Failed to extract image to temporary file {0} with command {1}", tempFile, info.Arguments);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.InternalServerError);
                return Stream.Null;
            }

            return StreamImage(new ImageMediaSource(tempFile));
        }

        public static Stream GetResizedImage(ImageMediaSource src, int? maxWidth, int? maxHeight)
        {
            // load file
            if (!src.Exists)
            {
                Log.Info("Requested resized image for non-existing source {0}", src.GetDebugName());
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return Stream.Null;
            }

            // create cache path
            string filename = String.Format("resize_{0}_{1}_{2}.jpg", src.GetUniqueIdentifier(), maxWidth, maxHeight);

            // check for existence on disk
            if (!cache.Contains(filename))
            {
                Image orig;
                using (var impersonator = src.GetImpersonator())
                {
                    orig = Image.FromStream(src.Retrieve());
                }

                if (!ResizeImage(orig, cache.GetPath(filename), maxWidth, maxHeight))
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

        private static bool ResizeImage(Image origImage, string newFile, int? maxWidth, int? maxHeight)
        {
            try
            {
                Resolution newSize = Resolution.Calculate(origImage.Width, origImage.Height, maxWidth, maxHeight, 1);

                Bitmap newImage = new Bitmap(newSize.Width, newSize.Height);

                Graphics g = Graphics.FromImage((Image)newImage);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.DrawImage(origImage, 0, 0, newSize.Width, newSize.Height);
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
