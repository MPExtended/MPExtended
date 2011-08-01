using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebFileInfo
    {
        public long Length { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string DirectoryName { get; set; }
        public bool Exists { get; set; }
        public string Extension { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
