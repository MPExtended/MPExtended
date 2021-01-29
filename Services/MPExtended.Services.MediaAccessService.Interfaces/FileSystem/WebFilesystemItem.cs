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
        public IList<WebCategory> Categories { get; set; }

        public WebFilesystemItem()
        {
            LastAccessTime = new DateTime(1970, 1, 1);
            LastModifiedTime = new DateTime(1970, 1, 1);
            Categories = new List<WebCategory>();
        }

    }
}
