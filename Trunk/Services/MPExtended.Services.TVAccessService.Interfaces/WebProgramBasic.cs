using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebProgramBasic
    {
        #region Constructor
        public WebProgramBasic()
        {
        }

        #endregion

        #region Properties
        public string Description { get; set; }
        public DateTime EndTime { get; set; }
        public int IdChannel { get; set; }
        public int IdProgram { get; set; }
        public DateTime StartTime { get; set; }
        public string Title { get; set; }

        // additional properties
        public int DurationInMinutes { get; set; }
        public bool IsScheduled { get; set; }
        #endregion
    }
}
