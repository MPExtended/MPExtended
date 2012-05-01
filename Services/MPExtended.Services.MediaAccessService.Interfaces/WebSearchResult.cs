using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebSearchResult : WebObject
    {
        public WebMediaType Type { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public int Score { get; set; }
        public WebDictionary<string> Details { get; set; }

        public WebSearchResult()
        {
            Details = new WebDictionary<string>();
        }
    }
}
