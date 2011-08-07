using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebPictureDirectory
    {
        public List<WebPicture> Pictures { get; set; }
        public List<String> SubDirectories { get; set; }
    }
}
