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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MPExtended.Libraries.General;
using MPExtended.Applications.WebMediaPortal.Code;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class SettingsViewModel
    {
        private List<SelectListItem> _mediaProfiles = new List<SelectListItem>();
        private List<SelectListItem> _tvProfiles = new List<SelectListItem>();
        private List<SelectListItem> _groups = new List<SelectListItem>();

        public List<SelectListItem> MediaProfiles
        {
            get
            {
                foreach (var profile in MPEServices.MASStreamControl.GetTranscoderProfiles())
                {
                    _mediaProfiles.Add(new SelectListItem() { Text = profile.Name, Value = profile.Name });
                }

                return _mediaProfiles;
            }
        }

        public List<SelectListItem> TVProfiles
        {
            get
            {
                foreach (var profile in MPEServices.TASStreamControl.GetTranscoderProfiles())
                {
                    _tvProfiles.Add(new SelectListItem() { Text = profile.Name, Value = profile.Name });
                }

                return _tvProfiles;
            }
        }

        public List<SelectListItem> Groups
        {
            get
            {
                foreach (var group in MPEServices.TAS.GetGroups())
                {
                    _groups.Add(new SelectListItem() { Text = group.GroupName, Value = group.Id.ToString() });
                }

                return _groups;
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
        [Required(ErrorMessage = "Please select a group")]
        public int SelectedGroup { get; set; }

        [DisplayName("Default media streaming profile")]
        [Required(ErrorMessage = "Please select a media profile")]
        public string SelectedMediaProfile { get; set; }

        [DisplayName("Default TV streaming profile")]
        [Required(ErrorMessage = "Please select a tv profile")]
        public string SelectedTVProfile { get; set; }

        [DisplayName("TV Show database")]
        [Required(ErrorMessage = "Please specify a valid TV show database")]
        public int TVShowProvider { get; set; }

        [DisplayName("Movie database")]
        [Required(ErrorMessage = "Please specify a valid movie database")]
        public int MovieProvider { get; set; }

        [DisplayName("Music database")]
        [Required(ErrorMessage = "Please specify a valid music database")]
        public int MusicProvider { get; set; }

        public SettingsViewModel()
        {
        }

        public SettingsViewModel(SettingModel model)
        {
            SelectedGroup = model.DefaultGroup;
            SelectedMediaProfile = model.DefaultMediaProfile;
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
            changeModel.DefaultGroup = SelectedGroup;
            changeModel.DefaultMediaProfile = SelectedMediaProfile;
            changeModel.DefaultTVProfile = SelectedTVProfile;
            changeModel.TVShowProvider = TVShowProvider;
            changeModel.MusicProvider = MusicProvider;
            changeModel.MovieProvider = MovieProvider;
            return changeModel;
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