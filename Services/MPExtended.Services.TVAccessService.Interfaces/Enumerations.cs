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
    public enum WebScheduleType
    {
        [EnumMember]
        Once = 0,
        [EnumMember]
        Daily = 1,
        [EnumMember]
        Weekly = 2,
        [EnumMember]
        EveryTimeOnThisChannel = 3,
        [EnumMember]
        EveryTimeOnEveryChannel = 4,
        [EnumMember]
        Weekends = 5,
        [EnumMember]
        WorkingDays = 6,
        [EnumMember]
        WeeklyEveryTimeOnThisChannel = 7
    }

    [DataContract]
    public enum WebScheduleKeepMethod
    {
        [EnumMember]
        UntilSpaceNeeded = 0,
        [EnumMember]
        UntilWatched = 1,
        [EnumMember]
        TillDate = 2,
        [EnumMember]
        Always = 3
    }

    [DataContract]
    public enum SortOrder
    {
        [EnumMember]
        Asc = 0,
        [EnumMember]
        Desc = 1
    }

    [DataContract]
    public enum SortField
    {
        [EnumMember]
        User = 0,
        [EnumMember]
        Name = 1,
        [EnumMember]
        Channel = 2,
        [EnumMember]
        StartTime = 3
    }
}
