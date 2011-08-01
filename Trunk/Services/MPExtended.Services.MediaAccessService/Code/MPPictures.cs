using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Code.Helper;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService.Code
{
    internal class MPPictures
    {
        public static WebPictureDirectory GetPictureDirectory(string path)
        {
          

            WebPictureDirectory dir = new WebPictureDirectory();
            if (GetAllShares().Select(p => path.Contains(p.Path)) != null)
            {
                dir.SubDirectories = FileSystem.GetDirectoryListByPath(path);

                dir.Pictures = FileSystem.GetFilesFromDirectory(path).Where(p => (p.Extension.ToLowerInvariant() == ".png") || (p.Extension.ToLowerInvariant() == ".jpg") || (p.Extension.ToLowerInvariant() == ".bmp")).Select(p => p.ToWebPicture()).ToList();
            }
            return dir;
        }
        
        public static WebPicture readFile(string filename)
        {
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
        public static List<WebShare> GetAllShares()
        {
            return ShareUtils.GetAllShares("pictures");
        }


    }
}
