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

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class SettingsViewModel
    {
        private List<SelectListItem> _profiles = new List<SelectListItem>();
        private List<SelectListItem> _groups = new List<SelectListItem>();

        public SettingsViewModel()
        {
            SelectedGroup = Code.Settings.GlobalSettings.DefaultGroup.ToString();
            SelectedProfile = Code.Settings.GlobalSettings.TranscodingProfile;
        }

        [Required(ErrorMessage = "Please select a group")]
        public string SelectedGroup { get; set; }

        [Required(ErrorMessage = "Please select a profile")]
        public string SelectedProfile { get; set; }
        public SettingModel Settings { get { return Code.Settings.GlobalSettings; } }
        public List<SelectListItem> Profiles
        {
            get
            {
                foreach (var profile in MPEServices.MASStreamControl.GetTranscoderProfiles())
                {
                    _profiles.Add(new SelectListItem() { Text = profile.Name, Value = profile.Name });

                }
                return _profiles;
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
     
    }

    
}