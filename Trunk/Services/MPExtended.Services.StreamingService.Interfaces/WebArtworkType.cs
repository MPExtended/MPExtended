using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.StreamingService.Interfaces
{
    [DataContract]
    public enum WebArtworkType
    {
        [EnumMember]
        Backdrop = 1,
        [EnumMember]
        Banner = 2,
        [EnumMember]
        Poster = 3,
        [EnumMember]
        Cover = 4
    }
}
