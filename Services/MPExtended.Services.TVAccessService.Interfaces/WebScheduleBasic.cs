﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebScheduleBasic
    {
        public int BitRateMode { get; set; }
        public DateTime Canceled { get; set; }
        public string Directory { get; set; }
        public bool DoesUseEpisodeManagement { get; set; }
        public DateTime EndTime { get; set; }
        public int IdChannel { get; set; }
        public int IdParentSchedule { get; set; }
        public int Id { get; set; }
        public bool IsChanged { get; set; }
        public bool IsManual { get; set; }
        public DateTime KeepDate { get; set; }
        public WebScheduleKeepMethod KeepMethod { get; set; }
        public int MaxAirings { get; set; }
        public int PostRecordInterval { get; set; }
        public int PreRecordInterval { get; set; }
        public int Priority { get; set; }
        public string Title { get; set; }
        public int Quality { get; set; }
        public int QualityType { get; set; }
        public int RecommendedCard { get; set; }
        public WebScheduleType ScheduleType { get; set; }
        public bool Series { get; set; }
        public DateTime StartTime { get; set; }
    }
}
