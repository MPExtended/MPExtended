using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Shared
{
    public enum MediaType
    {
        TVShow,
        Movie,
        Music,
        Picture

        
    }
    public interface MediaItem
    {
        // switch to global id system?
        //public string ItemId { get; set; }
        public MediaType Type { get; set; }
        public IList<string> TrustedExtensions { get; set; }

    }
}
