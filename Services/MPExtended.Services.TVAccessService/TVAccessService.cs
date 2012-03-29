#region Copyright (C) 2011-2012 MPExtended, 2010-2011 TV4Home
// Copyright (C) 2010-2011 TV4Home, http://tv4home.codeplex.com/
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using Gentle.Provider.MySQL;
using Gentle.Provider.SQLServer;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.TVAccessService.Interfaces;
using TvControl;
using TvDatabase;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.TVAccessService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class TVAccessService : ITVAccessService
    {
        private const int API_VERSION = 2;

        private TvBusinessLayer _tvBusiness;
        private IController _tvControl;

        #region Service
        public TVAccessService()
        {
            _tvBusiness = new TvBusinessLayer();

            // try to initialize Gentle and TVE API
            InitializeGentleAndTVE();
        }

        private void InitializeGentleAndTVE()
        {
            try
            {
                // read Gentle configuration from CommonAppData
                string gentleConfigFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Team MediaPortal", "MediaPortal TV Server", "Gentle.config"
                );

                if (!File.Exists(gentleConfigFile))
                    throw new FileNotFoundException("The Gentle configuration file couldn't be found. This occurs when TV Server is not installed.", gentleConfigFile);

                Gentle.Common.Configurator.AddFileHandler(gentleConfigFile);

                // connect to tv server via TVE API
                RemoteControl.Clear();
                RemoteControl.HostName = TvDatabase.Server.ListAll().First(server => server.IsMaster).HostName;

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
                .Where(c => c.Enabled)
                .SelectMany(c => _tvControl.GetUsersForCard(c.IdCard))
                .Where(u => u.Name == userName)
                .FirstOrDefault();
        }

        public WebTVServiceDescription GetServiceDescription()
        {
            return new WebTVServiceDescription()
            {
                HasConnectionToTVServer = RemoteControl.IsConnected,
                ApiVersion = API_VERSION,
                ServiceVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion
            };
        }
        #endregion

        #region TV Server
        public WebResult TestConnectionToTVService()
        {
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

        public string ReadSettingFromDatabase(string tagName)
        {
            return _tvBusiness.GetSetting(tagName, "").Value;
        }

        public WebResult WriteSettingToDatabase(string tagName, string value)
        {
            Setting setting = _tvBusiness.GetSetting(tagName, "");
            setting.Value = value;
            setting.Persist();
            return true;
        }


        /// <summary>
        /// Return external media info for recording
        /// </summary>
        /// <param name="type">Type of item</param>
        /// <param name="id">Id of recording</param>
        /// <returns>A dictionary object that can be sent to e.g. WifiRemote</returns>
        public SerializableDictionary<string> GetExternalMediaInfo(WebTvMediaType? type, string id)
        {
            return GetExternalMediaInfoForMpTvServer(type, id);
        }

        private SerializableDictionary<string> GetExternalMediaInfoForMpTvServer(WebTvMediaType? type, string id)
        {
            if (type == WebTvMediaType.Recording)
            {
                return new SerializableDictionary<string>()
                {
                    { "Type", "mp recording" },
                    { "Id", id }
                };
            }
            else if (type == WebTvMediaType.TV)
            {
                return new SerializableDictionary<string>()
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
                        ChannelName = Channel.Retrieve(rec.IdChannel).DisplayName
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
                        ChannelName = Channel.Retrieve(schedule.IdChannel).DisplayName
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
                        ChannelName = Channel.Retrieve(program.IdChannel).DisplayName
                    }));
            }

            return result.OrderByDescending(x => x.Score).ToList();
        }

        public IList<WebTVSearchResult> SearchResultsByRange(string text, int start, int end, WebTVSearchResultType? type = null)
        {
            return Search(text, type).TakeRange(start, end).ToList();
        }
        #endregion

        #region Cards
        public IList<WebCard> GetCards()
        {
            return Card.ListAll().Select(c => c.ToWebCard()).ToList();
        }

        public IList<WebVirtualCard> GetActiveCards()
        {
            return GetTimeshiftingOrRecordingVirtualCards().Select(card => card.ToWebVirtualCard()).ToList();
        }

        public IList<WebUser> GetActiveUsers()
        {
            return GetTimeshiftingOrRecordingVirtualCards().Select(card => card.User.ToWebUser()).ToList();
        }

        private IEnumerable<VirtualCard> GetTimeshiftingOrRecordingVirtualCards()
        {
            return Card.ListAll()
                .Where(card => RemoteControl.Instance.CardPresent(card.IdCard))
                .Select(card => RemoteControl.Instance.GetUsersForCard(card.IdCard))
                .Where(users => users != null)
                .SelectMany(user => user)
                .Select(user => new VirtualCard(user, RemoteControl.HostName))
                .Where(tvCard => tvCard.IsTimeShifting || !tvCard.IsRecording);
        }

        public IList<WebRtspClient> GetStreamingClients()
        {
            return _tvControl.StreamingClients.Select(cl => cl.ToWebRtspClient()).ToList();
        }

        public IList<WebDiskSpaceInformation> GetAllRecordingDiskInformation()
        {
            return GetCards().Select(x => x.RecordingFolder).Distinct()
                .Select(x => DiskSpaceInformation.GetSpaceInformation(x))
                .ToList();
        }

        public WebDiskSpaceInformation GetRecordingDiskInformationForCard(int id)
        {
            string folder = Card.Retrieve(id).ToWebCard().RecordingFolder;
            return DiskSpaceInformation.GetSpaceInformation(folder);
        }
        #endregion

        #region Schedules
        public WebResult StartRecordingManual(string userName, int channelId, string title)
        {
            Log.Debug("Start recording manual on channel " + channelId + ", userName: " + userName);
            return AddSchedule(channelId, title, DateTime.Now, DateTime.Now.AddDays(1), 0);
        }

        public WebResult AddSchedule(int channelId, string title, DateTime startTime, DateTime endTime, WebScheduleType scheduleType)
        {
            return AddScheduleDetailed(channelId, title, startTime, endTime, scheduleType, -1, -1, "", -1);
        }

        public WebResult AddScheduleDetailed(int channelId, string title, DateTime startTime, DateTime endTime, WebScheduleType scheduleType, int preRecordInterval, int postRecordInterval, string directory, int priority)
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

        public WebCount GetScheduleCount()
        {
            return new WebCount() { Count = Schedule.ListAll().Count };
        }

        public IList<WebScheduleBasic> GetSchedules(SortField? sort = SortField.Name, SortOrder? order = SortOrder.Asc)
        {
            return Schedule.ListAll().Select(s => s.ToWebSchedule()).SortScheduleList(sort, order).ToList();
        }

        public IList<WebScheduleBasic> GetSchedulesByRange(int start, int end, SortField? sort = SortField.Name, SortOrder? order = SortOrder.Asc)
        {
            return Schedule.ListAll().Select(s => s.ToWebSchedule()).SortScheduleList(sort, order).TakeRange(start, end).ToList();
        }

        public WebScheduleBasic GetScheduleById(int scheduleId)
        {
            return Schedule.Retrieve(scheduleId).ToWebSchedule();
        }

        public WebResult CancelSchedule(int programId)
        {
            try
            {
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

        public WebResult DeleteSchedule(int scheduleId)
        {
            // TODO: the workflow in this method doesn't make any sense at all
            try
            {
                Schedule schedule = Schedule.Retrieve(scheduleId);

                // first cancel all of the episodes of this program for this schedule
                foreach (Program program in Program.ListAll().Where(program => program.Title == schedule.ProgramName))
                {
                    if (schedule.IsRecordingProgram(program, true))
                    {
                        CanceledSchedule canceledSchedule = new CanceledSchedule(schedule.IdSchedule, program.IdChannel, program.StartTime);
                        canceledSchedule.Persist();
                        _tvControl.OnNewSchedule();
                    }
                }

                // now remove existing CanceledSchedule for this schedule
                foreach (CanceledSchedule canceled in CanceledSchedule.ListAll().Where(canceled => canceled.IdSchedule == schedule.IdSchedule))
                {
                    canceled.Remove();
                }
                schedule.Remove();
                _tvControl.OnNewSchedule();
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to delete schedule {0}", scheduleId), ex);
                return false;
            }
        }
        #endregion

        #region Channels
        #region TV specific
        public WebCount GetGroupCount()
        {
            return new WebCount() { Count = ChannelGroup.ListAll().Count };
        }

        public IList<WebChannelGroup> GetGroups(SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return ChannelGroup.ListAll().Select(chg => chg.ToWebChannelGroup()).SortGroupList(sort, order).ToList();
        }

        public IList<WebChannelGroup> GetGroupsByRange(int start, int end, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return ChannelGroup.ListAll().Select(chg => chg.ToWebChannelGroup()).SortGroupList(sort, order).TakeRange(start, end).ToList();
        }

        public WebChannelGroup GetGroupById(int groupId)
        {
            return ChannelGroup.Retrieve(groupId).ToWebChannelGroup();
        }

        public WebCount GetChannelCount(int groupId)
        {
            return new WebCount() { Count = _tvBusiness.GetTVGuideChannelsForGroup(groupId).Count };
        }

        public IList<WebChannelBasic> GetAllChannelsBasic(SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return Channel.ListAll().Where(ch => ch.IsTv).OrderBy(ch => sort == SortField.User ? ch.SortOrder : 1, order)
                .Select(ch => ch.ToWebChannelBasic()).SortChannelList(sort, order).ToList();
        }

        public IList<WebChannelBasic> GetChannelsBasic(int groupId, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return _tvBusiness.GetTVGuideChannelsForGroup(groupId).Select(ch => ch.ToWebChannelBasic()).SortChannelList(sort, order).ToList();
        }

        public IList<WebChannelBasic> GetChannelsBasicByRange(int groupId, int start, int end, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return _tvBusiness.GetTVGuideChannelsForGroup(groupId).Select(ch => ch.ToWebChannelBasic()).SortChannelList(sort, order).TakeRange(start, end).ToList();
        }

        public IList<WebChannelDetailed> GetAllChannelsDetailed(SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return Channel.ListAll().Where(ch => ch.IsTv).OrderBy(ch => sort == SortField.User ? ch.SortOrder : 1, order)
                .Select(ch => ch.ToWebChannelDetailed()).SortChannelList(sort, order).ToList();
        }

        public IList<WebChannelDetailed> GetChannelsDetailed(int groupId, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return _tvBusiness.GetTVGuideChannelsForGroup(groupId).Select(ch => ch.ToWebChannelDetailed()).SortChannelList(sort, order).ToList();
        }

        public IList<WebChannelDetailed> GetChannelsDetailedByRange(int groupId, int start, int end, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return _tvBusiness.GetTVGuideChannelsForGroup(groupId).Select(ch => ch.ToWebChannelDetailed()).SortChannelList(sort, order).TakeRange(start, end).ToList();
        }

        public IList<WebChannelState> GetAllChannelStatesForGroup(int groupId, string userName)
        {
            IList<WebChannelBasic> list = GetChannelsBasic(groupId);
            IList<WebChannelState> webChannelStates = new List<WebChannelState>();
            foreach (WebChannelBasic entry in list)
            {
                webChannelStates.Add(GetChannelState(entry.Id, userName));
            }

            return webChannelStates;

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
        public WebCount GetRadioGroupCount()
        {
            return new WebCount() { Count = RadioChannelGroup.ListAll().Count };
        }

        public IList<WebChannelGroup> GetRadioGroups(SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return RadioChannelGroup.ListAll().Select(chg => chg.ToWebChannelGroup()).SortGroupList(sort, order).ToList();
        }

        public IList<WebChannelGroup> GetRadioGroupsByRange(int start, int end, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return RadioChannelGroup.ListAll().Select(chg => chg.ToWebChannelGroup()).SortGroupList(sort, order).TakeRange(start, end).ToList();
        }

        public WebChannelGroup GetRadioGroupById(int groupId)
        {
            return RadioChannelGroup.Retrieve(groupId).ToWebChannelGroup();
        }

        public WebCount GetRadioChannelCount(int groupId)
        {
            return new WebCount() { Count = _tvBusiness.GetRadioGuideChannelsForGroup(groupId).Count };
        }

        public IList<WebChannelBasic> GetAllRadioChannelsBasic(SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return Channel.ListAll().Where(ch => ch.IsRadio).OrderBy(ch => sort == SortField.User ? ch.SortOrder : 1, order)
                .Select(ch => ch.ToWebChannelBasic()).SortChannelList(sort, order).ToList();
        }

        public IList<WebChannelBasic> GetRadioChannelsBasic(int groupId, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return _tvBusiness.GetRadioGuideChannelsForGroup(groupId).Select(ch => ch.ToWebChannelBasic()).SortChannelList(sort, order).ToList();
        }

        public IList<WebChannelBasic> GetRadioChannelsBasicByRange(int groupId, int start, int end, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return _tvBusiness.GetRadioGuideChannelsForGroup(groupId).Select(ch => ch.ToWebChannelBasic()).SortChannelList(sort, order).TakeRange(start, end).ToList();
        }

        public IList<WebChannelDetailed> GetAllRadioChannelsDetailed(SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return Channel.ListAll().Where(ch => ch.IsRadio).OrderBy(ch => sort == SortField.User ? ch.SortOrder : 1, order)
                .Select(ch => ch.ToWebChannelDetailed()).SortChannelList(sort, order).ToList();
        }

        public IList<WebChannelDetailed> GetRadioChannelsDetailed(int groupId, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return _tvBusiness.GetRadioGuideChannelsForGroup(groupId).Select(ch => ch.ToWebChannelDetailed()).SortChannelList(sort, order).ToList();
        }

        public IList<WebChannelDetailed> GetRadioChannelsDetailedByRange(int groupId, int start, int end, SortField? sort = SortField.User, SortOrder? order = SortOrder.Asc)
        {
            return _tvBusiness.GetRadioGuideChannelsForGroup(groupId).Select(ch => ch.ToWebChannelDetailed()).SortChannelList(sort, order).TakeRange(start, end).ToList();
        }

        public IList<WebChannelState> GetAllRadioChannelStatesForGroup(int groupId, string userName)
        {
            IList<WebChannelBasic> list = GetRadioChannelsBasic(groupId);
            IList<WebChannelState> webChannelStates = new List<WebChannelState>();
            foreach (WebChannelBasic entry in list)
            {
                webChannelStates.Add(GetChannelState(entry.Id, userName));
            }

            return webChannelStates;
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

        public string SwitchTVServerToChannelAndGetStreamingUrl(string userName, int channelId)
        {
            return SwitchTVServerToChannel(userName, channelId).RTSPUrl;
        }

        public string SwitchTVServerToChannelAndGetTimeshiftFilename(string userName, int channelId)
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

        public WebResult SendHeartbeat(string userName)
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

        public WebResult CancelCurrentTimeShifting(string userName)
        {
            IUser currentUser = GetUserByUserName(userName);
            if (currentUser == null)
            {
                Log.Warn("Tried to cancel timeshifting for invalid user {0}", userName);
                return false;
            }
            Log.Debug("Canceling timeshifting for user {0}", userName);

            return _tvControl.StopTimeShifting(ref currentUser);
        }
        #endregion

        #region Recordings
        public WebCount GetRecordingCount()
        {
            return new WebCount() { Count = Recording.ListAll().Count };
        }

        public IList<WebRecordingBasic> GetRecordings(SortField? sort = SortField.Name, SortOrder? order = SortOrder.Asc)
        {
            return Recording.ListAll().Select(rec => rec.ToWebRecording()).SortRecordingList(sort, order).ToList();
        }

        public IList<WebRecordingBasic> GetRecordingsByRange(int start, int end, SortField? sort = SortField.Name, SortOrder? order = SortOrder.Asc)
        {
            return Recording.ListAll().Select(rec => rec.ToWebRecording()).SortRecordingList(sort, order).TakeRange(start, end).ToList();
        }

        public WebRecordingBasic GetRecordingById(int id)
        {
            return Recording.Retrieve(id).ToWebRecording();
        }

        public WebRecordingFileInfo GetRecordingFileInfo(int id)
        {
            try
            {
                string filename = GetRecordingById(id).FileName;
                try
                {
                    return new WebRecordingFileInfo(new FileInfo(filename));
                }
                catch (UnauthorizedAccessException)
                {
                    // access denied, try impersonation when on a network share
                    if (new Uri(filename).IsUnc)
                    {
                        using (NetworkShareImpersonator impersonation = new NetworkShareImpersonator())
                        {
                            var ret = new WebRecordingFileInfo(filename);
                            ret.IsLocalFile = Configuration.Services.NetworkImpersonation.ReadInStreamingService;
                            ret.OnNetworkDrive = true;
                            return ret;
                        }
                    }
                }
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

                // return it as a simple file
                if (info.IsLocalFile && File.Exists(info.Path))
                {
                    return new FileStream(info.Path, FileMode.Open, FileAccess.Read);
                }

                // try to load it from a network drive
                if (info.OnNetworkDrive && info.Exists)
                {
                    using (NetworkShareImpersonator impersonation = new NetworkShareImpersonator())
                    {
                        return new FileStream(info.Path, FileMode.Open, FileAccess.Read);
                    }
                }

                // failed
                Log.Warn("No method to read file for recording {0}", id);
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
        public IList<WebProgramBasic> GetProgramsBasicForChannel(int channelId, DateTime startTime, DateTime endTime)
        {
            using (var cache = WebProgramExtensionMethods.CacheSchedules())
            {
                return _tvBusiness.GetPrograms(Channel.Retrieve(channelId), startTime, endTime).Select(p => p.ToWebProgramBasic()).ToList();
            }
        }

        public IList<WebProgramDetailed> GetProgramsDetailedForChannel(int channelId, DateTime startTime, DateTime endTime)
        {
            using (var cache = WebProgramExtensionMethods.CacheSchedules())
            {
                return _tvBusiness.GetPrograms(Channel.Retrieve(channelId), startTime, endTime).Select(p => p.ToWebProgramDetailed()).ToList();
            }
        }

        public IList<WebChannelPrograms<WebProgramBasic>> GetProgramsBasicForGroup(int channelGroup, DateTime startTime, DateTime endTime)
        {
            using (var cache = WebProgramExtensionMethods.CacheSchedules())
            {
                return _tvBusiness.GetTVGuideChannelsForGroup(channelGroup)
                    .Select(ch => new WebChannelPrograms<WebProgramBasic>()
                    {
                        IdChannel = ch.IdChannel,
                        Programs = _tvBusiness.GetPrograms(ch, startTime, endTime).Select(p => p.ToWebProgramBasic()).ToList()
                    })
                    .ToList();
            }
        }

        public IList<WebChannelPrograms<WebProgramDetailed>> GetProgramsDetailedForGroup(int channelGroup, DateTime startTime, DateTime endTime)
        {
            using (var cache = WebProgramExtensionMethods.CacheSchedules())
            {
                return _tvBusiness.GetTVGuideChannelsForGroup(channelGroup)
                    .Select(ch => new WebChannelPrograms<WebProgramDetailed>()
                    {
                        IdChannel = ch.IdChannel,
                        Programs = _tvBusiness.GetPrograms(ch, startTime, endTime).Select(p => p.ToWebProgramDetailed()).ToList()
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

        public WebCount SearchProgramsCount(string searchTerm)
        {
            return new WebCount() { Count = _tvBusiness.SearchPrograms(searchTerm).Count };
        }

        public IList<WebProgramDetailed> SearchProgramsDetailed(string searchTerm)
        {
            return _tvBusiness.SearchPrograms(searchTerm).Select(p => p.ToWebProgramDetailed()).ToList();
        }

        public IList<WebProgramDetailed> SearchProgramsDetailedByRange(string searchTerm, int start, int end)
        {
            return _tvBusiness.SearchPrograms(searchTerm).Select(p => p.ToWebProgramDetailed()).TakeRange(start, end).ToList();
        }

        public IList<WebProgramBasic> SearchProgramsBasic(string searchTerm)
        {
            return _tvBusiness.SearchPrograms(searchTerm).Select(p => p.ToWebProgramBasic()).ToList();
        }

        public IList<WebProgramBasic> SearchProgramsBasicByRange(string searchTerm, int start, int end)
        {
            return _tvBusiness.SearchPrograms(searchTerm).Select(p => p.ToWebProgramBasic()).TakeRange(start, end).ToList();
        }


        public WebProgramBasic GetProgramBasicById(int programId)
        {
            return Program.Retrieve(programId).ToWebProgramBasic();
        }

        public IList<WebProgramBasic> GetNowNextWebProgramBasicForChannel(int channelId)
        {
            return Channel.Retrieve(channelId).ToListWebProgramBasicNowNext();
        }

        public IList<WebProgramDetailed> GetNowNextWebProgramDetailedForChannel(int channelId)
        {
            return Channel.Retrieve(channelId).ToListWebProgramDetailedNowNext();
        }

        public WebProgramDetailed GetProgramDetailedById(int programId)
        {
            return Program.Retrieve(programId).ToWebProgramDetailed();
        }

        public WebResult GetProgramIsScheduledOnChannel(int channelId, int programId)
        {
            Program program = Program.Retrieve(programId);
            Channel channel = Channel.Retrieve(channelId);

            return channel.ReferringSchedule().Any(schedule => schedule.IsRecordingProgram(program, false));
        }

        public WebResult GetProgramIsScheduled(int programId)
        {
            Program p = Program.Retrieve(programId);
            return Schedule.ListAll().Where(schedule => schedule.IsRecordingProgram(p, true)).Count() > 0;
        }
        #endregion
    }
}
