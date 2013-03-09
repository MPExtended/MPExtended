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
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Strings;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class TVGuideViewModel
    {
        public struct TimeMarker
        {
            public DateTime Time { get; set; }
            public bool Last { get; set; }

            public string Format
            {
                get
                {
                    if (Time.Date == DateTime.Now.Date)
                    {
                        return Time.ToShortTimeString();
                    }
                    else
                    {
                        return Time.ToShortDateString() + " " + Time.ToShortTimeString();
                    }
                }
            }

            public string TimeFormat
            {
                get
                {
                    return Time.ToShortTimeString();
                }
            }

            public string FullFormat
            {
                get
                {
                    return Time.ToShortDateString() + " " + Time.ToShortTimeString();
                }
            }
        }

        private IEnumerable<WebChannelGroup> groups;

        public DateTime GuideStart { get; private set; }
        public DateTime GuideEnd { get; private set; }
        public bool HasDateSplit { get; private set; }
        public double FirstDayHours { get; private set; }
        public double SecondDayHours { get; private set; }

        public int GroupId { get; private set; }
        public string GroupName { get; private set; }
        public IEnumerable<TVGuideChannelViewModel> Channels { get; private set; }

        public DateTime EarlierGuideStart { get { return GuideStart.Subtract(GuideEnd - GuideStart); } }
        public DateTime LaterGuideStart { get { return GuideEnd; } }

        public IEnumerable<SelectListItem> Groups
        {
            get
            {
                return groups.Select(x => new SelectListItem() { 
                    Text = x.GroupName, 
                    Value = x.Id.ToString(),
                    Selected = x.Id == GroupId
                });
            }
        }

        public IEnumerable<TimeMarker> TimeMarkers
        {
            get
            {
                for(int i = 0; i < 8; i++)
                {
                    TimeMarker tm = new TimeMarker();
                    tm.Time = GuideStart.AddMinutes(i * 30);
                    tm.Last = i == 7;

                    yield return tm;
                }
            }
        }

        public string DateFormat
        {
            get
            {
                return GuideStart.ToShortDateString();
            }
        }

        public TVGuideViewModel(IEnumerable<WebChannelGroup> groups, WebChannelGroup channelGroup, DateTime guideStart, DateTime guideEnd)
        {
            this.groups = groups;

            GuideStart = guideStart;
            GuideEnd = guideEnd;
            HasDateSplit = GuideStart.Date != GuideEnd.Date && !(GuideEnd.Hour == 0 && GuideEnd.Minute == 0);
            if (!HasDateSplit)
            {
                FirstDayHours = (GuideEnd - GuideStart).Hours + ((double)(GuideEnd - GuideStart).Minutes / 60);
                SecondDayHours = 0;
            }
            else
            {
                FirstDayHours = (24 - GuideStart.Hour) - ((double)GuideStart.Minute / 60);
                SecondDayHours = GuideEnd.Hour + ((double)GuideEnd.Minute / 60);
            }


            GroupId = channelGroup.Id;
            GroupName = channelGroup.GroupName;

            DateTime loadGuideEnd = guideEnd.Subtract(TimeSpan.FromSeconds(1)); // do not load programs that start at the end of the guid
            Channels = Connections.Current.TAS.GetChannelsDetailed(channelGroup.Id).Select(x => new TVGuideChannelViewModel(x, guideStart, loadGuideEnd));
        }
    }

    public class TVGuideChannelViewModel
    {
        public int Id { get; private set; }
        public string DisplayName { get; private set; }
        public IEnumerable<TVGuideProgramViewModel> Programs { get; private set; }

        private IEnumerable<WebProgramDetailed> programList;

        public TVGuideChannelViewModel(WebChannelDetailed channel, DateTime guideStart, DateTime guideEnd)
        {
            Id = channel.Id;
            DisplayName = channel.Title;

            programList = Connections.Current.TAS.GetProgramsDetailedForChannel(Id, guideStart, guideEnd);
            Programs = programList.Select(x => new TVGuideProgramViewModel(x, guideStart, guideEnd));
        }
    }

    public class TVGuideProgramViewModel
    {
        public WebProgramDetailed Program { get; private set; }
        public int Id { get { return Program.Id; } }
        public string Title { get { return String.IsNullOrEmpty(Program.Title) ? UIStrings.Unknown : Program.Title; } }
        public DateTime StartTime { get { return Program.StartTime; } }
        public DateTime EndTime { get { return Program.EndTime; } }
        public bool IsScheduled { get { return Program.IsScheduled; } }
        

        public bool IsCurrent
        {
            get
            {
                return DateTime.Now >= StartTime && DateTime.Now <= EndTime;
            }
        }

        private DateTime guideStart;
        private DateTime guideEnd;

        public TVGuideProgramViewModel(WebProgramDetailed program, DateTime guideStart, DateTime guideEnd)
        {
            Program = program;
            this.guideStart = guideStart;
            this.guideEnd = guideEnd;
        }

        public double GetPercentageWidth()
        {
            // get duration of program
            DateTime calcStartTime = StartTime < guideStart ? guideStart : StartTime;
            DateTime calcEndTime = EndTime > guideEnd ? guideEnd : EndTime;
            double programDuration = (calcEndTime - calcStartTime).TotalMinutes;
            double guideDuration = (guideEnd - guideStart).TotalMinutes;

            // return percentage (scale value a tiny bit down because rounding could result in a total of 100.01%, which doesn't display properly)
            return programDuration / guideDuration * 99.9;
        }

        public string GetCssPercentageWidth()
        {
            // convert to string
            var invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
            return string.Format(invariantCulture, "{0:0.00}%", GetPercentageWidth());
        }

        public string GetPageLink(UrlHelper helper)
        {
            if (IsCurrent)
            {
                return helper.Action("WatchLiveTV", "Television", new { channelId = Program.ChannelId });
            }
            else
            {
                return helper.Action("ProgramDetails", "Television", new { programId = Id });
            }
        }
    }

    public class ProgramDetailsViewModel
    {
        public WebProgramDetailed Program { get; set; }

        public int Id { get { return Program.Id; } }
        public string Title { get { return String.IsNullOrEmpty(Program.Title) ? UIStrings.Unknown : Program.Title; } }
        public string Description { get { return Program.Description; } }
        public DateTime StartTime { get { return Program.StartTime; } }
        public DateTime EndTime { get { return Program.EndTime; } }
        public bool IsScheduled { get { return Program.IsScheduled; } }

        public string ChannelName { get; private set; }
        public int ChannelId { get; private set; }

        public bool CanWatchLive
        {
            get
            {
                return DateTime.Now >= StartTime && DateTime.Now <= EndTime;
            }
        }

        public ProgramDetailsViewModel(WebProgramDetailed program)
        {
            Program = program;

            var channel = Connections.Current.TAS.GetChannelDetailedById(program.ChannelId);
            ChannelName = channel.Title;
            ChannelId = channel.Id;
        }
    }
}