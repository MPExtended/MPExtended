using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.ScraperService.Interfaces
{
    public class WebScraperInputMatch
    {
        public object Tag { get; set; }
        public String Id { get; set; }
        public String Title { get; set; }
        public String ImdbId { get; set; }
        public String Description { get; set; }
        public DateTime FirstAired { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
