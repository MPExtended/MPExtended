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
