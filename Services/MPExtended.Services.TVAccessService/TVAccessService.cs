#region Copyright (C) 2010-2011 TV4Home, 2011 MPExtended
// Copyright (C) 2010-2011 TV4Home, http://tv4home.codeplex.com/
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using Gentle.Provider.MySQL;
using Gentle.Provider.SQLServer;
using MPExtended.Libraries.General;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.TVAccessService.Interfaces;
using TvControl;
using TvDatabase;

namespace MPExtended.Services.TVAccessService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class TVAccessService : ITVAccessService
    {
        private const int API_VERSION = 2;

        private TvBusinessLayer _tvBusiness;
        private IController _tvControl;
        private Dictionary<string, IUser> _tvUsers;

        #region Service
        public TVAccessService()
        {
            _tvUsers = new Dictionary<string, IUser>();
            _tvBusiness = new TvBusinessLayer();
            WcfUsernameValidator.Init();

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

        private IUser GetUserByUserName(string userName, bool create = false)
        {
            if (userName == null)
            {
                Log.Warn("Used user with null name");
                return null;
            }
            else if (!_tvUsers.ContainsKey(userName) && !create)
            {
                return null;
            }
            else if (!_tvUsers.ContainsKey(userName) && create)
            {
                _tvUsers.Add(userName, new User(userName, false));
            }

            return _tvUsers[userName];
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
        public bool TestConnectionToTVService()
        {
            return RemoteControl.IsConnected;
        }

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

        public string ReadSettingFromDatabase(string tagName)
        {
            return _tvBusiness.GetSetting(tagName, "").Value;
        }

        public void WriteSettingToDatabase(string tagName, string value)
        {
            Setting setting = _tvBusiness.GetSetting(tagName, "");
            setting.Value = value;
            setting.Persist();
        }
        #endregion

        #region Schedules
        public void StartRecordingManual(string userName, int channelId, string title)
        {
            Log.Debug("Start recording manual on channel " + channelId + ", userName: " + userName);
            AddSchedule(channelId, title, DateTime.Now, DateTime.Now.AddDays(1), 0);
        }

        public void AddSchedule(int channelId, string title, DateTime startTime, DateTime endTime, int scheduleType)
        {
            AddScheduleDetailed(channelId, title, startTime, endTime, scheduleType, -1, -1, "", -1);
        }

        public void AddScheduleDetailed(int channelId, string title, DateTime startTime, DateTime endTime, int scheduleType, int preRecordInterval, int postRecordInterval, string directory, int priority)
        {
            Log.Debug("Adding schedulw on channel {0} for {1}, {2}-{3}, type {4}", channelId, title, startTime, endTime, scheduleType);
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
        }

        public IList<WebScheduleBasic> GetSchedules()
        {
            return Schedule.ListAll().Select(sch => sch.ToWebSchedule()).ToList();
        }

        public WebScheduleBasic GetScheduleById(int scheduleId)
        {
            return Schedule.Retrieve(scheduleId).ToWebSchedule();
        }

        public void CancelSchedule(int programId)
        {
            Program p = Program.Retrieve(programId);

            foreach (Schedule schedule in Schedule.ListAll().Where(schedule => schedule.IsRecordingProgram(p, true)))
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
        }

        public void DeleteSchedule(int scheduleId)
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
        }
        #endregion

        #region Channels
        #region TV specific
        public IList<WebChannelGroup> GetGroups()
        {
            return ChannelGroup.ListAll().Select(chg => chg.ToWebChannelGroup()).ToList();
        }

        public WebChannelGroup GetGroupById(int groupId)
        {
            return ChannelGroup.Retrieve(groupId).ToWebChannelGroup();
        }

        public int GetChannelCount(int groupId)
        {
            return _tvBusiness.GetTVGuideChannelsForGroup(groupId).Count;
        }

        public IList<WebChannelBasic> GetChannelsBasic(int groupId)
        {
            return _tvBusiness.GetTVGuideChannelsForGroup(groupId).Select(ch => ch.ToWebChannelBasic()).ToList();
        }

        public IList<WebChannelBasic> GetChannelsBasicByRange(int groupId, int startIndex, int count)
        {
            try
            {
                return _tvBusiness.GetTVGuideChannelsForGroup(groupId).GetRange(startIndex, count).Select(ch => ch.ToWebChannelBasic()).ToList();
            }
            catch (ArgumentOutOfRangeException)
            {
                Log.Warn("Invalid indexes passed to GetChannelsBasicByRange: groupId={0} startIndex={1} count={2}", groupId, startIndex, count);
                return null;
            }
        }

        public IList<WebChannelDetailed> GetChannelsDetailed(int groupId)
        {
            return _tvBusiness.GetTVGuideChannelsForGroup(groupId).Select(ch => ch.ToWebChannelDetailed()).ToList();
        }

        public IList<WebChannelDetailed> GetChannelsDetailedByRange(int groupId, int startIndex, int count)
        {
            try
            {
                return _tvBusiness.GetTVGuideChannelsForGroup(groupId).GetRange(startIndex, count).Select(ch => ch.ToWebChannelDetailed()).ToList();
            }
            catch (ArgumentOutOfRangeException)
            {
                Log.Warn("Invalid indexes passed to GetChannelsDetailedByRange: groupId={0} startIndex={1} count={2}", groupId, startIndex, count);
                return null;
            }
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
        public IList<WebChannelGroup> GetRadioGroups()
        {
            return RadioChannelGroup.ListAll().Select(chg => chg.ToWebChannelGroup()).ToList();
        }

        public WebChannelGroup GetRadioGroupById(int groupId)
        {
            return RadioChannelGroup.Retrieve(groupId).ToWebChannelGroup();
        }

        public int GetRadioChannelCount(int groupId)
        {
            return _tvBusiness.GetRadioGuideChannelsForGroup(groupId).Count;
        }

        public IList<WebChannelBasic> GetRadioChannelsBasic(int groupId)
        {
            return _tvBusiness.GetRadioGuideChannelsForGroup(groupId).Select(ch => ch.ToWebChannelBasic()).ToList();
        }

        public IList<WebChannelBasic> GetRadioChannelsBasicByRange(int groupId, int startIndex, int count)
        {
            try
            {
                return _tvBusiness.GetRadioGuideChannelsForGroup(groupId).GetRange(startIndex, count).Select(ch => ch.ToWebChannelBasic()).ToList();
            }
            catch (ArgumentOutOfRangeException)
            {
                Log.Warn("Invalid indexes passed to GetRadioChannelsBasicByRange: groupId={0} startIndex={1} count={2}", groupId, startIndex, count);
                return null;
            }
        }

        public IList<WebChannelDetailed> GetRadioChannelsDetailed(int groupId)
        {
            return _tvBusiness.GetRadioGuideChannelsForGroup(groupId).Select(ch => ch.ToWebChannelDetailed()).ToList();
        }

        public IList<WebChannelDetailed> GetRadioChannelsDetailedByRange(int groupId, int startIndex, int count)
        {
            try
            {
                return _tvBusiness.GetRadioGuideChannelsForGroup(groupId).GetRange(startIndex, count).Select(ch => ch.ToWebChannelDetailed()).ToList();
            }
            catch (ArgumentOutOfRangeException)
            {
                Log.Warn("Invalid indexes passed to GetRadioChannelsDetailedByRange: groupId={0} startIndex={1} count={2}", groupId, startIndex, count);
                return null;
            }
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
            ChannelState state = _tvControl.GetChannelState(channelId, GetUserByUserName(userName, true));
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
            if (String.IsNullOrEmpty(userName))
            {
                Log.Error("Called SwitchTVServerToChannel with empty userName");
                throw new ArgumentNullException("userName");
            }

            Log.Debug("Starting timeshifiting with username {0} on channel id {1}", userName, channelId);
            IUser currentUser = GetUserByUserName(userName, true);

            VirtualCard tvCard;
            Log.Debug("Starting timeshifting");
            TvResult result = _tvControl.StartTimeShifting(ref currentUser, channelId, out tvCard);
            Log.Trace("Tried to start timeshifting, result {0}", result);

            if (result != TvResult.Succeeded)
            {
                // TODO: should we retry?
                Log.Error("Starting timeshifting failed with result {0}", result);
                throw new Exception("Failed to start tv stream: " + result);
            }

            Log.Debug("Timeshifting succeeded");
            if (tvCard == null)
            {
                Log.Error("Couldn't get virtual card");
                throw new Exception("Couldn't get virtual card");
            }

            // set card id and channel id of user, required for heartbeat and stopping of timeshifting
            currentUser.CardId = tvCard.Id;
            currentUser.IdChannel = channelId;
            _tvUsers[userName] = currentUser;

            return tvCard;
        }

        public void SendHeartbeat(string userName)
        {
            IUser currentUser = GetUserByUserName(userName);
            if (currentUser == null)
            {
                Log.Error("Tried to send heartbeat for invalid user {0}", userName);
                throw new ArgumentException("Invalid username");
            }

            _tvControl.HeartBeat(currentUser);
        }

        public bool CancelCurrentTimeShifting(string userName)
        {
            IUser currentUser = GetUserByUserName(userName);
            if (currentUser == null)
            {
                Log.Error("Tried to cancel timeshifting for invalid user {0}", userName);
                throw new ArgumentException("Invalid username");
            }

            return _tvControl.StopTimeShifting(ref currentUser);
        }
        #endregion

        #region Recordings
        public IList<WebRecordingBasic> GetRecordings()
        {
            return Recording.ListAll().Select(rec => rec.ToWebRecording()).ToList();
        }

        public WebRecordingBasic GetRecordingById(int recordingId)
        {
            return Recording.Retrieve(recordingId).ToWebRecording();
        }

        public WebFileInfo GetFileInfo(int recordingId)
        {
            return new WebFileInfo(GetRecordingById(recordingId).FileName);
        }
        #endregion

        #region EPG
        public IList<WebProgramBasic> GetProgramsBasicForChannel(int channelId, DateTime startTime, DateTime endTime)
        {
            return _tvBusiness.GetPrograms(Channel.Retrieve(channelId), startTime, endTime).Select(p => p.ToWebProgramBasic()).ToList();
        }

        public IList<WebProgramDetailed> GetProgramsDetailedForChannel(int channelId, DateTime startTime, DateTime endTime)
        {
            return _tvBusiness.GetPrograms(Channel.Retrieve(channelId), startTime, endTime).Select(p => p.ToWebProgramDetailed()).ToList();
        }

        public WebProgramDetailed GetCurrentProgramOnChannel(int channelId)
        {
            return Channel.Retrieve(channelId).CurrentProgram.ToWebProgramDetailed();
        }

        public WebProgramDetailed GetNextProgramOnChannel(int channelId)
        {
            return Channel.Retrieve(channelId).NextProgram.ToWebProgramDetailed();
        }

        public IList<WebProgramDetailed> SearchProgramsDetailed(string searchTerm)
        {
            return _tvBusiness.SearchPrograms(searchTerm).Select(p => p.ToWebProgramDetailed()).ToList();
        }

        public IList<WebProgramBasic> SearchProgramsBasic(string searchTerm)
        {
            return _tvBusiness.SearchPrograms(searchTerm).Select(p => p.ToWebProgramBasic()).ToList();
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

        public bool GetProgramIsScheduledOnChannel(int channelId, int programId)
        {
            Program program = Program.Retrieve(programId);
            Channel channel = Channel.Retrieve(channelId);

            return channel.ReferringSchedule().Any(schedule => schedule.IsRecordingProgram(program, false));
        }

        public bool GetProgramIsScheduled(int programId)
        {
            Program p = Program.Retrieve(programId);
            return Schedule.ListAll().Where(schedule => schedule.IsRecordingProgram(p, true)).Count() > 0;
        }
        #endregion
    }
}
