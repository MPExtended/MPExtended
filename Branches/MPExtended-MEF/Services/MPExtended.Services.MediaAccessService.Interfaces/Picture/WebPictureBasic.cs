using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Picture
{
    public class WebPictureBasic
    {
        public string PictureId { get; set; }
        public string Title { get; set; }
        public string DateTaken { get; set; }
        public string FilePath { get; set; }

        public override string ToString()
        {
            return Title;
        }
    
    }
}
