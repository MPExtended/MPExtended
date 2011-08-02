using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService.Code
{
    public static class WebFileInfoExtensionMethods
    {
        public static WebPicture ToWebPicture(this WebFileInfo info)
        {
            return MPPictures.GetPicture(info.FullName);
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
        public static WebFileInfo ToWebFileInfo(this FileInfo info)
        {
            return new WebFileInfo()
            {
                DirectoryName = info.DirectoryName,
                Exists = info.Exists, // isn't this always true?
                Extension = info.Extension,
                FullName = info.FullName,
                IsReadOnly = info.IsReadOnly,
                LastAccessTime = info.LastAccessTime,
                LastWriteTime = info.LastWriteTime,
                Length = info.Length,
                Name = info.Name
            };
        }
    }
}
