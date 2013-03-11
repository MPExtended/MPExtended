#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Mvc;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Applications.WebMediaPortal.Strings;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class ScheduleViewModel
    {
        public static Dictionary<WebScheduleType, string> ScheduleTypeNames = new Dictionary<WebScheduleType, string>()
            {
                { WebScheduleType.Once, FormStrings.ScheduleTypeOnce },
                { WebScheduleType.Daily, FormStrings.ScheduleTypeDaily },
                { WebScheduleType.Weekly, FormStrings.ScheduleTypeWeekly },
                { WebScheduleType.EveryTimeOnThisChannel, FormStrings.ScheduleTypeEveryTimeOnThisChannel },
                { WebScheduleType.EveryTimeOnEveryChannel, FormStrings.ScheduleTypeEveryTimeOnEveryChannel },
                { WebScheduleType.Weekends, FormStrings.ScheduleTypeWeekends },
                { WebScheduleType.WorkingDays, FormStrings.ScheduleTypeWorkingDays },
                { WebScheduleType.WeeklyEveryTimeOnThisChannel, FormStrings.ScheduleTypeWeeklyOnThisChannel }
            };

        public int ProgramId { get; set; }
        public int Id { get; set; }

        [StringLength(255)]
        [LocalizedDisplayName(typeof(FormStrings), "ScheduleTitle")]
        [Required(ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "FieldRequired")]
        public string Title { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "ScheduleStartTime")]
        [Required(ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "FieldRequired")]
        public DateTime? StartTime { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "ScheduleEndTime")]
        [Required(ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "FieldRequired")]
        public DateTime? EndTime { get; set; }
   
        [LocalizedDisplayName(typeof(FormStrings), "ScheduleType")]
        [ListChoice("ScheduleTypeList", AllowNull = false, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ScheduleSelectValidType")]
        public WebScheduleType ScheduleType { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "ScheduleChannel")]
        [ListChoice("ChannelList", AllowNull = false, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ScheduleSelectValidChannel")]
        public int Channel { get; set; }

        public string ChannelName { get; private set; }
        public string StartTimeFormatted { get; private set; }
        public string EndTimeFormatted { get; private set; }

        public IEnumerable<SelectListItem> ChannelList
        {
            get
            {
                return Connections.Current.TAS.GetChannelsDetailed(sort: WebSortField.Title)
                        .Where(x => x.VisibleInGuide)
                        .Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Title });
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

        public ScheduleViewModel(WebProgramDetailed program)
            : this (program.StartTime, program.EndTime, program.Title, program.ChannelId)
        {
            ProgramId = program.Id;

            ChannelName = Connections.Current.TAS.GetChannelDetailedById(program.ChannelId).Title;
        }

        public ScheduleViewModel(WebScheduleBasic schedule)
            : this (schedule.StartTime, schedule.EndTime, schedule.Title, schedule.ChannelId)
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
                    ChannelName = Connections.Current.TAS.GetChannelDetailedById(schedule.ChannelId).Title;
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
                case WebScheduleType.WeeklyEveryTimeOnThisChannel:
                    StartTimeFormatted = StartTime.Value.ToString("dddd");
                    break;
                case WebScheduleType.EveryTimeOnEveryChannel:
                case WebScheduleType.EveryTimeOnThisChannel:
                    // they don't have a time associated with them
                    break;
            }
        }
    }
}