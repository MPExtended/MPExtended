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
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.PlugIns.MAS.FSPictures
{
    [Export(typeof(IPictureLibrary))]
    [ExportMetadata("Name", "FS Pictures")]
    public class FSPictures : IPictureLibrary
    {
        List<WebPictureBasic> picturesBasic = new List<WebPictureBasic>();
        List<WebPictureDetailed> picturesDetailed = new List<WebPictureDetailed>();
        Dictionary<string, WebCategory> categories = new Dictionary<string, WebCategory>();
        Dictionary<string, WebCategory> subCategories = new Dictionary<string, WebCategory>();

        private IPluginData data;
        private string[] extensions = { ".jpg", ".png", ".bmp" };
        #region public
        [ImportingConstructor]
        public FSPictures(IPluginData data)
        {
            this.data = data;

        }

        public IEnumerable<WebPictureBasic> GetAllPicturesBasic()
        {
            picturesBasic.Clear();
            dirSearchWebPictureBasic(data.Configuration["root"].ConfigValue);

            return picturesBasic;
        }

        public IEnumerable<WebPictureDetailed> GetAllPicturesDetailed()
        {
            picturesDetailed.Clear();
            dirSearchWebPictureDetailed(data.Configuration["root"].ConfigValue);
            return picturesDetailed;
        }

        public WebPictureDetailed GetPictureDetailed(string pictureId)
        {
            if (picturesDetailed != null && picturesDetailed.Count > 0)
            {
                picturesDetailed.Single(p => p.Id == pictureId);
            }
            return null;
        }

        public IEnumerable<WebCategory> GetAllPictureCategoriesBasic()
        {
            categories.Clear();
            var root = new DirectoryInfo(data.Configuration["root"].ConfigValue);

            categories.Add(EncodeTo64(root.FullName), new WebCategory() { Title = root.Name, Id = EncodeTo64(root.FullName) });
            foreach (var dir in root.EnumerateDirectories())
            {
                categories.Add(EncodeTo64(dir.FullName), new WebCategory() { Title = dir.Name, Id = EncodeTo64(dir.FullName) });
            }
            return categories.Values;
        }

        public IEnumerable<WebPictureBasic> GetPicturesBasicByCategory(string id)
        {
            List<WebPictureBasic> list = new List<WebPictureBasic>();
            WebCategory dir;
            if (!categories.TryGetValue(id, out dir))
            {
                subCategories.TryGetValue(id, out dir);
            }
            foreach (string strFile in Directory.GetFiles(DecodeFrom64(dir.Id)))
            {
                var file = new FileInfo(strFile);
                if (extensions.Contains(file.Extension.ToLowerInvariant()))
                {
                    picturesBasic.Add(GetWebPictureBasic(file.FullName));
                }
            }
            return list;
        }

        public IEnumerable<WebPictureDetailed> GetPicturesDetailedByCategory(string id)
        {
            List<WebPictureDetailed> list = new List<WebPictureDetailed>();
            WebCategory dir;
            if (!categories.TryGetValue(id, out dir))
            {
                subCategories.TryGetValue(id, out dir);
            }
            foreach (string strFile in Directory.GetFiles(DecodeFrom64(dir.Id)))
            {
                var file = new FileInfo(strFile);
                if (extensions.Contains(file.Extension.ToLowerInvariant()))
                {
                    picturesBasic.Add(GetWebPictureDetailed(file.FullName));
                }
            }
            return list;
        }
        public IEnumerable<WebCategory> GetSubCategoriesBasicById(string categoryId)
        {
            List<WebCategory> list = new List<WebCategory>();
            WebCategory dir;
            if (!categories.TryGetValue(categoryId, out dir))
            {
                subCategories.TryGetValue(categoryId, out dir);
            }
            try
            {
            var root = new DirectoryInfo(DecodeFrom64(dir.Id));
            foreach (var folder in root.EnumerateDirectories())
            {
                if (!subCategories.ContainsKey(EncodeTo64(folder.FullName)))
                {
                    subCategories.Add(EncodeTo64(folder.FullName), new WebCategory() { Title = folder.Name, Id = EncodeTo64(folder.FullName) });
                }
            }            }
            catch (Exception ex)
            {
                data.Log.Warn("exception in GetSubCategoriesBasicById");
            }
            return list;
        }

        public WebFileInfo GetFileInfo(string path)
        {
            return new WebFileInfo(new FileInfo(path));
        }

        public Stream GetFile(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }
        #endregion


        internal void dirSearchWebPictureBasic(string strDir)
        {
            try
            {
                foreach (string strFile in Directory.GetFiles(strDir))
                {
                    var file = new FileInfo(strFile);
                    if (extensions.Contains(file.Extension.ToLowerInvariant()))
                    {
                        picturesBasic.Add(GetWebPictureBasic(file.FullName));
                    }
                }

                foreach (string strDirectory in Directory.GetDirectories(strDir))
                {
                    dirSearchWebPictureBasic(strDirectory);
                }
            }
            catch (Exception ex)
            {
                data.Log.Warn("exception in recursiv picture lookup");
            }
        }

        internal void dirSearchWebPictureDetailed(string strDir)
        {
            try
            {
                foreach (string strFile in Directory.GetFiles(strDir))
                {
                    var file = new FileInfo(strFile);
                    if (extensions.Contains(file.Extension.ToLowerInvariant()))
                    {
                        picturesDetailed.Add(GetWebPictureDetailed(file.FullName));
                    }
                }

                foreach (string strDirectory in Directory.GetDirectories(strDir))
                {
                    dirSearchWebPictureDetailed(strDirectory);
                }
            }
            catch (Exception ex)
            {
                data.Log.Warn("exception in recursiv picture lookup");
            }
        }

        internal WebPictureDetailed GetWebPictureDetailed(string path)
        {
            WebPictureDetailed pic = new WebPictureDetailed();
            BitmapSource img = BitmapFrame.Create(new Uri(path));
            pic.Id = EncodeTo64(path);

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
                //pic.Comment  = String.Format("{0}", meta.Subject);
                pic.Comment = String.Format("{0}", meta.Comment);
                pic.DateTaken = DateTime.Parse(meta.DateTaken);
                pic.CameraManufacturer = String.Format("{0}", meta.CameraManufacturer);
                pic.CameraModel = String.Format("{0}", meta.CameraModel);
                pic.Copyright = String.Format("{0}", meta.Copyright);
                pic.Rating = (float)meta.Rating;
            }
            catch (Exception ex)
            {
                data.Log.Error("Error reading picture metadata for " + path, ex);
            }
            pic.Path.Add(path);
            return pic;
        }

        internal WebPictureBasic GetWebPictureBasic(string path)
        {
            WebPictureBasic pic = new WebPictureBasic();
            BitmapSource img = BitmapFrame.Create(new Uri(path));

            pic.Id = EncodeTo64(path);
            /* Image metadata */
            BitmapMetadata meta = (BitmapMetadata)img.Metadata;
            try
            {
                pic.Title = String.Format("{0}", meta.Title);
                pic.DateTaken = DateTime.Parse(meta.DateTaken);

            }
            catch (Exception ex)
            {
                data.Log.Error("Error reading picture metadata for " + path, ex);
            }
            pic.Path.Add(path);
            return pic;
        }

        static private string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
        static private string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }
    }
}