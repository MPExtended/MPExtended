using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.Common.Interfaces
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
        Cover = 4,
        [EnumMember]
        Logo = 5
    }
}
