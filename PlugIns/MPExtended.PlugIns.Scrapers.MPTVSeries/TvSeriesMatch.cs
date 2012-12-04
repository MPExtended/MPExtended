using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.ScraperService.Interfaces;
using WindowPlugins.GUITVSeries.Feedback;

namespace MPExtended.PlugIns.Scrapers.MPTVSeries
{
    public class TvSeriesMatch
    {
        public TvSeriesMatch(WebScraperInputMatch match, CItem series)
        {
            this.Match = match;
            this.SeriesItem = series;
        }
        public WebScraperInputMatch Match { get; set; }
        public CItem SeriesItem { get; set; }
    }
}
