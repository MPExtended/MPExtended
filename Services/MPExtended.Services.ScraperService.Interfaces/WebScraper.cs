using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.ScraperService.Interfaces
{
    public class WebScraper
    {
        public override String ToString()
        {
            return ScraperName;
        }

        public int ScraperId { get; set; }
        public String ScraperName { get; set; }
        public String ScraperDescription { get; set; }
    }
}
