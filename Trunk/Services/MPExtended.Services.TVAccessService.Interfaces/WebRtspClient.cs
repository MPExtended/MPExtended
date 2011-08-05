using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebRtspClient
    {
        #region Properties
        public DateTime DateTimeStarted { get; set; }
        public string Description { get; set; }
        public string IpAdress { get; set; }
        public bool IsActive { get; set; }
        public string StreamName { get; set; }
        #endregion
    }
}
