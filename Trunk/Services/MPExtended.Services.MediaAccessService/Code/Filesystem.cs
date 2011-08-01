using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;
using MPExtended.Services.MediaAccessService.Interfaces;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using MPExtended.Services.MediaAccessService.Code;
using MPExtended.Services.MediaAccessService.Code.Helper;
using System.Xml;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Media.Imaging;

namespace MPExtended.Services.MediaAccessService.Code
{
    internal class FileSystem
    {
        public static WebFileInfo GetFileInfo(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    return new WebFileInfo(path);
                }
                catch (Exception ex)
                {
                    Log.Error("Error getting file info for " + path, ex);
                }
            }
            return null;
        }


        public static byte[] GetFile(string filePath)
        {
            FileStream fileStream = null;
            BinaryReader reader = null;
            byte[] fileByteArray = null;

            if (File.Exists(filePath))
            {
                try
                {
                    fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    reader = new BinaryReader(fileStream);

                    fileByteArray = reader.ReadBytes((int)fileStream.Length);
                }
                catch (Exception ex)
                {
                    Log.Error("Error getting file " + filePath, ex);
                    throw ex;
                }

            }
            return fileByteArray;
        }
        public static List<String> GetDirectoryListByPath(string path)
        {
            List<string> directoryPaths = new List<string>();

            if (Directory.Exists(path))
            {

                if (System.IO.Directory.EnumerateDirectories(path) != null)
                {
                    foreach (string directory in System.IO.Directory.EnumerateDirectories(path))
                    {
                        directoryPaths.Add(directory);
                    }
                }
            }

            return directoryPaths;
        }
        public static List<WebFileInfo> GetFilesFromDirectory(string path)
        {
            List<WebFileInfo> fileList = new List<WebFileInfo>();

            try
            {
                System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(path);
                System.IO.FileInfo[] _files = directoryInfo.GetFiles("*.*");
                if (_files != null)
                {
                    foreach (System.IO.FileInfo file in _files)
                    {
                        fileList.Add(new WebFileInfo(file));

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error getting files from directory " + path, ex);
                return null;
            }
            return fileList;
        }

        public static Stream GetImage(string path)
        {
            if (System.IO.File.Exists(path))
            {
                try
                {
                    FileStream fs = File.OpenRead(path);
                    WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";
                    return fs;
                }
                catch (Exception ex)
                {
                    Log.Error("Error getting image " + path, ex);
                }
            }
            return null;
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
            if (path != null && System.IO.File.Exists(path))
            {
                try
                {
                    FileInfo imageFile = new FileInfo(path);
                    String imagePathHash = imageFile.Directory.ToString().GetHashCode().ToString();

                    String appFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    String newPath = appFolder + "\\MPExtended.Services.MediaAccessService\\imagecache\\" + imagePathHash;

                    if (!Directory.Exists(newPath)) Directory.CreateDirectory(newPath);

                    newPath = newPath + "\\" + Path.GetFileNameWithoutExtension(imageFile.Name) + "_" + maxWidth +
                              "_" + maxHeight + imageFile.Extension;

                    if (!File.Exists(newPath))
                    {
                        if (!ResizeImage(path, newPath, maxWidth, maxHeight))
                        {
                            return null;
                        }
                    }

                    FileStream fs = File.OpenRead(newPath);
                    if (imageFile.Extension.Equals("png"))
                    {
                        WebOperationContext.Current.OutgoingResponse.ContentType = "image/png";
                    }
                    else
                    {
                        WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";
                    }
                    return fs;
                }
                catch (Exception ex)
                {
                    Log.Error("Error getting image " + path, ex);
                }
            }
            else
            {
                Log.Warn("Image " + path + " doesn't exist");
            }
            return null;
        }

        /// <summary>
        /// Returns the stream to a media item (video, music, image,...)
        /// </summary>
        /// <param name="path">Path to the local file</param>
        /// <returns>Stream of file or null if file not found</returns>
        public static Stream GetMediaItem(string path)
        {
            if (path != null && System.IO.File.Exists(path))
            {
                try
                {
                    FileInfo fi = new FileInfo(path);
                    OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                    //context.ContentLength = fi.Length;
                    context.Headers.Add(System.Net.HttpResponseHeader.CacheControl, "public");
                    context.Headers.Add(System.Net.HttpResponseHeader.ContentLength, fi.Length.ToString());
                    context.Headers.Add(System.Net.HttpResponseHeader.ContentType, "application/binary");
                    context.StatusCode = System.Net.HttpStatusCode.OK;
                    context.LastModified = fi.LastWriteTime;

                    FileStream fs = File.OpenRead(path);
                    return fs;
                }
                catch (Exception ex)
                {
                    Log.Error("Error getting media item " + path, ex);
                }
            }
            else
            {
                Log.Warn("Media Item " + path + " doesn't exist");
            }
            return null;
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
                if (origFile != null && System.IO.File.Exists(origFile))
                {
                    System.Drawing.Image origImage = System.Drawing.Image.FromFile(origFile);

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

                    //return (Image)b;

                    // Save resized picture
                    newImage.Save(newFile);
                    return true;
                }
                else
                {
                    Log.Warn("Image " + origFile + " doesn't exist");
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Couldn't resize image " + origFile, ex);
            }
            return false;
        }

    }
}
