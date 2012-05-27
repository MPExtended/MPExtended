using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebScheduledRecording
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ProgramName { get; set; }
        public string ChannelName { get; set; }
        public int IdChannel { get; set; }
        public int IdSchedule { get; set; }
        public int IdProgram { get; set; }
    }
}
