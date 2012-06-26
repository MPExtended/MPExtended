﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    [DataContract]
    public enum WebTvMediaType
    {
        // should equal to MPExtended.MediaAccessService.Interfaces.WebMediaType, except for TV and Recording values
        [EnumMember]
        TV = -2,
        [EnumMember]
        Recording = -1
    }

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
        // UI: Once
        [EnumMember]
        Once = 0,

        // UI: Every day at this time
        [EnumMember]
        Daily = 1,

        // UI: Every week at this time
        [EnumMember]
        Weekly = 2,

        // UI: Every time on this channel (starttime ignored)
        [EnumMember]
        EveryTimeOnThisChannel = 3,

        // UI: Every time on every channel (starttime ignored)
        [EnumMember]
        EveryTimeOnEveryChannel = 4,

        // UI: Weekends
        [EnumMember]
        Weekends = 5,

        // UI: Weekdays 
        [EnumMember]
        WorkingDays = 6,

        // UI: Weekly on this channel (starttime ignored)
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
        Title = 2,
        [EnumMember]
        StartTime = 3
    }

    // See TvEngine3/TVLibrary/TvLibrary.Interfaces/CardType.cs
    [DataContract]
    public enum WebCardType
    {
        [EnumMember]
        Analog = 0,
        [EnumMember]
        DvbS = 1,
        [EnumMember]
        DvbT = 2,
        [EnumMember]
        DvbC = 3,
        [EnumMember]
        Atsc = 4,
        [EnumMember]
        RadioWebStream = 5,
        [EnumMember]
        DvbIP = 6,
        [EnumMember]
        Unknown = 7
    }
}
