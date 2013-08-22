using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.ScraperService.Interfaces
{
    /// <summary>
    /// Used to give the user a list of items to choose from
    /// </summary>
    public class WebScraperInputMatch
    {
        /// <summary>
        /// Id of input match
        /// </summary>
        public String Id { get; set; }

        /// <summary>
        /// Title of the input match
        /// </summary>
        public String Title { get; set; }

        /// <summary>
        /// Description of the input match
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// First Aired information of this item (if available)
        /// </summary>
        public DateTime FirstAired { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
