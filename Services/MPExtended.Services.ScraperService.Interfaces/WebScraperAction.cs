using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.ScraperService.Interfaces
{
    public class WebScraperAction
    {
        /// <summary>
        /// Id of the action
        /// </summary>
        public String ActionId { get; set; }

        /// <summary>
        /// Title of the item
        /// </summary>
        public String Title { get; set; }

        /// <summary>
        /// Description of the item
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// Is this action available in the current state of the scraper/item
        /// </summary>
        public bool Enabled { get; set; }
    }
}
