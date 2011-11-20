using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebChannelGroup
    {
        public string GroupName { get; set; }
        public int Id { get; set; }
        public bool IsChanged { get; set; }
        public int SortOrder { get; set; }
        public bool IsRadio { get; set; }
        public bool IsTv { get; set; }
    }
}
