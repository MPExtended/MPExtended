using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.Services.MediaAccessService.Interfaces.FileSystem
{
    public class WebFile : WebMediaItem
    {
        public string Id { get; set; }
        public string Path { get; set; }

        public WebMediaType Type
        {
            get
            {
                return WebMediaType.File;
            }
            set
            {
                Type = value;
            }
        }
    }
}
