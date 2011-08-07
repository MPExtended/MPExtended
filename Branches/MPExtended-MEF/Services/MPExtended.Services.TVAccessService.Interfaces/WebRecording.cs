using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebRecording
    {
        #region Properties
        public string Description { get; set; }
        public DateTime EndTime { get; set; }
        public string EpisodeName { get; set; }
        public string EpisodeNum { get; set; }
        public string EpisodeNumber { get; set; }
        public string EpisodePart { get; set; }
        public string FileName { get; set; }
        public string Genre { get; set; }
        public int IdChannel { get; set; }
        public int IdRecording { get; set; }
        public int Idschedule { get; set; }
        public int IdServer { get; set; }
        public bool IsChanged { get; set; }
        public bool IsManual { get; set; }
        public bool IsRecording { get; set; }
        public int KeepUntil { get; set; }
        public DateTime KeepUntilDate { get; set; }
        public string SeriesNum { get; set; }
        public bool ShouldBeDeleted { get; set; }
        public DateTime StartTime { get; set; }
        public int StopTime { get; set; }
        public int TimesWatched { get; set; }
        public string Title { get; set; }
        #endregion
    }
}
