#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Mvc;
using MPExtended.Libraries.Client;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class ScheduleViewModel
    {
        public static Dictionary<WebScheduleType, string> ScheduleTypeNames = new Dictionary<WebScheduleType, string>()
            {
                { WebScheduleType.Once, "Once" },
                { WebScheduleType.Daily, "Daily" },
                { WebScheduleType.Weekly, "Weekly" },
                { WebScheduleType.EveryTimeOnThisChannel, "Every time on this channel" },
                { WebScheduleType.EveryTimeOnEveryChannel, "Every time on every channel" },
                { WebScheduleType.Weekends, "Weekends" },
                { WebScheduleType.WorkingDays, "Working days" },
                { WebScheduleType.WeeklyEveryTimeOnThisChannel, "Weekly on this channel" }
            };

        public int ProgramId { get; set; }
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [DisplayName("Start time")]
        [Required]
        public DateTime? StartTime { get; set; }

        [DisplayName("End time")]
        [Required]
        public DateTime? EndTime { get; set; }

        [DisplayName("Schedule type")]
        [ListChoice("ScheduleTypeList", AllowNull = false, ErrorMessage = "Select a valid schedule type")]
        public WebScheduleType ScheduleType { get; set; }

        [DisplayName("Channel")]
        [ListChoice("ChannelList", AllowNull = false, ErrorMessage = "Select a valid channel")]
        public int Channel { get; set; }

        public string ChannelName { get; private set; }
        public string StartTimeFormatted { get; private set; }
        public string EndTimeFormatted { get; private set; }

        public IEnumerable<SelectListItem> ChannelList
        {
            get
            {
                return MPEServices.TAS.GetAllChannelsBasic(SortField.Name)
                        .Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.DisplayName });
            }
        }

        public string ScheduleTypeName
        {
            get
            {
                return ScheduleTypeNames[ScheduleType];
            }
        }

        public IEnumerable<SelectListItem> ScheduleTypeList
        {
            get
            {
                return ScheduleTypeNames.Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value });
            }
        }

        public bool IsFromProgram
        {
            get
            {
                return ProgramId != 0;
            }
        }

        public ScheduleViewModel()
        {
            StartTime = null;
            EndTime = null;
        }

        private ScheduleViewModel(DateTime startTime, DateTime endTime, string title, int channelId)
        {
            StartTime = startTime;
            EndTime = endTime;
            Title = title;
            Channel = channelId;
        }

        public ScheduleViewModel(WebProgramBasic program)
            : this (program.StartTime, program.EndTime, program.Title, program.IdChannel)
        {
            ProgramId = program.Id;

            ChannelName = MPEServices.TAS.GetChannelBasicById(program.IdChannel).DisplayName;
        }

        public ScheduleViewModel(WebScheduleBasic schedule)
            : this (schedule.StartTime, schedule.EndTime, schedule.ProgramName, schedule.IdChannel)
        {
            Id = schedule.Id;
            ScheduleType = schedule.ScheduleType;

            switch (ScheduleType)
            {
                case WebScheduleType.Daily:
                case WebScheduleType.EveryTimeOnThisChannel:
                case WebScheduleType.Once:
                case WebScheduleType.Weekends:
                case WebScheduleType.Weekly:
                case WebScheduleType.WeeklyEveryTimeOnThisChannel:
                case WebScheduleType.WorkingDays:
                    ChannelName = MPEServices.TAS.GetChannelBasicById(schedule.IdChannel).DisplayName;
                    break;
                case WebScheduleType.EveryTimeOnEveryChannel:
                    ChannelName = "";
                    break;
            }

            switch (ScheduleType)
            {
                case WebScheduleType.Daily:
                case WebScheduleType.Weekends:
                case WebScheduleType.WorkingDays:
                    StartTimeFormatted = StartTime.Value.ToString("t");
                    EndTimeFormatted = EndTime.Value.ToString("t");
                    break;
                case WebScheduleType.Weekly:
                    StartTimeFormatted = StartTime.Value.ToString("dddd") + " " + StartTime.Value.ToString("t");
                    EndTimeFormatted = EndTime.Value.ToString("dddd") + " " + EndTime.Value.ToString("t");
                    break;
                case WebScheduleType.Once:
                default:
                    StartTimeFormatted = StartTime.Value.ToString("g");
                    EndTimeFormatted = EndTime.Value.ToString("g");
                    break;
                case WebScheduleType.EveryTimeOnEveryChannel:
                case WebScheduleType.EveryTimeOnThisChannel:
                case WebScheduleType.WeeklyEveryTimeOnThisChannel:
                    // they don't have a time associated with them
                    break;
            }
        }
    }
}