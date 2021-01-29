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
using System.Linq;
using System.Web.Mvc;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
  [ServiceAuthorize]
  public class ShareLibraryController : BaseController
  {
    //
    // GET: /ShareLibrary/
    public ActionResult Index(int? id = null, string folderId = null, string filter = null)
    {
      if (!id.HasValue &&  !Settings.ActiveSettings.FileSystemProvider.HasValue)
      {
        return HttpNotFound();
      }
      int provider = id.HasValue ? id.Value : Settings.ActiveSettings.FileSystemProvider.Value;
      var model = new ShareFoldersViewModel(provider, folderId, filter);
      if (model.Drives == null && model.Folders == null && model.Files == null)
      {
        return HttpNotFound();
      }
      return View(model);
    }

    public ActionResult Drive(int? id, string driveId)
    {
      if (!id.HasValue && !Settings.ActiveSettings.FileSystemProvider.HasValue)
      {
        return HttpNotFound();
      }
      int provider = id.HasValue ? id.Value : Settings.ActiveSettings.FileSystemProvider.Value;
      var model = new ShareDriveViewModel(provider, driveId);
      if (model.Drive == null)
        return HttpNotFound();
      return View(model);
    }

    [HttpGet]
    public ActionResult DriveInfo(int? id, string driveId)
    {
      if (!id.HasValue && !Settings.ActiveSettings.FileSystemProvider.HasValue)
      {
        return HttpNotFound();
      }
      int provider = id.HasValue ? id.Value : Settings.ActiveSettings.FileSystemProvider.Value;
      var model = new ShareDriveViewModel(provider, driveId);
      if (model.Drive == null)
        return HttpNotFound();
      return Json(model, JsonRequestBehavior.AllowGet);
    }

    public ActionResult Folder(int? id, string folderId)
    {
      if (!id.HasValue && !Settings.ActiveSettings.FileSystemProvider.HasValue)
      {
        return HttpNotFound();
      }
      int provider = id.HasValue ? id.Value : Settings.ActiveSettings.FileSystemProvider.Value;
      var model = new ShareFolderViewModel(provider, folderId);
      if (model.Folder == null)
        return HttpNotFound();
      return View(model);
    }

    [HttpGet]
    public ActionResult FolderInfo(int? id, string folderId)
    {
      if (!id.HasValue && !Settings.ActiveSettings.FileSystemProvider.HasValue)
      {
        return HttpNotFound();
      }
      int provider = id.HasValue ? id.Value : Settings.ActiveSettings.FileSystemProvider.Value;
      var model = new ShareFolderViewModel(provider, folderId);
      if (model.Folder == null)
        return HttpNotFound();
      return Json(model, JsonRequestBehavior.AllowGet);
    }

    public ActionResult File(int? id, string fileId)
    {
      int provider = id.HasValue ? id.Value : Settings.ActiveSettings.FileSystemProvider.Value;
      var model = new ShareFileViewModel(provider, fileId);
      if (model.File == null)
        return HttpNotFound();
      return View(model);
    }

    [HttpGet]
    public ActionResult FileInfo(int? id, string fileId)
    {
      if (!id.HasValue && !Settings.ActiveSettings.FileSystemProvider.HasValue)
      {
        return HttpNotFound();
      }
      int provider = id.HasValue ? id.Value : Settings.ActiveSettings.FileSystemProvider.Value;
      var model = new ShareFileViewModel(provider, fileId);
      if (model.File == null)
        return HttpNotFound();
      return Json(model, JsonRequestBehavior.AllowGet);
    }
    
    public ActionResult DeleteFile(int? id, string fileId)
    {
      if (!id.HasValue && !Settings.ActiveSettings.FileSystemProvider.HasValue)
      {
        return HttpNotFound();
      }
      int? provider = id.HasValue ? id : Settings.ActiveSettings.FileSystemProvider;
      var file = Connections.Current.MAS.GetFileSystemFileBasicById(provider, fileId);
      if (file == null)
      {
        return HttpNotFound();
      }
      Connections.Current.MAS.DeleteFile(provider, fileId);
      return RedirectToAction("Index", "ShareLibrary", new { id = provider, folderId = file.Categories.Last().Id });
    }  

    public ActionResult DriveCover(int? id, string itemId, int width = 0, int height = 0)
    {
      int provider = id.HasValue ? id.Value : Settings.ActiveSettings.FileSystemProvider.Value;
      return Images.ReturnFromService(WebMediaType.Drive, itemId, WebFileType.Cover, width, height, "Images/default/drive-cover.png", forProvider: provider);
    }

    public ActionResult FolderCover(int? id, string itemId, int width = 0, int height = 0)
    {
      int provider = id.HasValue ? id.Value : Settings.ActiveSettings.FileSystemProvider.Value;
      return Images.ReturnFromService(WebMediaType.Folder, itemId, WebFileType.Cover, width, height, "Images/default/folder-cover.png", forProvider: provider);
    }

    public ActionResult FileCover(int? id, string itemId, int width = 0, int height = 0)
    {
      int provider = id.HasValue ? id.Value : Settings.ActiveSettings.FileSystemProvider.Value;
      return Images.ReturnFromService(WebMediaType.File, itemId, WebFileType.Cover, width, height, "Images/default/file-cover.png", forProvider: provider);
    }

  }
}
