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
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using MPExtended.Libraries.Service;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;

namespace MPExtended.PlugIns.MAS.FSPictures
{
    public abstract class PictureLibraryBase : IPictureLibrary
    {
        protected IPluginData data;
        protected string[] Extensions { get; set; }

        public bool Supported { get; set; }

        public PictureLibraryBase(IPluginData data)
        {
            this.data = data;
            Extensions = new string[] { ".jpg", ".png", ".bmp" };
            Supported = true;
        }

        public virtual IEnumerable<WebPictureBasic> GetAllPicturesBasic()
        {
            return GetAllPictureCategories().Select(x => SearchPictures(IdToPath(x.Id), true, GetWebPictureBasic)).SelectMany(x => x);
        }

        public virtual IEnumerable<WebPictureDetailed> GetAllPicturesDetailed()
        {
            return GetAllPictureCategories().Select(x => SearchPictures(IdToPath(x.Id), true, GetWebPictureDetailed)).SelectMany(x => x);
        }

        public WebPictureBasic GetPictureBasic(string pictureId)
        {
            return GetWebPictureBasic(IdToPath(pictureId));
        }

        public WebPictureDetailed GetPictureDetailed(string pictureId)
        {
            return GetWebPictureDetailed(IdToPath(pictureId));
        }

        public IEnumerable<WebCategory> GetSubCategoriesById(string categoryId)
        {
            var list = new List<WebCategory>();
            var root = new DirectoryInfo(IdToPath(categoryId));
            foreach (var folder in root.EnumerateDirectories())
            {
                list.Add(new WebCategory() { Title = folder.Name, Id = PathToId(folder.FullName) });
            }
            return list;
        }

        public IEnumerable<WebPictureBasic> GetPicturesBasicByCategory(string id)
        {
            return SearchPictures(IdToPath(id), false, GetWebPictureBasic);
        }

        public IEnumerable<WebPictureDetailed> GetPicturesDetailedByCategory(string id)
        {
            return SearchPictures(IdToPath(id), false, GetWebPictureDetailed);
        }

        public WebFileInfo GetFileInfo(string path)
        {
            return new WebFileInfo(path);
        }

        public Stream GetFile(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public IEnumerable<WebSearchResult> Search(string text)
        {
            return new List<WebSearchResult>();
        }

        public WebDictionary<string> GetExternalMediaInfo(WebMediaType type, string id)
        {
            return new WebDictionary<string>()
            {
                { "Type", "file" },
                { "Path", GetPictureBasic(id).Path.First() },
            };
        }

        private delegate T CreatePicture<T>(string path);
        private List<T> SearchPictures<T>(string strDir, bool recursive, CreatePicture<T> creator)
        {
            try
            {
                List<T> output = new List<T>();
                foreach (string strFile in Directory.GetFiles(strDir))
                {
                    var file = new FileInfo(strFile);
                    if (Extensions.Contains(file.Extension.ToLowerInvariant()))
                    {
                        output.Add(creator.Invoke(file.FullName));
                    }
                }

                if (recursive)
                {
                    foreach (string strDirectory in Directory.GetDirectories(strDir))
                    {
                        output.AddRange(SearchPictures(strDirectory, true, creator));
                    }
                }

                return output;
            }
            catch (Exception ex)
            {
                Log.Warn("Failed in recursive picture search", ex);
                return new List<T>();
            }
        }

        private WebPictureDetailed GetWebPictureDetailed(string path)
        {
            WebPictureDetailed pic = new WebPictureDetailed();
            pic.Id = PathToId(path);
            pic.Path.Add(path);
            pic.Categories.Add(GetCategoryFromPath(path));

            // Image data
            BitmapSource img = BitmapFrame.Create(new Uri(path));
            pic.Mpixel = (img.PixelHeight * img.PixelWidth) / (double)1000000;
            pic.Width = Convert.ToString(img.PixelWidth);
            pic.Height = Convert.ToString(img.PixelHeight);
            pic.Dpi = Convert.ToString(img.DpiX * img.DpiY);

            // Image metadata
            BitmapMetadata meta = (BitmapMetadata)img.Metadata;
            try
            {
                pic.Title = meta.Title.Trim();
                pic.Comment = meta.Comment;
                pic.DateTaken = DateTime.Parse(meta.DateTaken);
                pic.CameraManufacturer = meta.CameraManufacturer;
                pic.CameraModel = meta.CameraModel;
                pic.Copyright = meta.Copyright;
                pic.Rating = (float)meta.Rating;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error reading picture metadata for {0}", path), ex);
            }

            return pic;
        }

        private WebPictureBasic GetWebPictureBasic(string path)
        {
            WebPictureBasic pic = new WebPictureBasic();
            pic.Id = PathToId(path);
            pic.Path.Add(path);
            pic.Categories.Add(GetCategoryFromPath(path));

            // Image metadata
            BitmapSource img = BitmapFrame.Create(new Uri(path));
            BitmapMetadata meta = (BitmapMetadata)img.Metadata;
            try
            {
                pic.Title = meta.Title.Trim();
                pic.DateTaken = DateTime.Parse(meta.DateTaken);
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error reading picture metadata for {0}", path), ex);
            }

            return pic;
        }

        private WebCategory GetCategoryFromPath(string path)
        {
            string dir = Path.GetDirectoryName(path);

            return new WebCategory()
            {
                Title = Path.GetFileName(dir),
                Id = PathToId(dir)
            };
        }

        public abstract IEnumerable<WebCategory> GetAllPictureCategories();
        protected abstract string PathToId(string fullpath);
        protected abstract string IdToPath(string id);
    }
}