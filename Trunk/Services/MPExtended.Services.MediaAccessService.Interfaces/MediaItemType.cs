using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    [DataContract]
    public enum MediaItemType
    {
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
    }
}
