using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebProgramBasic
    {
        public string Description { get; set; }
        public DateTime EndTime { get; set; }
        public int IdChannel { get; set; }
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public string Title { get; set; }

        // additional properties
        public int DurationInMinutes { get; set; }
        public bool IsScheduled { get; set; }
    }
}
