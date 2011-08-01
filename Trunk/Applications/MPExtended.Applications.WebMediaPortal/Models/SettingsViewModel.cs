using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.WebPages.Html;
using WebMediaPortal.Services;

namespace WebMediaPortal.Models
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
                foreach (var profile in WebServices.WebStreamService.GetTranscoderProfiles())
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
                foreach (var group in WebServices.TVService.GetGroups())
                {
                    _groups.Add(new SelectListItem() { Text = group.GroupName, Value = group.IdGroup.ToString() });

                }

                return _groups;
            }
        }
     
    }

    
}