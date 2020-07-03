#region Copyright (C) 2020 Team MediaPortal
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.Service;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;

namespace MPExtended.Applications.WebMediaPortal.Models
{
  public class PictureFolderViewModel
  {
    public WebPictureFolder Folder { get; set; }
    public IEnumerable<WebCategory> Breadcrumbs { get; set; }
    public IEnumerable<WebPictureFolder> Folders { get; set; }
    public IEnumerable<WebPictureBasic> Pictures { get; set; }
    
    public PictureFolderViewModel(string filter = null)
    {
      try
      {
        string id = "_root";
        Folder = Connections.Current.MAS.GetPictureFolderById(Settings.ActiveSettings.PicturesProvider, id);

        Folders = Connections.Current.MAS.GetSubFoldersById(Settings.ActiveSettings.PicturesProvider, id);
        Pictures = Connections.Current.MAS.GetPicturesBasicByCategory(Settings.ActiveSettings.PicturesProvider, id);
      }
      catch (Exception ex)
      {
        Folder = new WebPictureFolder();
        Breadcrumbs = new List<WebCategory>();
        
        Log.Warn(String.Format("Failed to load Picture root folder"), ex);
      }
    }

    public PictureFolderViewModel(string id, string folder, string filter = null)
    {
      try
      {
        Folder = Connections.Current.MAS.GetPictureFolderById(Settings.ActiveSettings.PicturesProvider, id);
        Breadcrumbs = Folder.Categories;
        
        Folders = Connections.Current.MAS.GetSubFoldersById(Settings.ActiveSettings.PicturesProvider, id);
        Pictures = Connections.Current.MAS.GetPicturesBasicByCategory(Settings.ActiveSettings.PicturesProvider, id);
      }
      catch (Exception ex)
      {
        Folder = new WebPictureFolder();
        Breadcrumbs = new List<WebCategory>();
        
        Log.Warn(String.Format("Failed to load Picture folder {0}", folder), ex);
      }
    }
  }

  public class PictureViewModel : MediaItemModel
  {
    public WebPictureDetailed Picture { get; set; }
    public IEnumerable<WebCategory> Breadcrumbs { get; set; }

    protected override WebMediaItem Item { get { return Picture; } }

    public PictureViewModel(WebPictureDetailed picture)
    {
      Picture = picture;
      Breadcrumbs = Picture.Categories;
    }

    public PictureViewModel(string id)
    {
      try
      {
        Picture = Connections.Current.MAS.GetPictureDetailedById(Settings.ActiveSettings.PicturesProvider, id);
        Breadcrumbs = Picture.Categories;
      }
      catch (Exception ex)
      {
        Breadcrumbs = new List<WebCategory>();
        Log.Warn(String.Format("Failed to load picture {0}", id), ex);
      }
    }
  }
}
