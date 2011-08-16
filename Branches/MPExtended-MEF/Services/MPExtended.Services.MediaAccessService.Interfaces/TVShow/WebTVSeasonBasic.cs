using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVSeasonBasic
    {
        public WebTVSeasonBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
        }
        public string Id { get; set; }
        public string Title { get; set; }   
        public bool IsProtected { get; set; }
        public DateTime DateAdded { get; set; }
        public override string ToString()
        {
            return Title;
        }
    }
}
