using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.ScraperService.Interfaces
{
    public class WebScraperItem
    {
        /// <summary>
        /// Id of the item
        /// </summary>
        public String ItemId { get; set; }

        /// <summary>
        /// Available actions for this item
        /// </summary>
        public List<WebScraperAction> ItemActions { get; set; }

        /// <summary>
        /// Title of the item
        /// </summary>
        public String Title { get; set; }

        /// <summary>
        /// Description of the item
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// Current state (action) of the item (e.g. online-lookup, copying,...)
        /// </summary>
        public String State { get; set; }

        /// <summary>
        /// Normalized (0-100) value of how far the current action/state has progressed
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Input request if user input is needed on this item
        /// </summary>
        public WebScraperInputRequest InputRequest { get; set; }

        /// <summary>
        /// Date when this item was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}
