using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.Common.Interfaces
{
    [DataContract]
    public enum WebSortOrder
    {
        [EnumMember]
        Asc = 0,
        [EnumMember]
        Desc = 1,
        [EnumMember]
        Custom = 2
    }
}
