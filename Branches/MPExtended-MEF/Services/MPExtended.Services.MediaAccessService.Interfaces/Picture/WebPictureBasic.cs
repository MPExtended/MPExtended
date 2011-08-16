using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.Services.MediaAccessService.Interfaces.Picture
{
    public class WebPictureBasic : MediaItem
    {
        public string PictureId { get; set; }
        public string CategoryId { get; set; }
        public string Title { get; set; }
        public string DateTaken { get; set; }
        public string FilePath { get; set; }

        public override string ToString()
        {
            return Title;
        }
    
    }
}
