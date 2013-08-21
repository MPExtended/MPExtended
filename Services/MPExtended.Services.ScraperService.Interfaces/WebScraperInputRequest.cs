using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.ScraperService.Interfaces
{
    /// <summary>
    /// A request for user interaction
    /// </summary>
    public class WebScraperInputRequest
    {
        /// <summary>
        /// Id of this input request
        /// </summary>
        public String Id { get; set; }

        /// <summary>
        /// Title of this input request (short name)
        /// </summary>
        public String Title { get; set; }

        /// <summary>
        /// Longer description of this input request
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// Input type of this request (e.g. numeric, text, choice,...)
        /// </summary>
        public WebInputTypes InputType { get; set; }

        /// <summary>
        /// If input type is item selection, these are the possible choices
        /// </summary>
        public IList<WebScraperInputMatch> InputOptions { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
