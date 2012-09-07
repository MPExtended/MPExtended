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
using System.IO;
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Services.TVAccessService.Interfaces;
using TvControl;
using TvDatabase;
using TvLibrary.Streaming;

namespace MPExtended.Services.TVAccessService
{
    internal static class WebCardExtensionMethods
    {
        public static WebCard ToWebCard(this Card card)
        {
            if (card == null)
            {
                Log.Warn("Tried to convert a null Card to WebCard");
                return null;
            }

            //card.T

            // See TvEngine3/TVLibrary/TvService/Scheduler/Scheduler.cs:1295 (SetupRecordingFolder) for the default fallback paths from MP
            return new WebCard
            {
                CAM = card.CAM,
                CamType = card.CamType,
                DecryptLimit = card.DecryptLimit,
                DevicePath = card.DevicePath,
                Enabled = card.Enabled,
                GrabEPG = card.GrabEPG,
                Id = card.IdCard,
                IsChanged = card.IsChanged,
                LastEpgGrab = card.LastEpgGrab != DateTime.MinValue ? card.LastEpgGrab : new DateTime(2000, 1, 1),
                Name = card.Name,
                NetProvider = card.netProvider,
                PreloadCard = card.PreloadCard,
                Priority = card.Priority,
                RecordingFolder = card.RecordingFolder != String.Empty ? card.RecordingFolder : 
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal", "MediaPortal TV Server", "recordings"),
                RecordingFormat = card.RecordingFormat,
                SupportSubChannels = card.supportSubChannels,
                TimeShiftFolder = card.TimeShiftFolder != String.Empty ? card.TimeShiftFolder :
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal", "MediaPortal TV Server", "timeshiftbuffer"),
            };
        }
    }

    internal static class WebVirtualCardExtensionMethods
    {
        public static WebVirtualCard ToWebVirtualCard(this VirtualCard card)
        {
            if (card == null)
            {
                Log.Warn("Tried to convert a null VirtualCard to WebVirtualCard");
                return null;
            }

            return new WebVirtualCard
            {
                BitRateMode = (int)card.BitRateMode,
                ChannelName = card.ChannelName,
                Device = card.Device,
                Enabled = card.Enabled,
                GetTimeshiftStoppedReason = (int)card.GetTimeshiftStoppedReason,
                GrabTeletext = card.GrabTeletext,
                HasTeletext = card.HasTeletext,
                Id = card.Id,
                ChannelId = card.IdChannel,
                IsGrabbingEpg = card.IsGrabbingEpg,
                IsRecording = card.IsRecording,
                IsScanning = card.IsScanning,
                IsScrambled = card.IsScrambled,
                IsTimeShifting = card.IsTimeShifting,
                IsTunerLocked = card.IsTunerLocked,
                MaxChannel = card.MaxChannel,
                MinChannel = card.MinChannel,
                Name = card.Name,
                QualityType = (int)card.QualityType,
                RecordingFileName = card.RecordingFileName,
                RecordingFolder = card.RecordingFolder,
                RecordingFormat = card.RecordingFormat,
                RecordingScheduleId = card.RecordingScheduleId,
                RecordingStarted = card.RecordingStarted != DateTime.MinValue ? card.RecordingStarted : new DateTime(2000, 1, 1),
                RemoteServer = card.RemoteServer,
                RTSPUrl = card.RTSPUrl,
                SignalLevel = card.SignalLevel,
                SignalQuality = card.SignalQuality,
                TimeShiftFileName = card.TimeShiftFileName,
                TimeShiftFolder = card.TimeshiftFolder,
                TimeShiftStarted = card.TimeShiftStarted != DateTime.MinValue ? card.TimeShiftStarted : new DateTime(2000, 1, 1),
                Type = (WebCardType)card.Type,
                User = card.User != null ? card.User.ToWebUser() : null
            };
        }
    }

    internal static class WebUserExtensionMethods
    {
        public static WebUser ToWebUser(this IUser user)
        {
            if (user == null)
            {
                Log.Warn("Tried to convert a null User to WebUser");
                return null;
            }

            return new WebUser
            {
                CardId = user.CardId,
                HeartBeat = user.HeartBeat != DateTime.MinValue ? user.HeartBeat : new DateTime(2000, 1, 1),
                ChannelId = user.IdChannel,
                IsAdmin = user.IsAdmin,
                Name = user.Name,
                SubChannel = user.SubChannel,
                TvStoppedReason = (int)user.TvStoppedReason
            };
        }
    }

