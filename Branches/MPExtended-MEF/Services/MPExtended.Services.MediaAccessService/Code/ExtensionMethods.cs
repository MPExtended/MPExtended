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
using System.Linq;
using System.Text;
using System.IO;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;

namespace MPExtended.Services.MediaAccessService.Code
{
    public static class WebFileInfoExtensionMethods
    {
        public static WebPictureBasic ToWebPicture(this WebFile info)
        {
            //return MPPictures.GetPicture(info.FullName);
        }
    }

    public static class IEnumerableExtensionMethods
    {
        public static string SelectShareNode(this IEnumerable<KeyValuePair<string, string>> enumerable, string key, int index)
        {
            return enumerable.Where(x => x.Key == key + index).Select(x => x.Value).First();
        }
    }

    public static class FileInfoExtensionMethods
    {
        public static WebFile ToWebFileInfo(this FileInfo info)
        {
            return new WebFile()
            {
                //DirectoryName = info.DirectoryName,
                //Exists = info.Exists, // isn't this always true?
                //Extension = info.Extension,
                //FullName = info.FullName,
                //IsReadOnly = info.IsReadOnly,
                //LastAccessTime = info.LastAccessTime,
                //LastWriteTime = info.LastWriteTime,
                //Length = info.Length,
                //Name = info.Name
            };
        }
    }
}
