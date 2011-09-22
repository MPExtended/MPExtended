using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.StreamingService.Interfaces
{
    [DataContract]
    public enum WebArtworkSource
    {
        // please be compatible with WebStreamMediaType
        [EnumMember]
        Movie = 0,
        [EnumMember]
        Music = 1,
        [EnumMember]
        TVShow = 3,
        [EnumMember]
        TVSeason = 5,
    }
}
