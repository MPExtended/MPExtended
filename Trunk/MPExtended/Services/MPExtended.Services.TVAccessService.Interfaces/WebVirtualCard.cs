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
    public class WebVirtualCard
    {
        #region Properties
        public int BitRateMode { get; set; }
        public string ChannelName { get; set; }
        public string Device { get; set; }
        public bool Enabled { get; set; }
        public int GetTimeshiftStoppedReason { get; set; }
        public bool GrabTeletext { get; set; }
        public bool HasTeletext { get; set; }
        public int Id { get; set; }
        public int IdChannel { get; set; }
        public bool IsGrabbingEpg { get; set; }
        public bool IsRecording { get; set; }
        public bool IsScanning { get; set; }
        public bool IsScrambled { get; set; }
        public bool IsTimeShifting { get; set; }
        public bool IsTunerLocked { get; set; }
        public int MaxChannel { get; set; }
        public int MinChannel { get; set; }
        public string Name { get; set; }
        public int QualityType { get; set; }
        public string RecordingFileName { get; set; }
        public string RecordingFolder { get; set; }
        public int RecordingFormat { get; set; }
        public int RecordingScheduleId { get; set; }
        public DateTime RecordingStarted { get; set; }
        public string RemoteServer { get; set; }
        public string RTSPUrl { get; set; }
        public int SignalLevel { get; set; }
        public int SignalQuality { get; set; }
        public string TimeShiftFileName { get; set; }
        public string TimeshiftFolder { get; set; }
        public DateTime TimeShiftStarted { get; set; }
        public int Type { get; set; }
        public WebUser User { get; set; }
        #endregion
    }
}
