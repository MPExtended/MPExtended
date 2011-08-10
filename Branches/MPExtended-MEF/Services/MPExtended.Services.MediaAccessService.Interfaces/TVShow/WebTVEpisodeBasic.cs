using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVEpisodeBasic
    {
        public string EpisodeId { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public bool IsProtected { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
