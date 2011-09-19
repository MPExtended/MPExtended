using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVEpisodeDetailed : WebTVEpisodeBasic
    {
        public IList<string> GuestStars { get; set; }
        public IList<string> Directors { get; set; }
        public IList<string> Writers { get; set; }
    }
}
