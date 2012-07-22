using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.FileSystem
{
    public class WebFilesystemItem : WebMediaItem
    {
        public DateTime LastAccessTime { get; set; }
        public DateTime LastModifiedTime { get; set; }
    }
}
