using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVEpisodeBasic : WebMediaItem, ITitleSortable, IDateAddedSortable, IRatingSortable, ITVEpisodeNumberSortable, ITVDateAiredSortable, IArtwork
    {
        public WebTVEpisodeBasic()
        {
            Artwork = new List<WebArtwork>();
            ExternalId = new List<WebExternalId>();
        }

        public string ShowId { get; set; }
        public string Title { get; set; }
        public int EpisodeNumber { get; set; }
        public string SeasonId { get; set; }
        public bool IsProtected { get; set; }
        public bool Watched { get; set; }
        public float Rating { get; set; }
        public IList<WebArtwork> Artwork { get; set; }
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
