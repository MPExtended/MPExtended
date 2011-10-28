using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebSearchResult : WebObject
    {
        public WebMediaType Type { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public int Score { get; set; }
    }
}
