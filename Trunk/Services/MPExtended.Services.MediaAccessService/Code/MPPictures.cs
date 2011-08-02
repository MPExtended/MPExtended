using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
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

        public static WebPicture GetPicture(string path)
        {
            if (!Shares.IsAllowedPath(path))
            {
                Log.Warn("Tried to GetImage non-allowed or non-existent file {0}", path);
                return null;
            }

            WebPicture pic = new WebPicture();
            BitmapSource img = BitmapFrame.Create(new Uri(path));

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
                Log.Error("Error reading picture metadata for " + path, ex);
            }
            pic.Filename = path;
            return pic;
        }
    }
}
