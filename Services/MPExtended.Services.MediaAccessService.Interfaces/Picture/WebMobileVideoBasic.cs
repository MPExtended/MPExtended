using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.MediaAccessService.Interfaces.Picture
{
    public class WebMobileVideoBasic : WebMediaItem, IDateAddedSortable, IPictureDateTakenSortable, ICategorySortable
    {
        public WebMobileVideoBasic()
        {
            DateTaken = new DateTime(1970, 1, 1);
            Categories = new List<WebCategory>();
        }

        public IList<WebCategory> Categories { get; set; }
        public DateTime DateTaken { get; set; }
        public string Description { get; set; }

        public override WebMediaType Type
        {
            get
            {
                return WebMediaType.MobileVideo;
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
