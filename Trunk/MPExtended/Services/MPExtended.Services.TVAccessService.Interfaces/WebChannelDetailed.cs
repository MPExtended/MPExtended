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
    public class WebChannelDetailed : WebChannelBasic
    {
        #region Properties
        public WebProgramDetailed CurrentProgram { get; set; }
        public WebProgramDetailed NextProgram { get; set; }
        public bool EpgHasGaps { get; set; }
        public string ExternalId { get; set; }
        public int FreeToAir { get; set; }
        public bool GrabEpg { get; set; }
        public IList<string> GroupNames { get; set; }
        public bool IsChanged { get; set; }
        public bool IsRadio { get; set; }
        public bool IsTv { get; set; }
        public DateTime LastGrabTime { get; set; }
        public int SortOrder { get; set; }
        public int TimesWatched { get; set; }
        public DateTime TotalTimeWatched { get; set; }
        public bool VisibleInGuide { get; set; }
        #endregion
    }
}
