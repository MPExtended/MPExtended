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
    public class WebProgramDetailed : WebProgramBasic
    {
        #region Constructor
        public WebProgramDetailed()
        {
        }
        #endregion

        #region Properties
        public string Classification { get; set; }
        public string EpisodeName { get; set; }
        public string EpisodeNum { get; set; }
        public string EpisodeNumber { get; set; }
        public string EpisodePart { get; set; }
        public string Genre { get; set; }
        public bool HasConflict { get; set; }
        public bool IsChanged { get; set; }
        public bool IsPartialRecordingSeriesPending { get; set; }
        public bool IsRecording { get; set; }
        public bool IsRecordingManual { get; set; }
        public bool IsRecordingOnce { get; set; }
        public bool IsRecordingOncePending { get; set; }
        public bool IsRecordingSeries { get; set; }
        public bool IsRecordingSeriesPending { get; set; }
        public bool Notify { get; set; }
        public DateTime OriginalAirDate { get; set; }
        public int ParentalRating { get; set; }
        public string SeriesNum { get; set; }
        public int StarRating { get; set; }
        #endregion
    }
}
