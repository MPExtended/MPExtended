using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public abstract class WebMediaItem : WebObject, IDateAddedSortable
    {
        public WebMediaItem()
        {
            DateAdded = new DateTime(1970, 1, 1);
            Path = new List<string>();
        }

        public string Id { get; set; }
        public IList<string> Path { get; set; }
        public DateTime DateAdded { get; set; }
        public abstract WebMediaType Type { get; }
    }
}
