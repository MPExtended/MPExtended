using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebChannelState
    {
        [DataContract]
        public enum States
        {
            [EnumMember]
            Unknown = -1,
            [EnumMember]
            NotTunable = 0,
            [EnumMember]
            Tunable = 1,
            [EnumMember]
            Timeshifting = 2,
            [EnumMember]
            Recording = 3
        }

        public int ChannelId { get; set; }
        public States State { get; set; }
    }


}
