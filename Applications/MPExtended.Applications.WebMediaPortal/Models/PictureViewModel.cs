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
    public IEnumerable<WebPictureFolder> Folders { get; set; }
    public IEnumerable<WebPictureBasic> Pictures { get; set; }
    public IEnumerable<WebMobileVideoBasic> Videos { get; set; }

    public PictureFolderViewModel(string id = null, string filter = null)
    {
      try
      {
        string folderId = string.IsNullOrEmpty(id) ? "_root" : id;
        Folder = Connections.Current.MAS.GetPictureFolderById(Settings.ActiveSettings.PicturesProvider, folderId);
        Folder.Categories = Folder.Categories.Reverse().ToList();

        Folders = Connections.Current.MAS.GetSubFoldersById(Settings.ActiveSettings.PicturesProvider, folderId);
        Pictures = Connections.Current.MAS.GetPicturesBasicByCategory(Settings.ActiveSettings.PicturesProvider, folderId);
        Videos = Connections.Current.MAS.GetMobileVideosBasicByCategory(Settings.ActiveSettings.PicturesProvider, folderId);
      }
      catch (Exception ex)
      {
        Folder = new WebPictureFolder();
        Log.Warn(String.Format("Failed to load Picture root folder"), ex);
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
        Picture.Categories = Picture.Categories.Reverse().ToList();
      }
      catch (Exception ex)
      {
        Log.Warn(String.Format("Failed to load picture {0}", id), ex);
      }
    }
  }

  public class VideoViewModel : MediaItemModel
  {
    public WebMobileVideoBasic Video { get; set; }

    protected override WebMediaItem Item { get { return Video; } }

    public VideoViewModel(WebMobileVideoBasic video)
    {
      Video = video;
    }

    public VideoViewModel(string id)
    {
      try
      {
        Video = Connections.Current.MAS.GetMobileVideoBasicById(Settings.ActiveSettings.PicturesProvider, id);
        Video.Categories = Video.Categories.Reverse().ToList();
      }
      catch (Exception ex)
      {
        Log.Warn(String.Format("Failed to load video {0}", id), ex);
      }
    }
  }

}
