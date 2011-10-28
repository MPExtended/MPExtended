using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Picture
{
    public class WebPictureBasic : WebMediaItem, ITitleSortable, IDateAddedSortable, IPictureDateTakenSortable
    {
        public WebPictureBasic()
        {
            DateTaken = new DateTime(1970, 1, 1);
            Path = new List<string>();
        }

        public string CategoryId { get; set; }
        public string Title { get; set; }
        public DateTime DateTaken { get; set; }

        public override WebMediaType Type
        {
            get
            {
                return WebMediaType.Picture;
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}