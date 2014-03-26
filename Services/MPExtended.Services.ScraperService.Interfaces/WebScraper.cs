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

        /// <summary>
        /// Id of the scraper
        /// </summary>
        public int ScraperId { get; set; }

        /// <summary>
        /// Name of the scraper
        /// </summary>
        public String ScraperName { get; set; }

        /// <summary>
        /// Description of the scraper
        /// </summary>
        public String ScraperDescription { get; set; }

        /// <summary>
        /// Current state of the scraper
        /// </summary>
        public WebScraperInfo ScraperInfo { get; set; }
    }
}
