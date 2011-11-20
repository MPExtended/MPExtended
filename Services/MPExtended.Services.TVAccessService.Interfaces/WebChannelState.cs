using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebChannelState
    {
        public int ChannelId { get; set; }
        public ChannelState State { get; set; }
    }
}
