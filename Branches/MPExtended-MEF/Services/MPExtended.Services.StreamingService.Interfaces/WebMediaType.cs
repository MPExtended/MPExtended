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
        // see MPExtended.Services.MediaAccessService.Interface.MediaItemType
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
        PictureShareItem = 6,
        [EnumMember]
        ImageItem = 7,

        [EnumMember]
        RecordingItem = 32 // TODO: what value to use here?
    }
}
