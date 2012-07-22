using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.MediaAccessService.Interfaces.FileSystem
{
    public class WebDriveBasic : WebFilesystemItem
    {
        public override WebMediaType Type
        {
            get
            {
                return WebMediaType.Drive;
            }
        }
    }
}
