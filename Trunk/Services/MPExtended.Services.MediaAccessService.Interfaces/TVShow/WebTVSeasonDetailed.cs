using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVSeasonDetailed : WebTVSeasonBasic
    {
        public WebTVSeasonDetailed()
        {
            BackdropPaths = new List<string>();
            PosterPaths = new List<string>();
        }

        public IList<string> BackdropPaths { get; set; }
        public IList<string> PosterPaths { get; set; }
    }
}
