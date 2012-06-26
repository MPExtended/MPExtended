using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    [DataContract]
    public enum WebFileType
    {
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

        // 5 is reserved for tv logos in the streaming service
    }
}
