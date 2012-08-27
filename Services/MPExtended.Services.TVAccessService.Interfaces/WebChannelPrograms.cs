using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebChannelPrograms<TProgram> where TProgram : WebProgramBasic
    {
        public int ChannelId { get; set; }
        public IList<TProgram> Programs { get; set; }
    }
}
