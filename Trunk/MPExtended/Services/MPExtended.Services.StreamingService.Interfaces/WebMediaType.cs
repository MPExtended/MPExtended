using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.StreamingService.Interfaces
{
    [DataContract]
    public enum WebMediaType
    {
        // same as GMA
        [EnumMember]
        VideoShareItem = 0,
        [EnumMember]
        VideoDatabaseItem = 1,
        [EnumMember]
        TvSeriesItem = 2,
        [EnumMember]
        MovieItem = 3,
        [EnumMember]
        MusicTrackItem = 4,
        [EnumMember]
        MusicShareItem = 5,
        [EnumMember]
        RecordingItem = 10
    }
}