    internal static class WebChannelExtensionMethods
    {
        public static WebChannelDetailed ToWebChannelDetailed(this Channel ch)
        {
            if (ch == null)
            {
                Log.Warn("Tried to convert a null Channel to WebChannelDetailed");
                return null;
            }

            return new WebChannelDetailed
            {
                CurrentProgram = ch.CurrentProgram != null ? ch.CurrentProgram.ToWebProgramDetailed() : null,
                Title = ch.DisplayName,
                EpgHasGaps = ch.EpgHasGaps,
                ExternalId = ch.ExternalId,
                FreeToAir = GetFreeToAirInformation(ch),
                GrabEpg = ch.GrabEpg,
                GroupNames = ch.GroupNames,
                Id = ch.IdChannel,
                IsChanged = ch.IsChanged,
                IsRadio = ch.IsRadio,
                IsTv = ch.IsTv,
                LastGrabTime = ch.LastGrabTime != DateTime.MinValue ? ch.LastGrabTime : new DateTime(2000, 1, 1),
                NextProgram = ch.NextProgram.ToWebProgramDetailed(),
                SortOrder = ch.SortOrder,
                TimesWatched = ch.TimesWatched,
                TotalTimeWatched = ch.TotalTimeWatched != DateTime.MinValue ? ch.TotalTimeWatched : new DateTime(2000, 1, 1),
                VisibleInGuide = ch.VisibleInGuide
            };
        }

        /// <summary>
        /// Checks if the channel is available on free to air or scrambled
        /// </summary>
        /// <param name="ch">The channel to get the free to air information for.</param>
        /// <returns>0 if the channel is available only scrambled, 1 if it is only available free to air, 2 if it is available scrambled and free to air.</returns>
        private static int GetFreeToAirInformation(Channel ch)
        {
            IList<TuningDetail> tuningDetails = ch.ReferringTuningDetail();

            // channel is available on only one frequency, use the FreeToAir information from this frequency
            if (tuningDetails.Count == 1)
                return tuningDetails.First().FreeToAir ? 1 : 0;


            // if the channel is available on more than one frequency, check if it is available only free to air, only scrambled or both
            bool hasFta = tuningDetails.Any(i => i.FreeToAir);
            bool hasScrambled = tuningDetails.Any(i => !i.FreeToAir);

            // has both scrambled and free to air frequencies
            if (hasFta && hasScrambled)
                return 2;

            // has only scrambled frequency
            if (hasScrambled)
                return 0;

            // has only free to air frequency
            return 1;
        }

        public static WebChannelBasic ToWebChannelBasic(this Channel ch)
        {
            if (ch == null)
            {
                Log.Warn("Tried to convert a null Channel to WebChannelBasic");
                return null;
            }

            return new WebChannelBasic
            {
                Title = ch.DisplayName,
                Id = ch.IdChannel,
                IsRadio = ch.IsRadio,
                IsTv = ch.IsTv,
                SortOrder = ch.SortOrder,
            };
        }

        public static List<WebProgramBasic> ToListWebProgramBasicNowNext(this Channel ch)
        {
            if (ch == null)
            {
                Log.Warn("Tried to convert a null Channel to WebProgramBasic");
                return null;
            }

            List<WebProgramBasic> tmp = new List<WebProgramBasic>();
            tmp.Add(ch.CurrentProgram.ToWebProgramBasic());
            tmp.Add(ch.NextProgram.ToWebProgramBasic());
            return tmp;
        }

        public static List<WebProgramDetailed> ToListWebProgramDetailedNowNext(this Channel ch)
        {
            if (ch == null)
            {
                Log.Warn("Tried to convert a null Channel to WebProgramDetailed");
                return null;
            }

            List<WebProgramDetailed> tmp = new List<WebProgramDetailed>();
            tmp.Add(ch.CurrentProgram.ToWebProgramDetailed());
            tmp.Add(ch.NextProgram.ToWebProgramDetailed());
            return tmp;
        }
    }

    internal static class WebChannelGroupExtensionMethods
    {
        public static WebChannelGroup ToWebChannelGroup(this ChannelGroup group)
        {
            if (group == null)
            {
                Log.Warn("Tried to convert a null ChannelGroup to WebChannelGroup");
                return null;
            }

            return new WebChannelGroup
            {
                GroupName = group.GroupName,
                Id = group.IdGroup,
                IsChanged = group.IsChanged,
                SortOrder = group.SortOrder,
                IsRadio = false,
                IsTv = true
            };
        }

