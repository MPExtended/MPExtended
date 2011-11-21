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
using System.Web.WebPages.Html;
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

        public SettingModel Settings
        {
            get
            {
                return Code.Settings.GlobalSettings;
            }
        }

        [Required(ErrorMessage = "Please select a group")]
        public string SelectedGroup { get; set; }
        
        [Required(ErrorMessage = "Please select a media profile")]
        public string SelectedMediaProfile { get; set; }

        [Required(ErrorMessage = "Please select a tv profile")]
        public string SelectedTVProfile { get; set; }

        public SettingsViewModel()
        {
            SelectedGroup = Code.Settings.GlobalSettings.DefaultGroup.ToString();
            SelectedMediaProfile = Code.Settings.GlobalSettings.DefaultMediaProfile;
            SelectedTVProfile = Code.Settings.GlobalSettings.DefaultTVProfile;
        }
    }
}