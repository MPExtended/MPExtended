#region Copyright (C) 2011-2013 MPExtended, 2010-2011 TV4Home
// Copyright (C) 2010-2011 TV4Home, http://tv4home.codeplex.com/
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
using System.IO;
using System.Linq;
using System.Text;
using System.ServiceModel;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Extensions;
using MPExtended.Libraries.Service.Hosting;
using MPExtended.Libraries.Service.Network;
using MPExtended.Libraries.Service.Shared;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;
using TvControl;
using TvDatabase;

namespace MPExtended.Services.TVAccessService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class TVAccessService : ITVAccessService
    {
        private const int API_VERSION = 5;

        #region Service
        public static ITVAccessService Instance { get; internal set; }

        private TvBusinessLayer _tvBusiness;
        private IController _tvControl;
        private bool _tveInstalled;

        public TVAccessService()
        {
            _tvBusiness = new TvBusinessLayer();
            InitializeGentleAndTVE();
        }

        private void InitializeGentleAndTVE()
        {
            _tveInstalled = true;

            try
            {
                // Use the same Gentle.config as the TVEngine
                string gentleConfigFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Team MediaPortal", "MediaPortal TV Server", "Gentle.config"
                );

                // but be quiet when it doesn't exists, as not everyone has the TV Engine installed
                if (!File.Exists(gentleConfigFile))
                {
                    Log.Info("Cannot find Gentle.config file, assuming TVEngine isn't installed...");
                    _tveInstalled = false;
                    return;
                }

                Gentle.Common.Configurator.AddFileHandler(gentleConfigFile);

                // connect to tv server via TVE API
                RemoteControl.Clear();
                RemoteControl.HostName = "127.0.0.1"; // Why do we have to even bother with this setting?

                _tvControl = RemoteControl.Instance;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to connect to TVEngine", ex);
            }
        }

        private IUser GetUserByUserName(string userName)
        {
            return Card.ListAll()
                .Where(c => c != null && c.Enabled)
                .SelectMany(c =>
                {
                    var users = _tvControl.GetUsersForCard(c.IdCard);
                    return users != null ? users : new IUser[] { };
                })
                .Where(u => u.Name == userName)
                .FirstOrDefault();
        }

        public WebTVServiceDescription GetServiceDescription()
        {
            return new WebTVServiceDescription()
            {
                HasConnectionToTVServer = TestConnectionToTVService(),
                ApiVersion = API_VERSION,
                ServiceVersion = VersionUtil.GetVersionName(),
            };
        }
        #endregion

        #region TV Server
        public WebBoolResult TestConnectionToTVService()
        {
            if (!_tveInstalled)
                return false;

            if (!RemoteControl.IsConnected)
            {
                return false;
            }

            try
            {
                TvDatabase.Version.ListAll();
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to connected to TV service database", ex);
                return false;
            }
        }

        public WebStringResult ReadSettingFromDatabase(string tagName)
        {
            return _tvBusiness.GetSetting(tagName, "").Value;
        }

        public WebBoolResult WriteSettingToDatabase(string tagName, string value)
        {
            Setting setting = _tvBusiness.GetSetting(tagName, "");
            setting.Value = value;
            setting.Persist();
            return true;
        }

        public IList<WebDiskSpaceInformation> GetLocalDiskInformation(string filter = null)
        {
            return DriveInfo.GetDrives()
                .Select(x => DiskSpaceInformation.GetSpaceInformation(x.Name))
                .Filter(filter)
                .ToList();
        }

        /// <summary>
        /// Return external media info for recording
        /// </summary>
        /// <param name="type">Type of item</param>
        /// <param name="id">Id of recording</param>
        /// <returns>A dictionary object that can be sent to e.g. WifiRemote</returns>
        public WebDictionary<string> GetExternalMediaInfo(WebMediaType? type, string id)
        {
            if (type == WebMediaType.Recording)
            {
                return new WebDictionary<string>()
                {
                    { "Type", "mp recording" },
                    { "Id", id }
                };
            }
            else if (type == WebMediaType.TV)
            {
                return new WebDictionary<string>()
                {
                    { "Type", "mp tvchannel" },
                    { "Id", id }
                };
            }

            return null;
        }

        public IList<WebTVSearchResult> Search(string text, WebTVSearchResultType? type = null)
        {
            if (String.IsNullOrWhiteSpace(text))
            {
                return new List<WebTVSearchResult>();
            }

            text = text.ToLower();
            IEnumerable<WebTVSearchResult> result = new List<WebTVSearchResult>();

            if (type == null || type == WebTVSearchResultType.TVChannel || type == WebTVSearchResultType.RadioChannel)
            {
                result = result.Concat(Channel.ListAll()
                    .Where(channel => channel.DisplayName.Contains(text, StringComparison.CurrentCultureIgnoreCase))
                    .Select(channel => new WebTVSearchResult()
                    {
                        Id = channel.IdChannel.ToString(),
                        Score = 50 + (int)Math.Round((decimal)text.Length / channel.DisplayName.Length * 50),
                        Title = channel.DisplayName,
                        Type = channel.IsTv ? WebTVSearchResultType.TVChannel : WebTVSearchResultType.RadioChannel
                    })
                    .Where(r => type == null || r.Type == type));
            }

            if (type == null || type == WebTVSearchResultType.TVGroup)
            {
                result = result.Concat(ChannelGroup.ListAll()
                    .Where(group => group.GroupName.Contains(text, StringComparison.CurrentCultureIgnoreCase))
                    .Select(group => new WebTVSearchResult()
                    {
                        Id = group.IdGroup.ToString(),
                        Score = 50 + (int)Math.Round((decimal)text.Length / group.GroupName.Length * 50),
                        Title = group.GroupName,
                        Type = WebTVSearchResultType.TVGroup,
                    }));
            }

            if (type == null || type == WebTVSearchResultType.RadioGroup)
            {
                result = result.Concat(RadioChannelGroup.ListAll()
                    .Where(group => group.GroupName.Contains(text, StringComparison.CurrentCultureIgnoreCase))
                    .Select(group => new WebTVSearchResult()
                    {
                        Id = group.IdGroup.ToString(),
                        Score = 50 + (int)Math.Round((decimal)text.Length / group.GroupName.Length * 50),
                        Title = group.GroupName,
                        Type = WebTVSearchResultType.RadioGroup
                    }));
            }

            if (type == null || type == WebTVSearchResultType.Recording)
            {
                result = result.Concat(Recording.ListAll()
                    .Where(rec => rec.Title.Contains(text, StringComparison.CurrentCultureIgnoreCase))
                    .Select(rec => new WebTVSearchResult()
                    {
                        Id = rec.IdRecording.ToString(),
                        Score = 50 + (int)Math.Round((decimal)text.Length / rec.Title.Length * 50),
                        Title = rec.Title,
                        Type = WebTVSearchResultType.Recording,
                        StartTime = rec.StartTime,
                        EndTime = rec.EndTime,
                        ChannelName = GetChannelDisplayName(rec.IdChannel)
                    }));
            }

            if (type == null || type == WebTVSearchResultType.Schedule)
            {
                result = result.Concat(Schedule.ListAll()
                    .Where(schedule => schedule.ProgramName.Contains(text, StringComparison.CurrentCultureIgnoreCase))
                    .Select(schedule => new WebTVSearchResult()
                    {
                        Id = schedule.IdSchedule.ToString(),
                        Score = 50 + (int)Math.Round((decimal)text.Length / schedule.ProgramName.Length * 50),
                        Title = schedule.ProgramName,
                        Type = WebTVSearchResultType.Schedule,
                        ChannelName = GetChannelDisplayName(schedule.IdChannel)
                    }));
            }

            if (type == null || type == WebTVSearchResultType.Program)
            {
                result = result.Concat(_tvBusiness.SearchPrograms("%" + text + "%")
                    .Select(program => new WebTVSearchResult()
                    {
                        Id = program.IdProgram.ToString(),
                        Score = (int)Math.Round(((decimal)text.Length / program.Title.Length * 50) - Math.Abs((decimal)(program.StartTime - DateTime.Now).TotalDays)),
                        Title = program.Title,
                        Type = WebTVSearchResultType.Program,
                        StartTime = program.StartTime,
                        EndTime = program.EndTime,
                        ChannelName = GetChannelDisplayName(program.IdChannel)
                    }));
            }

            return result.OrderByDescending(x => x.Score).ToList();
        }

        private string GetChannelDisplayName(int idChannel)
        {
            Channel channel = Channel.Retrieve(idChannel);
            return channel == null ? String.Empty : channel.DisplayName;
        }

        public IList<WebTVSearchResult> SearchResultsByRange(string text, int start, int end, WebTVSearchResultType? type = null)
        {
            return Search(text, type).TakeRange(start, end).ToList();
        }
        #endregion

        #region Cards
        public IList<WebCard> GetCards(string filter = null)
        {
            return Card.ListAll().Select(c => c.ToWebCard()).Filter(filter).ToList();
        }

        public IList<WebVirtualCard> GetActiveCards(string filter = null)
        {
            return GetTimeshiftingOrRecordingVirtualCards().Select(card => card.ToWebVirtualCard()).Filter(filter).ToList();
        }

        public IList<WebUser> GetActiveUsers(string filter = null)
        {
            return GetTimeshiftingOrRecordingVirtualCards().Select(card => card.User.ToWebUser()).Filter(filter).ToList();
        }

        private IEnumerable<VirtualCard> GetTimeshiftingOrRecordingVirtualCards()
        {
            return Card.ListAll()
                .Where(card => RemoteControl.Instance.CardPresent(card.IdCard))
                .Select(card => RemoteControl.Instance.GetUsersForCard(card.IdCard))
                .Where(users => users != null)
                .SelectMany(user => user)
                .Select(user => new VirtualCard(user, RemoteControl.HostName))
                .Where(tvCard => tvCard.IsTimeShifting || tvCard.IsRecording);
        }

        public IList<WebRtspClient> GetStreamingClients(string filter = null)
        {
            return _tvControl.StreamingClients.Select(cl => cl.ToWebRtspClient()).Filter(filter).ToList();
        }

        public IList<WebDiskSpaceInformation> GetAllRecordingDiskInformation(string filter = null)
        {
            return GetCards()
                .Select(x => x.RecordingFolder).Distinct()
                .Select(x => DiskSpaceInformation.GetSpaceInformation(x))
                .GroupBy(x => x.Disk, (key, list) => list.First())
                .Filter(filter)
                .ToList();
        }

        public WebDiskSpaceInformation GetRecordingDiskInformationForCard(int id)
        {
            string folder = Card.Retrieve(id).ToWebCard().RecordingFolder;
            return DiskSpaceInformation.GetSpaceInformation(folder);
        }
        #endregion

        #region Schedules
        public WebBoolResult StartRecordingManual(string userName, int channelId, string title)
        {
            Log.Debug("Start recording manual on channel " + channelId + ", userName: " + userName);
            return AddSchedule(channelId, title, DateTime.Now, DateTime.Now.AddDays(1), 0);
        }

        public WebBoolResult AddSchedule(int channelId, string title, DateTime startTime, DateTime endTime, WebScheduleType scheduleType)
        {
            return AddScheduleDetailed(channelId, title, startTime, endTime, scheduleType, -1, -1, "", -1);
        }

        public WebBoolResult AddScheduleDetailed(int channelId, string title, DateTime startTime, DateTime endTime, WebScheduleType scheduleType, int preRecordInterval, int postRecordInterval, string directory, int priority)
        {
            try
            {
                Log.Debug("Adding schedule on channel {0} for {1}, {2} till {3}, type {4}", channelId, title, startTime, endTime, scheduleType);
                ScheduleRecordingType scheduleRecType = (ScheduleRecordingType)scheduleType;
                Schedule schedule = _tvBusiness.AddSchedule(channelId, title, startTime, endTime, (int)scheduleRecType);

                schedule.ScheduleType = (int)scheduleRecType;
                schedule.PreRecordInterval = preRecordInterval >= 0 ? preRecordInterval : Int32.Parse(_tvBusiness.GetSetting("preRecordInterval", "5").Value);
                schedule.PostRecordInterval = postRecordInterval >= 0 ? postRecordInterval : Int32.Parse(_tvBusiness.GetSetting("postRecordInterval", "5").Value);

                if (!String.IsNullOrEmpty(directory))
                {
                    schedule.Directory = directory;
                }

                if (priority >= 0)
                {
                    schedule.Priority = priority;
                }

                schedule.Persist();
                _tvControl.OnNewSchedule();
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to add schedule", ex);
                return false;
            }
        }

        public WebIntResult GetScheduleCount(string filter = null)
        {
            return Schedule.ListAll().Select(s => s.ToWebSchedule()).Filter(filter).Count();
        }

        public IList<WebScheduleBasic> GetSchedules(string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return Schedule.ListAll().Select(s => s.ToWebSchedule()).Filter(filter).SortScheduleList(sort, order).ToList();
        }

        public IList<WebScheduleBasic> GetSchedulesByRange(int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return Schedule.ListAll().Select(s => s.ToWebSchedule()).Filter(filter).SortScheduleList(sort, order).TakeRange(start, end).ToList();
        }

        public WebScheduleBasic GetScheduleById(int scheduleId)
        {
            return Schedule.Retrieve(scheduleId).ToWebSchedule();
        }

        public WebBoolResult CancelSchedule(int programId)
        {
            try
            {
                Log.Debug("Canceling schedule for programId {0}", programId);
                var program = Program.Retrieve(programId);
                foreach (Schedule schedule in Schedule.ListAll().Where(schedule => schedule.IsRecordingProgram(program, true)))
                {
                    switch (schedule.ScheduleType)
                    {
                        case (int)ScheduleRecordingType.Once:
                            schedule.Delete();
                            _tvControl.OnNewSchedule();
                            break;
                        default:
                            CanceledSchedule canceledSchedule = new CanceledSchedule(schedule.IdSchedule, schedule.IdChannel, schedule.StartTime);
                            canceledSchedule.Persist();
                            _tvControl.OnNewSchedule();
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to cancel schedule for programId {0}", programId), ex);
                return false;
            }
        }

        public WebBoolResult EditSchedule(int scheduleId, WebScheduleType scheduleType)
        {
            try
            {
                Log.Debug("Editing schedule with id {0} schedule type {1}", scheduleId, scheduleType);
                Schedule schedule = Schedule.Retrieve(scheduleId);

                ScheduleRecordingType scheduleRecType = (ScheduleRecordingType)scheduleType;
                schedule.ScheduleType = (int)scheduleRecType;

                schedule.Persist();

                _tvControl.OnNewSchedule(); // I don't think this is needed, but doesn't hurt either
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to edit schedule {0}", scheduleId), ex);
                return false;
            }
        }

        public WebBoolResult DeleteSchedule(int scheduleId)
        {
            try
            {
                Log.Debug("Deleting schedule with id {0}", scheduleId);
                Schedule schedule = Schedule.Retrieve(scheduleId);
                _tvControl.StopRecordingSchedule(schedule.IdSchedule);

                // delete canceled schedules first
                foreach (var cs in CanceledSchedule.ListAll().Where(x => x.IdSchedule == schedule.IdSchedule))
                    cs.Remove();

                schedule.Remove();
                _tvControl.OnNewSchedule(); // I don't think this is needed, but doesn't hurt either
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to delete schedule {0}", scheduleId), ex);
                return false;
            }
        }

        public WebBoolResult StopRecording(int scheduleId)
        {
            try
            {
                _tvControl.StopRecordingSchedule(scheduleId);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to stop recording for schedule {0}", scheduleId), ex);
                return false;
            }
        }

        private WebScheduledRecording GetScheduledRecording(Schedule schedule, DateTime date)
        {
            // ignore schedules that don't even match the date we are checking for
            ScheduleRecordingType type = (ScheduleRecordingType)schedule.ScheduleType;
            if ((type == ScheduleRecordingType.Weekends && !(date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)) ||
                (type == ScheduleRecordingType.WorkingDays && (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)) ||
                (type == ScheduleRecordingType.Weekly && schedule.StartTime.DayOfWeek != date.DayOfWeek) ||
                (type == ScheduleRecordingType.Once && schedule.StartTime.Date != date.Date) ||
                (type == ScheduleRecordingType.WeeklyEveryTimeOnThisChannel && schedule.StartTime.Date != date.Date))
                return null;

            // retrieve some data
            var channel = Channel.Retrieve(schedule.IdChannel);
            WebScheduledRecording recording = new WebScheduledRecording();
            recording.ScheduleId = schedule.IdSchedule;

            // first check all types that do not require any EPG matching
            ScheduleRecordingType[] noEpgTypes = { ScheduleRecordingType.Daily, ScheduleRecordingType.Weekends, ScheduleRecordingType.WorkingDays, 
                                                   ScheduleRecordingType.Weekly, ScheduleRecordingType.Once };
            if (noEpgTypes.Contains(type))
            {
                recording.ChannelId = channel.IdChannel;
                recording.ChannelName = channel.DisplayName;
                recording.StartTime = schedule.StartTime;
                recording.EndTime = schedule.EndTime;
                var matchingPrograms = _tvBusiness.GetPrograms(channel, schedule.StartTime, schedule.EndTime);
                recording.ProgramId = matchingPrograms.Any() ? matchingPrograms.First().IdProgram : 0;
                recording.Title = matchingPrograms.Any() ? matchingPrograms.First().Title : schedule.ProgramName;
                return recording;
            }

            // all schedule types which reach this far match a program in the EPG
            IList<Program> programs =
                type == ScheduleRecordingType.WeeklyEveryTimeOnThisChannel || type == ScheduleRecordingType.EveryTimeOnThisChannel ?
                    _tvBusiness.GetPrograms(channel, date.Date, date.Date.Add(TimeSpan.FromDays(1))) :
                    _tvBusiness.GetPrograms(date.Date, date.Date.Add(TimeSpan.FromDays(1)));
            var program = programs.FirstOrDefault(x => x.Title == schedule.ProgramName && (x.StartTime > DateTime.Now || date.Date < DateTime.Today));
            if (program == null)
                return null;

            // set properties from the program and channel of the program
            channel = program.ReferencedChannel();
            recording.ChannelId = channel.IdChannel;
            recording.ChannelName = channel.DisplayName;
            recording.StartTime = program.StartTime;
            recording.EndTime = program.EndTime;
            recording.ProgramId = program.IdProgram;
            recording.Title = program.Title;
            return recording;
        }

        public IList<WebScheduledRecording> GetScheduledRecordingsForDate(DateTime date, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return Schedule.ListAll().Select(x => GetScheduledRecording(x, date)).Where(x => x != null).Filter(filter).SortScheduledRecordingList(sort, order).ToList();
        }

        public IList<WebScheduledRecording> GetScheduledRecordingsForToday(string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return GetScheduledRecordingsForDate(DateTime.Today, filter, sort, order);
        }
        #endregion

        #region Channels
        #region TV specific
        public WebIntResult GetGroupCount(string filter = null)
        {
            return ChannelGroup.ListAll().Select(chg => chg.ToWebChannelGroup()).Filter(filter).Count();
        }

        public IList<WebChannelGroup> GetGroups(string filter = null, WebSortField? sort = WebSortField.User, WebSortOrder? order = WebSortOrder.Asc)
        {
            return ChannelGroup.ListAll().Select(chg => chg.ToWebChannelGroup()).Filter(filter).SortGroupList(sort, order).ToList();
        }

        public IList<WebChannelGroup> GetGroupsByRange(int start, int end, string filter = null, WebSortField? sort = WebSortField.User, WebSortOrder? order = WebSortOrder.Asc)
        {
            return ChannelGroup.ListAll().Select(chg => chg.ToWebChannelGroup()).Filter(filter).SortGroupList(sort, order).TakeRange(start, end).ToList();
        }

        public WebChannelGroup GetGroupById(int groupId)
        {
            return ChannelGroup.Retrieve(groupId).ToWebChannelGroup();
        }

        private IEnumerable<Channel> GetChannels(int? groupId, WebSortField? sort, WebSortOrder? order)
        {
            return groupId == null ?
                (IEnumerable<Channel>)Channel.ListAll().Where(ch => ch.IsTv).OrderBy(ch => sort == WebSortField.User ? ch.SortOrder : 1, order) :
                (IEnumerable<Channel>)_tvBusiness.GetTVGuideChannelsForGroup(groupId.Value);
        }

        public WebIntResult GetChannelCount(int? groupId, string filter = null)
        {
            return GetChannels(groupId, null, null).Select(ch => ch.ToWebChannelBasic()).Filter(filter).Count();
        }

        public IList<WebChannelBasic> GetChannelsBasic(int? groupId, string filter = null, WebSortField? sort = WebSortField.User, WebSortOrder? order = WebSortOrder.Asc)
        {
            return GetChannels(groupId, sort, order).Select(ch => ch.ToWebChannelBasic()).Filter(filter).SortChannelList(sort, order).ToList();
        }

        public IList<WebChannelBasic> GetChannelsBasicByRange(int start, int end, int? groupId, string filter = null, WebSortField? sort = WebSortField.User, WebSortOrder? order = WebSortOrder.Asc)
        {
            return GetChannels(groupId, sort, order).Select(ch => ch.ToWebChannelBasic()).Filter(filter).SortChannelList(sort, order).TakeRange(start, end).ToList();
        }

        public IList<WebChannelDetailed> GetChannelsDetailed(int? groupId, string filter = null, WebSortField? sort = WebSortField.User, WebSortOrder? order = WebSortOrder.Asc)
        {
            return GetChannels(groupId, sort, order).Select(ch => ch.ToWebChannelDetailed()).Filter(filter).SortChannelList(sort, order).ToList();
        }

        public IList<WebChannelDetailed> GetChannelsDetailedByRange(int start, int end, int? groupId, string filter = null, WebSortField? sort = WebSortField.User, WebSortOrder? order = WebSortOrder.Asc)
        {
            return GetChannels(groupId, sort, order).Select(ch => ch.ToWebChannelDetailed()).Filter(filter).SortChannelList(sort, order).TakeRange(start, end).ToList();
        }

        public IList<WebChannelState> GetAllChannelStatesForGroup(int groupId, string userName, string filter = null)
        {
            IList<WebChannelBasic> list = GetChannelsBasic(groupId);
            IList<WebChannelState> webChannelStates = new List<WebChannelState>();
            foreach (WebChannelBasic entry in list)
            {
                webChannelStates.Add(GetChannelState(entry.Id, userName));
            }

            return webChannelStates.Filter(filter).ToList();

            /* This is the old implementation which doesn't work due to a NullReferenceException in TvService
            Dictionary<int, ChannelState> channelStates = _tvControl.GetAllChannelStatesForGroup(groupId, GetUserByUserName(userName, true));
            Dictionary<int, WebChannelState> webChannelStates = new Dictionary<int, WebChannelState>();
            if (channelStates != null && channelStates.Count > 0)
            {
                foreach (var entry in channelStates)
                {
                    webChannelStates.Add(entry.Key, entry.Value.ToWebChannelState(entry.Key));
                }
            }

            return webChannelStates;
             */
        }

        #endregion

        #region Radio specific
        public WebIntResult GetRadioGroupCount(string filter = null)
        {
            return RadioChannelGroup.ListAll().Select(chg => chg.ToWebChannelGroup()).Filter(filter).Count();
        }

        public IList<WebChannelGroup> GetRadioGroups(string filter = null, WebSortField? sort = WebSortField.User, WebSortOrder? order = WebSortOrder.Asc)
        {
            return RadioChannelGroup.ListAll().Select(chg => chg.ToWebChannelGroup()).Filter(filter).SortGroupList(sort, order).ToList();
        }

        public IList<WebChannelGroup> GetRadioGroupsByRange(int start, int end, string filter = null, WebSortField? sort = WebSortField.User, WebSortOrder? order = WebSortOrder.Asc)
        {
            return RadioChannelGroup.ListAll().Select(chg => chg.ToWebChannelGroup()).Filter(filter).SortGroupList(sort, order).TakeRange(start, end).ToList();
        }

        public WebChannelGroup GetRadioGroupById(int groupId)
        {
            return RadioChannelGroup.Retrieve(groupId).ToWebChannelGroup();
        }

        private IEnumerable<Channel> GetRadioChannels(int? groupId, WebSortField? sort, WebSortOrder? order)
        {
            return groupId == null ?
                (IEnumerable<Channel>)Channel.ListAll().Where(ch => ch.IsRadio).OrderBy(ch => sort == WebSortField.User ? ch.SortOrder : 1, order) :
                (IEnumerable<Channel>)_tvBusiness.GetRadioGuideChannelsForGroup(groupId.Value);
        }

        public WebIntResult GetRadioChannelCount(int? groupId, string filter = null)
        {
            return GetRadioChannels(groupId, null, null).Select(ch => ch.ToWebChannelBasic()).Filter(filter).Count();
        }

        public IList<WebChannelBasic> GetRadioChannelsBasic(int? groupId, string filter = null, WebSortField? sort = WebSortField.User, WebSortOrder? order = WebSortOrder.Asc)
        {
            return GetRadioChannels(groupId, sort, order).Select(ch => ch.ToWebChannelBasic()).Filter(filter).SortChannelList(sort, order).ToList();
        }

        public IList<WebChannelBasic> GetRadioChannelsBasicByRange(int start, int end, int? groupId, string filter = null, WebSortField? sort = WebSortField.User, WebSortOrder? order = WebSortOrder.Asc)
        {
            return GetRadioChannels(groupId, sort, order).Select(ch => ch.ToWebChannelBasic()).Filter(filter).SortChannelList(sort, order).TakeRange(start, end).ToList();
        }

        public IList<WebChannelDetailed> GetRadioChannelsDetailed(int? groupId, string filter = null, WebSortField? sort = WebSortField.User, WebSortOrder? order = WebSortOrder.Asc)
        {
            return GetRadioChannels(groupId, sort, order).Select(ch => ch.ToWebChannelDetailed()).Filter(filter).SortChannelList(sort, order).ToList();
        }

        public IList<WebChannelDetailed> GetRadioChannelsDetailedByRange(int start, int end, int? groupId, string filter = null, WebSortField? sort = WebSortField.User, WebSortOrder? order = WebSortOrder.Asc)
        {
            return GetRadioChannels(groupId, sort, order).Select(ch => ch.ToWebChannelDetailed()).Filter(filter).SortChannelList(sort, order).TakeRange(start, end).ToList();
        }

        public IList<WebChannelState> GetAllRadioChannelStatesForGroup(int groupId, string userName, string filter = null)
        {
            IList<WebChannelBasic> list = GetRadioChannelsBasic(groupId);
            IList<WebChannelState> webChannelStates = new List<WebChannelState>();
            foreach (WebChannelBasic entry in list)
            {
                webChannelStates.Add(GetChannelState(entry.Id, userName));
            }

            return webChannelStates.Filter(filter).ToList();
        }
        #endregion

        public WebChannelBasic GetChannelBasicById(int channelId)
        {
            return Channel.Retrieve(channelId).ToWebChannelBasic();
        }

        public WebChannelDetailed GetChannelDetailedById(int channelId)
        {
            return Channel.Retrieve(channelId).ToWebChannelDetailed();
        }

        public WebChannelState GetChannelState(int channelId, string userName)
        {
            IUser user = new User();
            user.Name = userName;
            TvControl.ChannelState state = _tvControl.GetChannelState(channelId, user);
            Log.Trace("ChannelId: " + channelId + ", State: " + state.ToString());
            return state.ToWebChannelState(channelId);
        }
        #endregion

        #region Timeshifting
        public WebVirtualCard SwitchTVServerToChannelAndGetVirtualCard(string userName, int channelId)
        {
            return SwitchTVServerToChannel(userName, channelId).ToWebVirtualCard();
        }

        public WebStringResult SwitchTVServerToChannelAndGetStreamingUrl(string userName, int channelId)
        {
            return SwitchTVServerToChannel(userName, channelId).RTSPUrl;
        }

        public WebStringResult SwitchTVServerToChannelAndGetTimeshiftFilename(string userName, int channelId)
        {
            return SwitchTVServerToChannel(userName, channelId).TimeShiftFileName;
        }

        private VirtualCard SwitchTVServerToChannel(string userName, int channelId)
        {
            // validate arguments
            if (String.IsNullOrEmpty(userName))
            {
                Log.Error("Called SwitchTVServerToChannel with empty userName");
                return null;
            }

            // create the user
            Log.Debug("Starting timeshifting with username {0} on channel id {1}", userName, channelId);
            IUser currentUser = new User();
            currentUser.Name = userName;
            currentUser.IsAdmin = true;

            // actually start timeshifting
            VirtualCard tvCard;
            Log.Debug("Starting timeshifting");
            TvResult result = _tvControl.StartTimeShifting(ref currentUser, channelId, out tvCard);
            Log.Trace("Tried to start timeshifting, result {0}", result);

            // make sure result is correct and return
            if (result != TvResult.Succeeded)
            {
                Log.Error("Starting timeshifting failed with result {0}", result);
                return null;
            }

            Log.Debug("Timeshifting succeeded");
            if (tvCard == null)
            {
                Log.Error("Couldn't get virtual card");
                return null;
            }

            return tvCard;
        }

        public WebBoolResult SendHeartbeat(string userName)
        {
            IUser currentUser = GetUserByUserName(userName);
            if (currentUser == null)
            {
                Log.Error("Tried to send heartbeat for invalid user {0}", userName);
                return false;
            }

            _tvControl.HeartBeat(currentUser);
            return true;
        }

        public WebBoolResult CancelCurrentTimeShifting(string userName)
        {
            IUser currentUser = GetUserByUserName(userName);
            if (currentUser == null)
            {
                Log.Warn("Tried to cancel timeshifting for invalid user {0}", userName);
                return false;
            }

            var card = GetTimeshiftingOrRecordingVirtualCards().FirstOrDefault(x => x.User.Name == userName);
            if (card != null && card.IsRecording && !card.IsTimeShifting) // this is a recording
            {
                Log.Debug("Timeshifting for user {0} is a recording, stopping recording instead!", userName);
                return StopRecording(card.RecordingScheduleId);
            }

            Log.Debug("Canceling timeshifting for user {0}", userName);
            return _tvControl.StopTimeShifting(ref currentUser);
        }
        #endregion

        #region Recordings
        public WebIntResult GetRecordingCount(string filter = null)
        {
            return Recording.ListAll().Select(rec => rec.ToWebRecording()).Filter(filter).Count();
        }

        public IList<WebRecordingBasic> GetRecordings(string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return Recording.ListAll().Select(rec => rec.ToWebRecording()).Filter(filter).SortRecordingList(sort, order).ToList();
        }

        public IList<WebRecordingBasic> GetRecordingsByRange(int start, int end, string filter = null, WebSortField? sort = WebSortField.Title, WebSortOrder? order = WebSortOrder.Asc)
        {
            return Recording.ListAll().Select(rec => rec.ToWebRecording()).Filter(filter).SortRecordingList(sort, order).TakeRange(start, end).ToList();
        }

        public WebRecordingBasic GetRecordingById(int id)
        {
            return Recording.Retrieve(id).ToWebRecording();
        }

        public WebBoolResult DeleteRecording(int id)
        {
            Log.Info("Deleting recording {0}, as requested by client {1}", id, WCFUtil.GetClientIPAddress());
            try
            {
                bool retVal = _tvControl.DeleteRecording(id);
                if (!retVal)
                    Log.Warn("Failed to delete recording, TV Server returned false");
                return retVal;
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to delete recoding", ex);
                return false;
            }
        }

        public WebRecordingFileInfo GetRecordingFileInfo(int id)
        {
            string filename = String.Empty;
            try
            {
                filename = GetRecordingById(id).FileName;

                bool tryImpersonation = false;
                try
                {
                    var fileInfo = new WebRecordingFileInfo(new FileInfo(filename));
                    if (fileInfo != null && fileInfo.Exists)
                        return fileInfo;

                    tryImpersonation = PathUtil.MightBeOnNetworkDrive(filename);
                }
                catch (UnauthorizedAccessException)
                {
                    tryImpersonation = true;
                }

                if (tryImpersonation && Configuration.Services.NetworkImpersonation.IsEnabled())
                {
                    using (var context = NetworkContextFactory.CreateImpersonationContext())
                    {
                        var ret = new WebRecordingFileInfo(context.RewritePath(filename));
                        ret.IsLocalFile = true;
                        ret.OnNetworkDrive = true;
                        return ret;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Log.Info("Failed to load fileinfo for recording {0} because it does not exists at path {1}", id, filename);
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to load fileinfo for recording", ex);
            }

            return new WebRecordingFileInfo();
        }

        public Stream ReadRecordingFile(int id)
        {
            try
            {
                WebRecordingFileInfo info = GetRecordingFileInfo(id);

                // read it as a regular file
                if (info.Exists && info.IsLocalFile && !info.OnNetworkDrive && File.Exists(info.Path))
                    return new FileStream(info.Path, FileMode.Open, FileAccess.Read);

                // try to load it from a network drive
                if (info.Exists && info.IsLocalFile && info.OnNetworkDrive)
                {
                    using (var context = NetworkContextFactory.Create())
                        return new FileStream(context.RewritePath(info.Path), FileMode.Open, FileAccess.Read);
                }

                // failed
                Log.Warn("No method to read file for recording {0} with path {1}", id, info.Path);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.NotFound);
                return Stream.Null;
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to read file for recording {0}", id), ex);
                WCFUtil.SetResponseCode(System.Net.HttpStatusCode.InternalServerError);
                return Stream.Null;
            }
        }
        #endregion

        #region EPG
        public IList<WebProgramBasic> GetProgramsBasicForChannel(int channelId, DateTime startTime, DateTime endTime, string filter = null)
        {
            using (var cache = WebProgramExtensionMethods.CacheSchedules())
            {
                return _tvBusiness.GetPrograms(Channel.Retrieve(channelId), startTime, endTime).Select(p => p.ToWebProgramBasic()).Filter(filter).ToList();
            }
        }

        public IList<WebProgramDetailed> GetProgramsDetailedForChannel(int channelId, DateTime startTime, DateTime endTime, string filter = null)
        {
            using (var cache = WebProgramExtensionMethods.CacheSchedules())
            {
                return _tvBusiness.GetPrograms(Channel.Retrieve(channelId), startTime, endTime).Select(p => p.ToWebProgramDetailed()).Filter(filter).ToList();
            }
        }

        public IList<WebChannelPrograms<WebProgramBasic>> GetProgramsBasicForGroup(int groupId, DateTime startTime, DateTime endTime, string filter = null)
        {
            using (var cache = WebProgramExtensionMethods.CacheSchedules())
            {
                return _tvBusiness.GetTVGuideChannelsForGroup(groupId)
                    .Select(ch => new WebChannelPrograms<WebProgramBasic>()
                    {
                        ChannelId = ch.IdChannel,
                        Programs = _tvBusiness.GetPrograms(ch, startTime, endTime).Select(p => p.ToWebProgramBasic()).Filter(filter).ToList()
                    })
                    .ToList();
            }
        }

        public IList<WebChannelPrograms<WebProgramDetailed>> GetProgramsDetailedForGroup(int groupId, DateTime startTime, DateTime endTime, string filter = null)
        {
            using (var cache = WebProgramExtensionMethods.CacheSchedules())
            {
                return _tvBusiness.GetTVGuideChannelsForGroup(groupId)
                    .Select(ch => new WebChannelPrograms<WebProgramDetailed>()
                    {
                        ChannelId = ch.IdChannel,
                        Programs = _tvBusiness.GetPrograms(ch, startTime, endTime).Select(p => p.ToWebProgramDetailed()).Filter(filter).ToList()
                    })
                    .ToList();
            }
        }

        public WebProgramDetailed GetCurrentProgramOnChannel(int channelId)
        {
            return Channel.Retrieve(channelId).CurrentProgram.ToWebProgramDetailed();
        }

        public WebProgramDetailed GetNextProgramOnChannel(int channelId)
        {
            return Channel.Retrieve(channelId).NextProgram.ToWebProgramDetailed();
        }

        public WebIntResult SearchProgramsCount(string searchTerm)
        {
            return _tvBusiness.SearchPrograms(searchTerm).Count;
        }

        public IList<WebProgramDetailed> SearchProgramsDetailed(string searchTerm, string filter = null)
        {
            return _tvBusiness.SearchPrograms(searchTerm).Select(p => p.ToWebProgramDetailed()).Filter(filter).ToList();
        }

        public IList<WebProgramDetailed> SearchProgramsDetailedByRange(string searchTerm, int start, int end, string filter = null)
        {
            return _tvBusiness.SearchPrograms(searchTerm).Select(p => p.ToWebProgramDetailed()).Filter(filter).TakeRange(start, end).ToList();
        }

        public IList<WebProgramBasic> SearchProgramsBasic(string searchTerm, string filter = null)
        {
            return _tvBusiness.SearchPrograms(searchTerm).Select(p => p.ToWebProgramBasic()).Filter(filter).ToList();
        }

        public IList<WebProgramBasic> SearchProgramsBasicByRange(string searchTerm, int start, int end, string filter = null)
        {
            return _tvBusiness.SearchPrograms(searchTerm).Select(p => p.ToWebProgramBasic()).Filter(filter).TakeRange(start, end).ToList();
        }


        public WebProgramBasic GetProgramBasicById(int programId)
        {
            return Program.Retrieve(programId).ToWebProgramBasic();
        }

        public IList<WebProgramBasic> GetNowNextWebProgramBasicForChannel(int channelId, string filter = null)
        {
            return Channel.Retrieve(channelId).ToListWebProgramBasicNowNext().Filter(filter).ToList();
        }

        public IList<WebProgramDetailed> GetNowNextWebProgramDetailedForChannel(int channelId, string filter = null)
        {
            return Channel.Retrieve(channelId).ToListWebProgramDetailedNowNext().Filter(filter).ToList();
        }

        public WebProgramDetailed GetProgramDetailedById(int programId)
        {
            return Program.Retrieve(programId).ToWebProgramDetailed();
        }

        public WebBoolResult GetProgramIsScheduledOnChannel(int channelId, int programId)
        {
            Program program = Program.Retrieve(programId);
            Channel channel = Channel.Retrieve(channelId);

            return channel.ReferringSchedule().Any(schedule => schedule.IsRecordingProgram(program, false));
        }

        public WebBoolResult GetProgramIsScheduled(int programId)
        {
            Program p = Program.Retrieve(programId);
            return Schedule.ListAll().Where(schedule => schedule.IsRecordingProgram(p, true)).Count() > 0;
        }
        #endregion
    }
}
