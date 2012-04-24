using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Playlist
{
    public class WebPlaylistItem : WebMediaItem, ITitleSortable
    {
        public WebPlaylistItem()
        {

        }

        public WebPlaylistItem(Music.WebMusicTrackBasic track)
        {
            this.Id = track.Id;
            this.PID = track.PID;
            this.Title = track.Title;
            this.Type = track.Type;
            this.Duration = track.Duration;
            this.DateAdded = track.DateAdded;
            this.Path = track.Path;
        }

        public String Title { get; set; }
        public int Duration { get; set; }
    }
}
