using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Playlist
{
    public class WebPlaylistItem : WebMediaItem, ITitleSortable
    {
        public String Title { get; set; }
        public int Duration { get; set; }
    }
}
