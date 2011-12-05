#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
                { WebScheduleType.WeeklyEveryTimeOnThisChannel, "Every week on this time on this channel" }
            };

        public int Id { get; set; }
        public string Title { get; set; }
        [DataType(DataType.Time)]
        public DateTime StartTime { get; set; }
        [DataType(DataType.Time)]
        public DateTime EndTime { get; set; }
        public string Type { get; set; }

        public ScheduleViewModel(WebScheduleBasic schedule)
        {
            Id = schedule.Id;
            Title = schedule.ProgramName;
            StartTime = schedule.StartTime;
            EndTime = schedule.EndTime;
            Type = ScheduleTypeNames[schedule.ScheduleType];
        }
    }

    public class AddScheduleViewModel
    {
        public int ProgramId { get; set; }
        public string ProgramTitle { get; set; }
        public IEnumerable<SelectListItem> Types { get; set; }

        public AddScheduleViewModel(WebProgramBasic program)
        {
            ProgramId = program.Id;
            ProgramTitle = program.Title;
            Types = ScheduleViewModel.ScheduleTypeNames
                .Select(x => new SelectListItem() { Value = ((int)x.Key).ToString(), Text = x.Value })
                .ToList();
        }
    }
}