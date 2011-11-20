using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    [DataContract]
    public enum ChannelState
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

    [DataContract]
    public enum OrderBy
    {
        [EnumMember]
        Asc = 0,
        [EnumMember]
        Desc = 1
    }

    [DataContract]
    public enum SortBy
    {
        [EnumMember]
        User = 0,
        [EnumMember]
        Name = 1,
        [EnumMember]
        Channel = 2
    }
}
