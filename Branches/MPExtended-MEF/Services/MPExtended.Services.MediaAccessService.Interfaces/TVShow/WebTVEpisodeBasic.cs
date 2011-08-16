using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVEpisodeBasic : WebMediaItem
    {
        public WebTVEpisodeBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
        }
        public string Id { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public bool IsProtected { get; set; }
        public DateTime DateAdded { get; set; }

        public override string ToString()
        {
            return Title;
        }


        public WebMediaType Type
        {
            get
            {
                return WebMediaType.TVShow;
            }
            set
            {
                Type = value;
            }
        }
    }
}
