using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebUser
    {
        public int CardId { get; set; }
        public DateTime HeartBeat { get; set; }
        public int IdChannel { get; set; }
        public bool IsAdmin { get; set; }
        public string Name { get; set; }
        public int SubChannel { get; set; }
        public int TvStoppedReason { get; set; }
    }
}
