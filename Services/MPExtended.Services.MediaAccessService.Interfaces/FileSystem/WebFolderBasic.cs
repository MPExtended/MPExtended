using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.FileSystem
{
    public class WebFolderBasic : WebFilesystemItem
    {
        public override WebMediaType Type
        {
            get
            {
                return WebMediaType.Folder;
            }
        }
    }
}
