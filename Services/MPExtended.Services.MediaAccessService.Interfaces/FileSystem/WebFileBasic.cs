using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.MediaAccessService.Interfaces.FileSystem
{
    public class WebFileBasic : WebFilesystemItem
    {
        public long Size { get; set; }

        public override WebMediaType Type
        {
            get
            {
                return WebMediaType.File;
            }
        }
    }
}