        public static WebChannelGroup ToWebChannelGroup(this RadioChannelGroup group)
        {
            if (group == null)
            {
                Log.Warn("Tried to convert a null ChannelGroup to WebChannelGroup");
                return null;
            }

            return new WebChannelGroup
            {
                GroupName = group.GroupName,
                Id = group.IdGroup,
                IsChanged = group.IsChanged,
                SortOrder = group.SortOrder,
                IsRadio = true,
                IsTv = false
            };
        }
    }

    internal static class WebProgramExtensionMethods
    {
        private static IEnumerable<Schedule> AllSchedules;

        public static IDisposable CacheSchedules()
        {
            return new CacheLifetimeToken<IEnumerable<Schedule>>(val => AllSchedules = val, () => AllSchedules = Schedule.ListAll());
        }

        public static WebProgramDetailed ToWebProgramDetailed(this Program p)
        {
            if (p == null)
            {
                Log.Warn("Tried to convert a null Program to WebProgramDetailed");
                return null;
            }

            return new WebProgramDetailed
            {
                Classification = p.Classification,
                Description = p.Description,
                EndTime = p.EndTime != DateTime.MinValue ? p.EndTime : new DateTime(2000, 1, 1),
                EpisodeName = p.EpisodeName,
                EpisodeNum = p.EpisodeNum,
                EpisodeNumber = p.EpisodeNumber,
                EpisodePart = p.EpisodePart,
                Genre = p.Genre,
                HasConflict = p.HasConflict,
                ChannelId = p.IdChannel,
                Id = p.IdProgram,
                IsChanged = p.IsChanged,
                IsPartialRecordingSeriesPending = p.IsPartialRecordingSeriesPending,
                IsRecording = p.IsRecording,
                IsRecordingManual = p.IsRecordingManual,
                IsRecordingOnce = p.IsRecordingOnce,
                IsRecordingOncePending = p.IsRecordingOncePending,
                IsRecordingSeries = p.IsRecordingSeries,
                IsRecordingSeriesPending = p.IsRecordingSeriesPending,
                Notify = p.Notify,
                OriginalAirDate = p.OriginalAirDate != DateTime.MinValue ? p.OriginalAirDate : new DateTime(2000, 1, 1),
                ParentalRating = p.ParentalRating,
                SeriesNum = p.SeriesNum,
                StarRating = p.StarRating,
                StartTime = p.StartTime != DateTime.MinValue ? p.StartTime : new DateTime(2000, 1, 1),
                Title = p.Title,
                DurationInMinutes = (int)((p.EndTime - p.StartTime).TotalMinutes),
                IsScheduled = (AllSchedules == null ? Schedule.ListAll() : AllSchedules)
                                    .Where(schedule => schedule.IdChannel == p.IdChannel && schedule.IsRecordingProgram(p, true)).Count() > 0
            };
        }

        public static WebProgramBasic ToWebProgramBasic(this Program p)
        {
            if (p == null)
            {
                Log.Warn("Tried to convert a null Program to WebProgramBasic");
                return null;
            }

            return new WebProgramBasic
            {
                Description = p.Description,
                EndTime = p.EndTime != DateTime.MinValue ? p.EndTime : new DateTime(2000, 1, 1),
                ChannelId = p.IdChannel,
                Id = p.IdProgram,
                StartTime = p.StartTime != DateTime.MinValue ? p.StartTime : new DateTime(2000, 1, 1),
                Title = p.Title,
                DurationInMinutes = (int)((p.EndTime - p.StartTime).TotalMinutes),
                IsScheduled = Schedule.ListAll().Where(schedule => schedule.IdChannel == p.IdChannel && schedule.IsRecordingProgram(p, true)).Count() > 0
            };
        }
    }

    internal static class WebRtspClientExtensionMethods
    {
        public static WebRtspClient ToWebRtspClient(this RtspClient rtsp)
        {
            if (rtsp == null)
            {
                Log.Warn("Tried to convert a null RtspClient to WebRtspClient");
                return null;
            }

            return new WebRtspClient
            {
                DateTimeStarted = rtsp.DateTimeStarted != DateTime.MinValue ? rtsp.DateTimeStarted : new DateTime(2000, 1, 1),
                Description = rtsp.Description,
                IpAdress = rtsp.IpAdress,
                IsActive = rtsp.IsActive,
                StreamName = rtsp.StreamName
            };
        }
    }

