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
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;

namespace MPExtended.Applications.WebMediaPortal.Models
{
  public class ShareFoldersViewModel
  {
    public int Provider { get; set; }
    public WebFolderBasic Folder { get; set; }
    public IEnumerable<WebDriveBasic> Drives { get; set; }
    public IEnumerable<WebFolderBasic> Folders { get; set; }
    public IEnumerable<WebFileBasic> Files{ get; set; }

    public ShareFoldersViewModel(int id, string folderId = null, string filter = null)
    {
      Provider = id;
      try
      {
        if(string.IsNullOrEmpty(folderId))
        {
          {
            Folder = new WebFolderBasic();
            Drives = Connections.Current.MAS.GetFileSystemDrives(Provider);
            Folders = new List<WebFolderBasic>();
            Files = new List<WebFileBasic>();
          }
        }
        else
        {
          Folder = Connections.Current.MAS.GetFileSystemFolderBasicById(Provider, folderId);
          Drives = new List<WebDriveBasic>();
          Folders = Connections.Current.MAS.GetFileSystemFolders(Provider, folderId);
          Files = Connections.Current.MAS.GetFileSystemFiles(Provider, folderId);
        }
      }
      catch (Exception ex)
      {
        Log.Warn(String.Format("Failed to load Picture folder"), ex);
      }
    }
  }

  public class ShareDriveViewModel : MediaItemModel
  {
    public int Provider { get; set; }
    public WebDriveBasic Drive { get; set; }

    protected override WebMediaItem Item { get { return Drive; } }

    public ShareDriveViewModel(WebDriveBasic drive)
    {
      Drive = drive;
    }

    public ShareDriveViewModel(int id, string driveId)
    {
      Provider = id;
      try
      {
        Drive = Connections.Current.MAS.GetFileSystemDriveBasicById(Provider, driveId);
      }
      catch (Exception ex)
      {
        Log.Warn(String.Format("Failed to load drive {0}", id), ex);
      }
    }
  }

  public class ShareFolderViewModel : MediaItemModel
  {
    public int Provider { get; set; }
    public WebFolderBasic Folder { get; set; }

    protected override WebMediaItem Item { get { return Folder; } }

    public ShareFolderViewModel(WebFolderBasic folder)
    {
      Folder = folder;
    }

    public ShareFolderViewModel(int id, string folderId)
    {
      Provider = id;
      try
      {
        Folder = Connections.Current.MAS.GetFileSystemFolderBasicById(Provider, folderId);
      }
      catch (Exception ex)
      {
        Log.Warn(String.Format("Failed to load folder {0}", id), ex);
      }
    }
  }

  public class ShareFileViewModel : MediaItemModel
  {
    public int Provider { get; set; }
    public WebFileBasic File { get; set; }

    protected override WebMediaItem Item { get { return File; } }

    public ShareFileViewModel(WebFileBasic file)
    {
      File = file;
    }

    public ShareFileViewModel(int id, string fileId)
    {
      Provider = id;
      try
      {
        File = Connections.Current.MAS.GetFileSystemFileBasicById(Provider, fileId);
      }
      catch (Exception ex)
      {
        Log.Warn(String.Format("Failed to load file {0}", id), ex);
      }
    }
  }
}
