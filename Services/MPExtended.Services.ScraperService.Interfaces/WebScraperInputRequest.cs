using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.ScraperService.Interfaces
{
    public class WebScraperInputRequest : WebObject
    {
        public String Id { get; set; }
        public String Title { get; set; }
        public String Text { get; set; }
        public WebInputTypes InputType { get; set; }

        public IList<WebScraperInputMatch> InputOptions { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
