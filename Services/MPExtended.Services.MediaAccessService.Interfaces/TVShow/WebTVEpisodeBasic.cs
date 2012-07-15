using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVEpisodeBasic : WebMediaItem, IDateAddedSortable, IRatingSortable, ITVEpisodeNumberSortable, ITVDateAiredSortable
    {
        public WebTVEpisodeBasic()
        {
            ExternalId = new List<WebExternalId>();
        }

        public string ShowId { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string SeasonId { get; set; }
        public bool IsProtected { get; set; }
        public bool Watched { get; set; }
        public float Rating { get; set; }
        public DateTime FirstAired { get; set; }
        public IList<WebExternalId> ExternalId { get; set; }

        public override WebMediaType Type
        {
            get
            {
                return WebMediaType.TVEpisode;
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
