using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Playlist
{
    public class WebPlaylist : WebMediaItem
    {
        public WebPlaylist(String id, String name, String path)
        {
            this.Type = WebMediaType.Playlist;
            this.Id = id;
            this.Path = new List<String>();
            this.Path.Add(path);
        }

        public String Name;
        public int ItemCount { get; set; }
    }
}
