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
        public string Title { get; set; }
        public string ChannelName { get; set; }
        public int ChannelId { get; set; }
        public int ScheduleId { get; set; }
        public int ProgramId { get; set; }
    }
}
