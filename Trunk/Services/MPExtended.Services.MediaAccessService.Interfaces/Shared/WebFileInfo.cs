using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Shared
{
    public class WebFileInfo
    {
        public WebFileInfo()
        {
            LastAccessTime = new DateTime(1970, 1, 1);
            LastModifiedTime = new DateTime(1970, 1, 1);
        }

        public WebFileInfo(FileInfo info)
        {
            IsLocalFile = true;
            Size = info.Length;
            Name = info.Name;
            Path = info.FullName;
            LastAccessTime = info.LastAccessTime;
            LastModifiedTime = info.LastWriteTime;
            Extension = info.Extension;
            IsReadOnly = info.IsReadOnly;
        }

        public bool IsLocalFile { get; set; }
        public long Size { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastModifiedTime { get; set; }
        public string Extension { get; set; }
        public bool IsReadOnly { get; set; }

        public override string ToString()
        {
            return Path;
        }
    }
}
