using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.ScraperService.Interfaces
{
    public class WebScraperStatus
    {
        public String CurrentAction { get; set; }
        public int CurrentProgress { get; set; }
        public int InputNeeded { get; set; }
        public WebScraperState ScraperState { get; set; }

        public override string ToString()
        {
            return CurrentAction;
        }
    }
}
