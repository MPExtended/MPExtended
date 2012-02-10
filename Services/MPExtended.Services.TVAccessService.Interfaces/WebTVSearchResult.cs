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
        Recording = 3,
        [EnumMember]
        TVGroup = 4,
        [EnumMember]
        RadioGroup = 5,
        [EnumMember]
        Program = 6
    }

    public class WebTVSearchResult
    {
        public WebTVSearchResultType Type { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public int Score { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // The reason this isn't a channel id here is that the search function is focused on speed: it should be able to give realtime results
        // while the user is typing. Having to do another request to lookup the display name of a channel doesn't help with that. 
        public string ChannelName { get; set; }
    }
}
