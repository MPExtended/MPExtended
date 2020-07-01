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
    public IEnumerable<WebCategory> Breadcrumbs { get; set; }
    public IEnumerable<WebCategory> Folders { get; set; }
    public IEnumerable<WebPictureBasic> Pictures { get; set; }
    
    private List<WebCategory> Paths = new List<WebCategory>();

    public PictureFolderViewModel(string filter = null)
    {
      try
      {
        Paths = new List<WebCategory>();
        var categoryList = Connections.Current.MAS.GetPictureCategories(Settings.ActiveSettings.PicturesProvider, filter);
        var root = categoryList.Where(x => !string.IsNullOrEmpty(x.Title) && x.Id == "_root")
                               .Select(x => new WebCategory()
                               {
                                 Id = x.Id,
                                 Title = x.Title,
                                 PID = x.PID,
                                 Description = x.Description
                               }).First();
        Paths.Add(root);
        Folders = Connections.Current.MAS.GetPictureSubCategories(Settings.ActiveSettings.PicturesProvider, root.Id);
        Pictures = Connections.Current.MAS.GetPicturesBasicByCategory(Settings.ActiveSettings.PicturesProvider, root.Id);

        int idx = Paths.Select(x => x.Id).ToList().IndexOf(root.Id);
        Breadcrumbs = idx < 0 ? Paths : Paths.Take(idx + 1).ToList();
      }
      catch (Exception ex)
      {
        Log.Warn(String.Format("Failed to load Picture root folder"), ex);
      }
    }

    public PictureFolderViewModel(string id, string folder, string filter = null)
    {
      try
      {
        if (!Paths.Any(x => x.Id == id))
        {
          Paths.Add(new WebCategory() {
                          Id = id,
                          Title = folder,
          });
        }
        
        Folders = Connections.Current.MAS.GetPictureSubCategories(Settings.ActiveSettings.PicturesProvider, id);
        Pictures = Connections.Current.MAS.GetPicturesBasicByCategory(Settings.ActiveSettings.PicturesProvider, id);
        
        int idx = Paths.Select(x => x.Id).ToList().IndexOf(id);
        Breadcrumbs = idx < 0 ? Paths : Paths.Take(idx + 1).ToList();
      }
      catch (Exception ex)
      {
        Log.Warn(String.Format("Failed to load Picture folder {0}", folder), ex);
      }
    }
  }

  public class PictureViewModel : MediaItemModel
  {
    public WebPictureDetailed Picture { get; set; }

    protected override WebMediaItem Item { get { return Picture; } }

    public PictureViewModel(WebPictureDetailed picture)
    {
      Picture = picture;
    }

    public PictureViewModel(string id)
    {
      try
      {
        Picture = Connections.Current.MAS.GetPictureDetailedById(Settings.ActiveSettings.PicturesProvider, id);
      }
      catch (Exception ex)
      {
        Log.Warn(String.Format("Failed to load picture {0}", id), ex);
      }
    }
  }
}
