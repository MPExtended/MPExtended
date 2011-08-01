using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.ServiceModel.Web;
using System.Windows.Media.Imaging;
using System.Linq;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService.Code
{
    internal class MPPictures
    {
        public static WebPictureDirectory GetPictureDirectory(string path)
        {
            if (!Shares.IsAllowedPath(path))
            {
                Log.Warn("Tried to GetPictureDirectory on non-allowed or non-existent directory {0}", path);
                return null;
            }

            string[] extensions = Shares.GetAllShares(Shares.ShareType.Picture).First().Extensions;
            return new WebPictureDirectory() {
                SubDirectories = Shares.GetDirectoryListByPath(path),
                Pictures = Shares.GetFileListByPath(path).Where(p => extensions.Contains(p.Extension.ToLowerInvariant())).Select(p => p.ToWebPicture()).ToList()
            };
        }

        public static Stream GetImage(string path)
        {
            if (!Shares.IsAllowedPath(path))
            {
                Log.Warn("Tried to GetImage on non-allowed or non-existent file {0}", path);
                return null;
            }

            return GetImageTrusted(path);
        }

        public static Stream GetImageTrusted(string path) 
        {
            Dictionary<string, string> commonMimeTypes = new Dictionary<string, string>() {
                { "jpeg", "image/jpeg" },
                { "jpg", "image/jpeg" },
                { "png", "image/png" },
                { "gif", "image/gif" },
                { "bmp", "image/x-ms-bmp" },
            };
            WebFileInfo info = Shares.GetFileInfo(path);
            string mime = commonMimeTypes.ContainsKey(info.Extension) ? commonMimeTypes[info.Extension] : "application/octet-stream";
            
            FileStream fs = File.OpenRead(path);
            WebOperationContext.Current.OutgoingResponse.ContentType = mime;
            return fs;
        }

        /// <summary>
        /// Returns the image object of the given path, resized to fit the maxWidth and maxHeight
        /// parameters, or null if the image doesn't exists
        /// </summary>
        /// <param name="path">Path to image</param>
        /// <param name="maxWidth">Maximum width of image</param>
        /// <param name="maxHeight">Maximum height of image</param>
        /// <returns>Stream of image or null</returns>
        public static Stream GetImageResized(string path, int maxWidth, int maxHeight)
        {
            if (!Shares.IsAllowedPath(path))
            {
                Log.Warn("Tried to GetImage non-allowed or non-existent file {0}", path);
                return null;
            }

            FileInfo imageFile = new FileInfo(path);
            string imagePathHash = imageFile.Directory.ToString().GetHashCode().ToString();

            // string tmpDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended", "imagecache")
            string tmpDir = Path.Combine(Path.GetTempPath(), "MPExtended.Services.MediaAccessService.MPPictures");
            if(!Directory.Exists(tmpDir)) 
                Directory.CreateDirectory(tmpDir);

            string cachedPath = Path.Combine(tmpDir, String.Format("{0}_{1}_{2}.{3}", Path.GetFileNameWithoutExtension(imageFile.Name), maxWidth, maxHeight, "png"));
            if(!File.Exists(cachedPath)) 
            {
                if(!ResizeImage(path, cachedPath, maxWidth, maxHeight)) 
                {
                    return null;
                }
            }

            return GetImageTrusted(cachedPath);
        }

        public static WebPicture ReadFile(string filename)
        {
            if (!Shares.IsAllowedPath(filename))
            {
                Log.Warn("Tried to GetImage non-allowed or non-existent file {0}", filename);
                return null;
            }

            WebPicture pic = new WebPicture();
            BitmapSource img = BitmapFrame.Create(new Uri(filename));

            /* Image data */
            pic.Mpixel = (img.PixelHeight * img.PixelWidth) / (double)1000000;
            pic.Width = Convert.ToString(img.PixelWidth);
            pic.Height = Convert.ToString(img.PixelHeight);
            pic.Dpi = Convert.ToString(img.DpiX * img.DpiY);

            /* Image metadata */
            BitmapMetadata meta = (BitmapMetadata)img.Metadata;
            try
            {
                pic.Title = String.Format("{0}", meta.Title);
                pic.Subject = String.Format("{0}", meta.Subject);
                pic.Comment = String.Format("{0}", meta.Comment);
                pic.DateTaken = String.Format("{0}", meta.DateTaken);
                pic.CameraManufacturer = String.Format("{0}", meta.CameraManufacturer);
                pic.CameraModel = String.Format("{0}", meta.CameraModel);
                pic.Copyright = String.Format("{0}", meta.Copyright);
                pic.Rating = Convert.ToString(meta.Rating);
            }
            catch (Exception ex)
            {
                Log.Error("Error reading picture metadata for " + filename, ex);
            }
            pic.Filename = filename;
            return pic;
        }

        /// <summary>
        /// Resizes the given image and saves the new file to a new location
        /// </summary>
        /// <param name="origFile">Original file</param>
        /// <param name="newFile">Target file</param>
        /// <param name="maxWidth">Maximum width of the image</param>
        /// <param name="maxHeight">Maximum height of the image</param>
        /// <returns>True if resizing succeeded, false otherwise</returns>
        private static bool ResizeImage(string origFile, string newFile, int maxWidth, int maxHeight)
        {
            try
            {
                Image origImage = System.Drawing.Image.FromFile(origFile);

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
                Log.Warn("Couldn't resize image " + origFile, ex);
            }
            return false;
        }
    }
}
