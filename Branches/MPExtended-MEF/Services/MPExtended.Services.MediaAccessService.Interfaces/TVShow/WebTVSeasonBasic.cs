using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVSeasonBasic
    {
        public string SeasonId { get; set; }
        public string Title { get; set; }   
        public bool IsProtected { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
