using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Playlist
{
    public class WebPlaylist : WebMediaItem, ITitleSortable
    {
        public WebMediaType Type { get { return WebMediaType.Playlist; } }
        public String Title { get; set; }
        public int ItemCount { get; set; }
    }
}
