using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebCard
    {
        #region Properties
        public bool CAM { get; set; }
        public int CamType { get; set; }
        public int DecryptLimit { get; set; }
        public string DevicePath { get; set; }
        public bool Enabled { get; set; }
        public bool GrabEPG { get; set; }
        public int IdCard { get; set; }
        public int IdServer { get; set; }
        public bool IsChanged { get; set; }
        public DateTime LastEpgGrab { get; set; }
        public string Name { get; set; }
        public int netProvider { get; set; }
        public bool PreloadCard { get; set; }
        public int Priority { get; set; }
        public string RecordingFolder { get; set; }
        public int RecordingFormat { get; set; }
        public bool supportSubChannels { get; set; }
        public string TimeShiftFolder { get; set; }
        #endregion
    }
}
