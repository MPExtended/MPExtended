using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.MediaAccessService.Interfaces.Playlist
{
    public class WebPlaylist : WebMediaItem
    {
        public int ItemCount { get; set; }

        public override WebMediaType Type
        {
            get
            {
                return WebMediaType.Playlist;
            }
        }
    }
}
