using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVEpisodeDetailed : WebTVEpisodeBasic, IRatingSortable
    {
        public DateTime FirstAired { get; set; }
        public int Rating { get; set; }
    }
}
