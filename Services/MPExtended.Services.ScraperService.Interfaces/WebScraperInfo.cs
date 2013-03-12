using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.ScraperService.Interfaces
{
    public class WebScraperInfo
    {
        public String CurrentAction { get; set; }
        public int CurrentProgress { get; set; }
        public int InputNeeded { get; set; }
        public WebScraperState ScraperState { get; set; }

        public override string ToString()
        {
            return ScraperState.ToString();
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null || obj.GetType() != typeof(WebScraperInfo))
            {
                return false;
            }

            WebScraperInfo info = obj as WebScraperInfo;
            return CurrentAction == info.CurrentAction && 
                CurrentProgress == info.CurrentProgress && 
                InputNeeded == info.InputNeeded && 
                ScraperState == info.ScraperState;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
