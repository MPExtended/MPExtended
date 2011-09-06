using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.StreamingService.Interfaces
{
    [DataContract]
    public enum WebStreamMediaType
    {
        // should equal to MPExtended.MediaAccessService.Interfaces.WebMediaType
        // TODO: maybe change name?
        [EnumMember]
        Recording = -1,
        [EnumMember]
        Movie = 0,
        [EnumMember]
        Music = 1,
        [EnumMember]
        Picture = 2,
        [EnumMember]
        TVShow = 3,
        [EnumMember]
        File = 4,
        [EnumMember]
        Folder = 5
    }
}
