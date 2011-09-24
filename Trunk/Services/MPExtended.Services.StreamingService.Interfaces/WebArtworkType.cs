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
        // present values should equal to MPExtended.Services.MediaAccessService.WebFileType
        [EnumMember]
        Content = 0,
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
