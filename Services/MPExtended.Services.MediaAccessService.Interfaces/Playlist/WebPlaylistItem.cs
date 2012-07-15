using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Music;

namespace MPExtended.Services.MediaAccessService.Interfaces.Playlist
{
    public class WebPlaylistItem : WebMediaItem
    {
        public WebPlaylistItem()
        {
        }

        public WebPlaylistItem(WebMusicTrackBasic track)
        {
            this.Id = track.Id;
            this.PID = track.PID;
            this.Title = track.Title;
            this.Type = track.Type;
            this.Duration = track.Duration;
            this.DateAdded = track.DateAdded;
            this.Path = track.Path;
        }

        public int Duration { get; set; }
    }
}
