using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.ScraperService.Interfaces;

namespace MPExtended.Scrapers.ScraperManager
{
    public static class ExtensionMethods
    {
        public static String ToString(this WebScraperInputRequest req)
        {
            return req.Title;
        }
    }
}
