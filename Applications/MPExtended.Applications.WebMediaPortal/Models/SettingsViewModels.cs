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
using MPExtended.Libraries.General;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class SettingsViewModel
    {
        public List<SelectListItem> MediaProfiles
        {
            get
            {
                return GetProfiles(MPEServices.MASStreamControl, "pc-vlc-video", "pc-flash-video");
            }
        }

        public List<SelectListItem> AudioProfiles
        {
            get
            {
                return GetProfiles(MPEServices.MASStreamControl, "pc-vlc-audio", "pc-flash-audio");
            }
        }

        public List<SelectListItem> TVProfiles
        {
            get
            {
                return GetProfiles(MPEServices.TASStreamControl, "pc-vlc-video", "pc-flash-video");
            }
        }

        public List<SelectListItem> TVGroups
        {
            get
            {
                return MPEServices.TAS.GetGroups()
                    .Select(x => new SelectListItem() { Text = x.GroupName, Value = x.Id.ToString() })
                    .ToList();
            }
        }

        public IEnumerable<SelectListItem> TVShowDatabases
        {
            get
            {
                return MPEServices.MAS.GetServiceDescription().AvailableTvShowLibraries
                    .Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        public IEnumerable<SelectListItem> MovieDatabases
        {
            get
            {
                return MPEServices.MAS.GetServiceDescription().AvailableMovieLibraries
                    .Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        public IEnumerable<SelectListItem> MusicDatabases
        {
            get
            {
                return MPEServices.MAS.GetServiceDescription().AvailableMusicLibraries
                    .Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        public bool ShowMASConfiguration
        {
            get
            {
                return MPEServices.HasMASConnection;
            }
        }

        public bool ShowTASConfiguration
        {
            get
            {
                return MPEServices.HasTASConnection;
            }
        }

        [DisplayName("Default TV group")]
        [ListChoice("TVGroups", AllowNull = true, ErrorMessage="Please select a valid TV group")]
        public int SelectedGroup { get; set; }

        [DisplayName("Default media streaming profile")]
        [ListChoice("MediaProfiles", AllowNull = true, ErrorMessage="Please select a valid media streaming profile")]
        public string SelectedMediaProfile { get; set; }

        [DisplayName("Default music streaming profile")]
        [ListChoice("AudioProfiles", AllowNull = true, ErrorMessage = "Please select a valid audio streaming profile")]
        public string SelectedAudioProfile { get; set; }

        [DisplayName("Default TV streaming profile")]
        [ListChoice("TVProfiles", AllowNull = true, ErrorMessage = "Please select a valid TV streaming profile")]
        public string SelectedTVProfile { get; set; }

        [DisplayName("TV Show database")]
        [ListChoice("TVShowDatabases", AllowNull = true, ErrorMessage = "Please select a valid TV show database")]
        public int TVShowProvider { get; set; }

        [DisplayName("Movie database")]
        [ListChoice("MovieDatabases", AllowNull = true, ErrorMessage = "Please select a valid movie database")]
        public int MovieProvider { get; set; }

        [DisplayName("Music database")]
        [ListChoice("MusicDatabases", AllowNull = true, ErrorMessage = "Please select a valid music database")]
        public int MusicProvider { get; set; }


        [DisplayName("Stream type")]
        public StreamType StreamType { get; set; }

        [DisplayName("VLC player: enable deinterlacing by default?")]
        public bool EnableDeinterlace { get; set; }


        public SettingsViewModel()
        {
        }

        public SettingsViewModel(SettingModel model)
        {
            StreamType = model.StreamType;
            EnableDeinterlace = model.EnableDeinterlace;
            SelectedGroup = model.DefaultGroup;
            SelectedMediaProfile = model.DefaultMediaProfile;
            SelectedAudioProfile = model.DefaultAudioProfile;
            SelectedTVProfile = model.DefaultTVProfile;

            if (ShowMASConfiguration)
            {
                var serviceDesc = MPEServices.MAS.GetServiceDescription();
                MovieProvider = GetCurrentProvider(model.MovieProvider, serviceDesc.DefaultMovieLibrary);
                MusicProvider = GetCurrentProvider(model.MusicProvider, serviceDesc.DefaultMusicLibrary);
                TVShowProvider = GetCurrentProvider(model.TVShowProvider, serviceDesc.DefaultTvShowLibrary);
            }
        }

        public SettingModel ToSettingModel(SettingModel changeModel)
        {
            changeModel.StreamType = StreamType;
            changeModel.EnableDeinterlace = EnableDeinterlace;
            changeModel.DefaultGroup = SelectedGroup;
            changeModel.DefaultMediaProfile = SelectedMediaProfile;
            changeModel.DefaultAudioProfile = SelectedAudioProfile;
            changeModel.DefaultTVProfile = SelectedTVProfile;

            changeModel.TVShowProvider = TVShowProvider;
            changeModel.MusicProvider = MusicProvider;
            changeModel.MovieProvider = MovieProvider;
            return changeModel;
        }

        private List<SelectListItem> GetProfiles(IWebStreamingService service, params string[] targets)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (string target in targets)
            {
                foreach(var profile in service.GetTranscoderProfilesForTarget(target))
                {
                    items.Add(new SelectListItem() { Text = profile.Name, Value = profile.Name });
                }
            }
            return items;
        }

        private int GetCurrentProvider(int? setting, int defaultValue)
        {
            return setting == null ? defaultValue : setting.Value;
        }

        private string GetCurrentUrl(string local, string global)
        {
            if (local != null && local.Trim().Length > 0)
            {
                return local;
            }
            else
            {
                return global;
            }
        }
    }

    public class ServiceSettingsViewModel
    {
        [DisplayName("Client service")]
        [Required(ErrorMessage = "Please specify a valid MAS url")]
        [CustomValidation(typeof(Validators), "ValidateMASUrl", ErrorMessage = "Failed to connect to the specified client service")]
        public string MASUrl { get; set; }

        [DisplayName("Server service")]
        [Required(ErrorMessage = "Please specify a valid TAS url")]
        [CustomValidation(typeof(Validators), "ValidateTASUrl", ErrorMessage = "Failed to connect to the specified server service")]
        public string TASUrl { get; set; }

        public ServiceSettingsViewModel()
        {
        }

        public ServiceSettingsViewModel(SettingModel model)
        {
            MASUrl = model.MASUrl;
            TASUrl = model.TASUrl;
        }

        public SettingModel ToSettingModel(SettingModel changeModel)
        {
            changeModel.MASUrl = MASUrl;
            changeModel.TASUrl = TASUrl;
            return changeModel;
        }
    }
}