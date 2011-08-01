//Copyright 2010 http://tv4home.codeplex.com
//This file is part of MPExtended.Services.TVAccessService.Interfaces.
//MPExtended.Services.TVAccessService.Interfaces is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 2 of the License, or (at your option) any later version.
//MPExtended.Services.TVAccessService.Interfaces is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU Lesser General Public License along with MPExtended.Services.TVAccessService.Interfaces. If not, see http://www.gnu.org/licenses/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebSchedule
    {
        #region Properties
        public int BitRateMode { get; set; }
        public DateTime Canceled { get; set; }
        public string Directory { get; set; }
        public bool DoesUseEpisodeManagement { get; set; }
        public DateTime EndTime { get; set; }
        public int IdChannel { get; set; }
        public int IdParentSchedule { get; set; }
        public int IdSchedule { get; set; }
        public bool IsChanged { get; set; }
        public bool IsManual { get; set; }
        public DateTime KeepDate { get; set; }
        public int KeepMethod { get; set; }
        public int MaxAirings { get; set; }
        public int PostRecordInterval { get; set; }
        public int PreRecordInterval { get; set; }
        public int Priority { get; set; }
        public string ProgramName { get; set; }
        public int Quality { get; set; }
        public int QualityType { get; set; }
        public int RecommendedCard { get; set; }
        public int ScheduleType { get; set; }
        public bool Series { get; set; }
        public DateTime StartTime { get; set; }
        #endregion
    }
}
