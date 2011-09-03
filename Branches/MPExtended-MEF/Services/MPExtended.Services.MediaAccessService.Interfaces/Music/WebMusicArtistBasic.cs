using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicArtistBasic : ITitleSortable, ICategorySortable
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public IList<string> UserDefinedCategories { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
