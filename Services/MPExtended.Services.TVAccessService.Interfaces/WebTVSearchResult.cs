using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    [DataContract]
    public enum WebTVSearchResultType
    {
        [EnumMember]
        TVChannel = 0,
        [EnumMember]
        RadioChannel = 1,
        [EnumMember]
        Schedule = 2,
        [EnumMember]
        Recording = 3
    }

    public class WebTVSearchResult
    {
        public WebTVSearchResultType Type { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public int Score { get; set; }
    }
}
