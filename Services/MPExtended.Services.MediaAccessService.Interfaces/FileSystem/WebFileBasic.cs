using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.FileSystem
{
    public class WebFileBasic : WebMediaItem
    {
        public string Name { get; set; }

        public override WebMediaType Type
        {
            get
            {
                return WebMediaType.File;
            }
        }
    }
}
