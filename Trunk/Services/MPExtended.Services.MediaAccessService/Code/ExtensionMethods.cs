using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService.Code
{
    public static class WebFileInfoExtensionMethods
    {
        public static WebPicture ToWebPicture(this WebFileInfo info)
        {
            if (info == null)
                return null;
            return MPPictures.readFile(info.FullName);
        }
    }

  
}
