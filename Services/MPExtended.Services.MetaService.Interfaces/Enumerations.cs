using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MPExtended.Services.MetaService.Interfaces
{
    [DataContract]
    public enum WebService
    {
        [EnumMember]
        MediaAccessService = 1,
        [EnumMember]
        TVAccessService = 2,
        [EnumMember]
        StreamingService = 3,
        [EnumMember]
        UserSessionService = 4,
        [EnumMember]
        MetaService = 5,
        [EnumMember]
        WifiRemote = 6
    }
}
