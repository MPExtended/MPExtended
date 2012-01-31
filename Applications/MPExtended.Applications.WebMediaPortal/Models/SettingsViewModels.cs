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
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Mvc;
using MPExtended.Applications.WebMediaPortal.Strings;
using MPExtended.Libraries.Client;
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

        public IEnumerable<SelectListItem> Skins
        {
            get
            {
                IEnumerable<SelectListItem> items = new List<SelectListItem>()
                {
                    new SelectListItem() { Text = FormStrings.DefaultSkinName, Value = "default" }
                };

                string path = HttpContext.Current.Server.MapPath("~/Skins");
                if (Directory.Exists(path))
                {
                    items = items.Union(Directory.GetDirectories(path).Select(x => new SelectListItem()
                    {
                        Text = Path.GetFileName(x),
                        Value = Path.GetFileName(x)
                    }));

                }

                return items;
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

        [LocalizedDisplayName(typeof(FormStrings), "DefaultTVGroup")]
        [ListChoice("TVGroups", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidTVGroup")]
        public int? SelectedGroup { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "DefaultMediaStreamingProfile")]
        [ListChoice("MediaProfiles", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidMediaProfile")]
        public string SelectedMediaProfile { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "DefaultMusicStreamingProfile")]
        [ListChoice("AudioProfiles", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidMusicProfile")]
        public string SelectedAudioProfile { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "DefaultTVStreamingProfile")]
        [ListChoice("TVProfiles", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidTVProfile")]
        public string SelectedTVProfile { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "DefaultTVShowDatabase")]
        [ListChoice("TVShowDatabases", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidTVShowDatabase")]
        public int? TVShowProvider { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "DefaultMovieDatabase")]
        [ListChoice("MovieDatabases", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidMovieDatabase")]
        public int? MovieProvider { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "DefaultMusicDatabase")]
        [ListChoice("MusicDatabases", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidMusicDatabase")]
        public int? MusicProvider { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "StreamType")]
        public StreamType StreamType { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "VLCPlayerEnableDeinterlacing")]
        public bool EnableDeinterlace { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "Skin")]
        [Required(ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidSkin")]
        [ListChoice("Skins", AllowNull = false, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidSkin")]
        public string Skin { get; set; }

        public SettingsViewModel()
        {
        }

        public SettingsViewModel(SettingModel model)
        {
		    Skin = model.Skin;
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
            changeModel.Skin = Skin;
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
        [LocalizedDisplayName(typeof(FormStrings), "ClientService")]
        [Required(ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidClientService")]
        [CustomValidation(typeof(Validators), "ValidateMASUrl", ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "FailedToConnectToClient")]
        public string MASUrl { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "ServerService")]
        [Required(ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidServerService")]
        [CustomValidation(typeof(Validators), "ValidateTASUrl", ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "FailedToConnectToServer")]
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