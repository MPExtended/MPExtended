#region Copyright (C) 2010-2011 TV4Home, 2011 MPExtended
// Copyright (C) 2010-2011 TV4Home, http://tv4home.codeplex.com/
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
        #region Fields
        private const int API_VERSION = 2;

        private TvBusinessLayer _tvBusiness;
        private IController _tvControl;
        private Dictionary<string, User> _tvUsers;
        #endregion

        #region Constructor
        public TVAccessService()
        {
            _tvUsers = new Dictionary<string, User>();
            _tvBusiness = new TvBusinessLayer();
            WcfUsernameValidator.Init();

            // try to initialize Gentle and TVE API
            InitializeGentleAndTVE();
        }
        #endregion

        #region Public Methods
        public bool TestConnectionToTVService()
        {
            return RemoteControl.IsConnected;
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

        public void AddSchedule(int channelId, string title, DateTime startTime, DateTime endTime, int scheduleType)
        {
            AddScheduleDetailed(channelId, title, startTime, endTime, scheduleType, -1, -1, "", -1);

        }
        public WebChannelState GetChannelState(int channelId, string userName)
        {
            ChannelState state = _tvControl.GetChannelState(channelId, GetUserByUserName(userName));
            return ToWebChannelState(state);
        }


        public Dictionary<int, WebChannelState> GetAllChannelStatesForGroup(int groupId, string userName)
        {
            Dictionary<int, ChannelState> channelStates = _tvControl.GetAllChannelStatesForGroup(groupId, GetUserByUserName(userName));
            Dictionary<int, WebChannelState> webChannelStates = new Dictionary<int, WebChannelState>();
            if (channelStates != null && channelStates.Count > 0)
            {
                foreach (var entry in channelStates)
                {

                    webChannelStates.Add(entry.Key, ToWebChannelState(entry.Value));

                }
            }
            return webChannelStates;
        }

        public void AddScheduleDetailed(int channelId, string title, DateTime startTime, DateTime endTime, int scheduleType, int preRecordInterval, int postRecordInterval, string directory, int priority)
        {
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
                schedule.Priority = priority;

            schedule.Persist();
            _tvControl.OnNewSchedule();
        }

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

        public void SendHeartbeat(string userName)
        {
            if (String.IsNullOrEmpty(userName))
            {
                Log.Error("ArgumentNullException in SendHeartbeat");
                throw new ArgumentNullException("userName");
            }
            User currentUser = GetUserByUserName(userName);

            _tvControl.HeartBeat(currentUser);
        }

        public bool CancelCurrentTimeShifting(string userName)
        {
            if (String.IsNullOrEmpty(userName))
            {
                Log.Error("ArgumentNullException in CancelCurrentTimeShifting");
                throw new ArgumentNullException("userName");
            }
            User currentUser = GetUserByUserName(userName);

            return _tvControl.StopTimeShifting(ref currentUser);
        }


        public IList<WebChannelDetailed> GetChannelsDetailed(int groupId)
        {
            if (groupId <= 0)
                return null;

            return _tvBusiness.GetTVGuideChannelsForGroup(groupId).Select(ch => ch.ToWebChannelDetailed()).ToList();
        }

        public IList<WebChannelDetailed> GetChannelsDetailedByRange(int groupId, int startIndex, int count)
        {
            if (groupId <= 0)
                return null;

            if (startIndex < 0)
                return null;

            return _tvBusiness.GetTVGuideChannelsForGroup(groupId).GetRange(startIndex, count).Select(ch => ch.ToWebChannelDetailed()).ToList();
        }

        public IList<WebChannelBasic> GetChannelsBasic(int groupId)
        {
            if (groupId <= 0)
                return null;

            return _tvBusiness.GetTVGuideChannelsForGroup(groupId).Select(ch => ch.ToWebChannelBasic()).ToList();
        }

        public IList<WebChannelBasic> GetChannelsBasicByRange(int groupId, int startIndex, int count)
        {
            if (groupId <= 0)
            {
                Log.Debug("GroupId in GetChannelsBasicByRange is smaller than 0");
                return null;
            }
            if (startIndex < 0)
            {
                Log.Debug("StartIndex in GetChannelsBasicByRange is smaller than 0");
                return null;
            }

            return _tvBusiness.GetTVGuideChannelsForGroup(groupId).GetRange(startIndex, count).Select(ch => ch.ToWebChannelBasic()).ToList();
        }

        public int GetChannelCount(int groupId)
        {
            if (groupId <= 0)
            {
                Log.Debug("GroupId in GetChannelsBasicByRange is smaller than 0");
                return 0;
            }

            return _tvBusiness.GetTVGuideChannelsForGroup(groupId).Count;
        }

        public IList<WebRecordingBasic> GetRecordings()
        {
            return Recording.ListAll().Select(rec => rec.ToWebRecording()).ToList();
        }
        public WebRecordingBasic GetRecordingById(int recordingId)
        {
            return Recording.ListAll().First(p => p.IdRecording == recordingId).ToWebRecording();
        }

        public IList<WebScheduleBasic> GetSchedules()
        {
            return Schedule.ListAll().Select(sch => sch.ToWebSchedule()).ToList();
        }
        public WebScheduleBasic GetScheduleById(int scheduleId)
        {
            return Schedule.Retrieve(scheduleId).ToWebSchedule();
        }

        public WebChannelDetailed GetChannelDetailedById(int channelId)
        {
            if (channelId <= 0)
            {
                Log.Debug("ChannelId in GetChannelDetailedById is smaller than 0");
                return null;
            }

            return Channel.Retrieve(channelId).ToWebChannelDetailed();
        }
        public Stream GetChannelLogo(int channelId)
        {
            var channel = Channel.Retrieve(channelId);
            if (channel != null)
            {
                var fileList = GetChannelLogos();
                if (fileList != null && fileList.Count > 0)
                {

                    var logo = fileList.First(p => Path.GetFileNameWithoutExtension(p.Name).ToLowerInvariant() == channel.DisplayName.ToLowerInvariant());
                    if (logo != null && File.Exists(logo.FullName))
                    {
                        return GetImage(logo.FullName);

                    }
                }

            }
            return null;
        }

        public WebChannelBasic GetChannelBasicById(int channelId)
        {
            if (channelId <= 0)
            {
                Log.Debug("ChannelId in GetChannelBasicById is smaller than 0");
                return null;
            }

            return Channel.Retrieve(channelId).ToWebChannelBasic();
        }

        public WebProgramDetailed GetProgramDetailedById(int programId)
        {
            if (programId <= 0)
            {
                Log.Debug("programId in GetProgramDetailedById is smaller than 0");
                return null;
            }

            return Program.Retrieve(programId).ToWebProgramDetailed();
        }

        public WebProgramBasic GetProgramBasicById(int programId)
        {
            if (programId <= 0)
            {
                Log.Debug("programId in GetProgramBasicById is smaller or equal 0");
                return null;
            }

            return Program.Retrieve(programId).ToWebProgramBasic();
        }
        public IList<WebProgramBasic> GetNowNextWebProgramBasicForChannel(int channelId)
        {
            if (channelId <= 0)
            {
                Log.Debug("ChannelId in GetCurrentAndNextProgramForChannel is smaller than 0");
                return null;
            }
            return Channel.Retrieve(channelId).ToListWebProgramBasicNowNext();
        }
        public IList<WebProgramDetailed> GetNowNextWebProgramDetailedForChannel(int channelId)
        {
            if (channelId <= 0)
            {
                Log.Debug("ChannelId in GetCurrentAndNextProgramForChannel is smaller than 0");
                return null;
            }
            return Channel.Retrieve(channelId).ToListWebProgramDetailedNowNext();
        }


        public IList<WebProgramBasic> GetProgramsBasicForChannel(int channelId, DateTime startTime, DateTime endTime)
        {
            return _tvBusiness.GetPrograms(Channel.Retrieve(channelId), startTime, endTime).Select(p => p.ToWebProgramBasic()).ToList();
        }

        public IList<WebProgramDetailed> GetProgramsDetailedForChannel(int channelId, DateTime startTime, DateTime endTime)
        {
            return _tvBusiness.GetPrograms(Channel.Retrieve(channelId), startTime, endTime).Select(p => p.ToWebProgramDetailed()).ToList();
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
            return Schedule.ListAll().Where(schedule => schedule.IsRecordingProgram(p, true) && schedule.IdChannel == p.IdChannel).Count() > 0;
        }

        public IList<WebProgramDetailed> SearchProgramsDetailed(string searchTerm)
        {
            return _tvBusiness.SearchPrograms(searchTerm).Select(p => p.ToWebProgramDetailed()).ToList();
        }

        public IList<WebProgramBasic> SearchProgramsBasic(string searchTerm)
        {
            return _tvBusiness.SearchPrograms(searchTerm).Select(p => p.ToWebProgramBasic()).ToList();
        }

        public IList<WebChannelGroup> GetGroups()
        {
            return ChannelGroup.ListAll().Select(chg => chg.ToWebChannelGroup()).ToList();
        }

        public WebChannelGroup GetGroupById(int groupId)
        {
            if (groupId <= 0)
            {
                Log.Debug("groupId in GetGroupById is smaller or equal 0");
                return null;
            }

            return ChannelGroup.Retrieve(groupId).ToWebChannelGroup();
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
            // mgp first cancel all of the episodes of this program for this schedule
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

        public IList<WebCard> GetCards()
        {
            return Card.ListAll().Select(c => c.ToWebCard()).ToList();
        }

        public IList<WebVirtualCard> GetActiveCards()
        {
            return GetTimeshiftingOrRecordingVirtualCards().Select(card => card.ToWebVirtualCard()).ToList();
        }

        public IList<WebRtspClient> GetStreamingClients()
        {
            return _tvControl.StreamingClients.Select(cl => cl.ToWebRtspClient()).ToList();
        }

        public IList<WebUser> GetActiveUsers()
        {
            return GetTimeshiftingOrRecordingVirtualCards().Select(card => card.User.ToWebUser()).ToList();
        }

        public WebProgramDetailed GetCurrentProgramOnChannel(int channelId)
        {
            if (channelId <= 0)
            {
                Log.Debug("channelId in GetCurrentProgramOnChannel is smaller or equal 0");
                return null;
            }


            return Channel.Retrieve(channelId).CurrentProgram.ToWebProgramDetailed();
        }

        public WebProgramDetailed GetNextProgramOnChannel(int channelId)
        {
            if (channelId <= 0)
            {
                Log.Debug("channelId in GetNextProgramOnChannel is smaller or equal 0");
                return null;
            }


            return Channel.Retrieve(channelId).NextProgram.ToWebProgramDetailed();
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

        #region Private Methods
        private void InitializeGentleAndTVE()
        {
            try
            {
                // read Gentle configuration from CommonAppData
                string gentleConfigFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Team MediaPortal\MediaPortal TV Server\Gentle.config";

                if (!System.IO.File.Exists(gentleConfigFile))
                    throw new System.IO.FileNotFoundException("The Gentle configuration file couldn't be found. This occurs when TV Server is not installed.", gentleConfigFile);

                Gentle.Common.Configurator.AddFileHandler(gentleConfigFile);

                // connect to tv server via TVE API
                RemoteControl.Clear();
                RemoteControl.HostName = GetMasterServerHostname();

                _tvControl = RemoteControl.Instance;
            }
            catch (Exception ex)
            {
                // TODO: log the exception
                Log.Error("Failed to connect to TVEngine", ex);
            }
        }

        private string GetMasterServerHostname()
        {
            return TvDatabase.Server.ListAll().First(server => server.IsMaster).HostName;
        }

        private VirtualCard SwitchTVServerToChannel(string userName, int channelId)
        {
            if (String.IsNullOrEmpty(userName))
            {
                Log.Error("ArgumentNullException for argument userName in SwitchTVServerToChannel");
                throw new ArgumentNullException("userName");
            }

            if (channelId == 0)
            {
                Log.Error("ArgumentNullException for argument channelId in SwitchTVServerToChannel, channelId is not valid");
                throw new ArgumentException("The channel id is not valid!", "channelId");
            }
            Log.Debug("getting UserName");
            User currentUser = GetUserByUserName(userName);

            VirtualCard tvCard;
            Log.Debug("Start Timeshifting");
            TvResult result = _tvControl.StartTimeShifting(ref currentUser, channelId, out tvCard);

            if (result != TvResult.Succeeded)
            {
                Log.Debug("Timeshifting hasn't succeded, Thread sleeps 2000ms");
                // wait a short while
                System.Threading.Thread.Sleep(2000);

                // Try again
                Log.Debug("Timeshifting hasn't succeded,start timeshifting again");
                result = _tvControl.StartTimeShifting(ref currentUser, channelId, out tvCard);

                if (result != TvResult.Succeeded)
                {
                    // didn't work
                    Log.Error("InvalidOperationException couldn't start TV stream: " + result);
                    throw new InvalidOperationException("Couldn't start TV stream: " + result);

                }
            }
            Log.Debug("Timeshifting has succeded");
            if (tvCard == null)
            {
                Log.Error("NullReferenceException: Couldn't get virtual card.");
                throw new NullReferenceException("Couldn't get virtual card.");
            }
            // set card id and channel id of user, required for heartbeat and stopping of timeshifting
            currentUser.CardId = tvCard.Id;
            currentUser.IdChannel = channelId;

            _tvUsers[userName] = currentUser;

            return tvCard;
        }

        private User GetUserByUserName(string userName)
        {
            if (!_tvUsers.ContainsKey(userName))
                _tvUsers.Add(userName, new User(userName, false));

            return _tvUsers[userName];
        }

        private IList<VirtualCard> GetTimeshiftingOrRecordingVirtualCards()
        {
            List<VirtualCard> result = new List<VirtualCard>();

            foreach (Card card in Card.ListAll())
            {
                if (!RemoteControl.Instance.CardPresent(card.IdCard))
                    continue;

                User[] users = RemoteControl.Instance.GetUsersForCard(card.IdCard);

                if (users == null)
                    continue;

                foreach (User user in users)
                {
                    if (card.IdCard != user.CardId)
                        continue;

                    VirtualCard tvCard = new VirtualCard(user, RemoteControl.HostName);

                    if (!tvCard.IsTimeShifting && !tvCard.IsRecording)
                        continue;

                    result.Add(tvCard);
                }
            }

            return result;
        }

        private static Stream GetImage(string path)
        {
            if (System.IO.File.Exists(path))
            {
                try
                {
                    FileStream fs = File.OpenRead(path);
                    WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";
                    return fs;
                }
                catch (Exception ex)
                {
                    Log.Error("Error getting image " + path, ex);
                }
            }
            return null;
        }
        private static IList<FileInfo> GetChannelLogos()
        {
            List<FileInfo> logoPaths = new List<FileInfo>();

            var path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\TV4Home\\ChannelLogos";
            if (Directory.Exists(path))
            {
                try
                {
                    System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(path);
                    System.IO.FileInfo[] fileList = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                    logoPaths = fileList.Where(p => (p.Extension.ToLowerInvariant() == ".png") || (p.Extension.ToLowerInvariant() == ".jpg")).ToList();
                }
                catch (Exception ex)
                {
                    Log.Error("Exception in GetChannelLogos", ex);
                }
            }
            return logoPaths;
        }
        private WebChannelState ToWebChannelState(ChannelState state)
        {

            switch (state)
            {
                case ChannelState.tunable:
                    return WebChannelState.Tunable;
                case ChannelState.timeshifting:
                    return WebChannelState.Timeshifting;
                case ChannelState.recording:
                    return WebChannelState.Recording;
                case ChannelState.nottunable:
                    return WebChannelState.NotTunable;
            }
            return WebChannelState.Tunable;
        }

        #endregion
    }

    /// <summary>
    /// This is required for VS to pick up the reference in the web project. Ignore it.
    /// </summary>
    class GentleProviders
    {
        private GentleProviders()
        {
            MySQLProvider prov1 = new MySQLProvider("");
            SQLServerProvider prov2 = new SQLServerProvider("");
        }
    }
}
