using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
   public class WebPictureDirectory
    {
        List<WebPicture> pictures = new List<WebPicture>();
        public List<WebPicture> Pictures
        {
            get
            {
                return pictures;
            }
            set
            {
                pictures = value;
            }

        }

        List<String> subDirectories = new List<string>();
        public List<String> SubDirectories
        {
            get
            {
                return subDirectories;
            }
            set
            {
                subDirectories = value;
            }

        }
    }
}
