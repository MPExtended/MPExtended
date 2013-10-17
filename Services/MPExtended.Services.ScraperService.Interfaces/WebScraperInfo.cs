using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.ScraperService.Interfaces
{
    public class WebScraperInfo
    {
        /// <summary>
        /// What is the scraper doing currently
        /// </summary>
        public String CurrentAction { get; set; }

        /// <summary>
        /// How war is the current action progressed (0-100)
        /// </summary>
        public int CurrentProgress { get; set; }

        /// <summary>
        /// How many scraper input requests are waiting for user interaction
        /// </summary>
        public int InputNeeded { get; set; }

        /// <summary>
        /// The current state of the scraper
        /// </summary>
        public WebScraperState ScraperState { get; set; }

        public override string ToString()
        {
            return ScraperState.ToString();
        }
    }
}