    internal static class WebScheduleExtensionMethods
    {
        public static WebScheduleBasic ToWebSchedule(this Schedule sch)
        {
            if (sch == null)
            {
                Log.Warn("Tried to convert a null Schedule to WebSchedule");
                return null;
            }

            return new WebScheduleBasic
            {
                BitRateMode = (int)sch.BitRateMode,
                Canceled = sch.Canceled != DateTime.MinValue ? sch.Canceled : new DateTime(2000, 1, 1),
                Directory = sch.Directory,
                DoesUseEpisodeManagement = sch.DoesUseEpisodeManagement,
                EndTime = sch.EndTime != DateTime.MinValue ? sch.EndTime : new DateTime(2000, 1, 1),
                ChannelId = sch.IdChannel,
                ParentScheduleId = sch.IdParentSchedule,
                Id = sch.IdSchedule,
                IsChanged = sch.IsChanged,
                IsManual = sch.IsManual,
                KeepDate = sch.KeepDate != DateTime.MinValue ? sch.KeepDate : new DateTime(2000, 1, 1),
                KeepMethod = (WebScheduleKeepMethod)sch.KeepMethod,
                MaxAirings = sch.MaxAirings,
                PostRecordInterval = sch.PostRecordInterval,
                PreRecordInterval = sch.PreRecordInterval,
                Priority = sch.Priority,
                Title = sch.ProgramName,
                Quality = sch.Quality,
                QualityType = (int)sch.QualityType,
                RecommendedCard = sch.RecommendedCard,
                ScheduleType = (WebScheduleType)sch.ScheduleType,
                Series = sch.Series,
                StartTime = sch.StartTime != DateTime.MinValue ? sch.StartTime : new DateTime(2000, 1, 1)
            };
        }
    }

    internal static class WebRecordingExtensionMethods
    {
        public static WebRecordingBasic ToWebRecording(this Recording rec)
        {
            if (rec == null)
            {
                Log.Warn("Tried to convert a null Recording to WebRecordingBasic");
                return null;
            }

            return new WebRecordingBasic
            {
                Description = rec.Description,
                EndTime = rec.EndTime != DateTime.MinValue ? rec.EndTime : new DateTime(2000, 1, 1),
                EpisodeName = rec.EpisodeName,
                EpisodeNum = rec.EpisodeNum,
                EpisodeNumber = rec.EpisodeNumber,
                EpisodePart = rec.EpisodePart,
                FileName = rec.FileName,
                Genre = rec.Genre,
                ChannelId = rec.IdChannel,
                Id = rec.IdRecording,
                ScheduleId = rec.Idschedule,
                IsChanged = rec.IsChanged,
                IsManual = rec.IsManual,
                IsRecording = rec.IsRecording,
                KeepUntil = rec.KeepUntil,
                KeepUntilDate = rec.KeepUntilDate != DateTime.MinValue ? rec.KeepUntilDate : new DateTime(2000, 1, 1),
                SeriesNum = rec.SeriesNum,
                ShouldBeDeleted = rec.ShouldBeDeleted,
                StartTime = rec.StartTime != DateTime.MinValue ? rec.StartTime : new DateTime(2000, 1, 1),
                StopTime = rec.StopTime,
                TimesWatched = rec.TimesWatched,
                Title = rec.Title,
                ChannelName = Channel.Retrieve(rec.IdChannel).DisplayName
            };
        }
    }

    internal static class WebChannelStateExtensionMethods
    {
        public static WebChannelState ToWebChannelState(this TvControl.ChannelState state, int channelId)
        {
            WebChannelState newState = new WebChannelState();
            newState.ChannelId = channelId;
            switch (state)
            {
                case TvControl.ChannelState.tunable:
                    newState.State = Interfaces.ChannelState.Tunable;
                    break;
                case TvControl.ChannelState.timeshifting:
                    newState.State = Interfaces.ChannelState.Timeshifting;
                    break;
                case TvControl.ChannelState.recording:
                    newState.State = Interfaces.ChannelState.Recording;
                    break;
                case TvControl.ChannelState.nottunable:
                    newState.State = Interfaces.ChannelState.NotTunable;
                    break;
                default:
                    newState.State = Interfaces.ChannelState.Unknown;
                    break;
            }

            return newState;
        }
    }
}
