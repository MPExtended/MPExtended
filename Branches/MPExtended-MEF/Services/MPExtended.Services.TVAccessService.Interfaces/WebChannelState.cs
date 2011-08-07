using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    [DataContract]
    public enum WebChannelState
    {
        [EnumMember]
        NotTunable = 0,
        [EnumMember]
        Tunable = 1,
        [EnumMember]
        Timeshifting = 2,
        [EnumMember]
        Recording = 3
    }
}
