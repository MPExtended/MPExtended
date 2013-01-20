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
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Code.Composition;
using MPExtended.Applications.WebMediaPortal.Mvc;
using MPExtended.Applications.WebMediaPortal.Strings;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Config;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.StreamingService.Interfaces;
using Config = MPExtended.Libraries.Service.Config;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public enum StreamTypeWithDescription
    {
        [LocalizedDescription(typeof(FormStrings), "DirectStreamingProfileDescription")]
        Direct,

        [LocalizedDescription(typeof(FormStrings), "DirectWhenPossibleDescription")]
        DirectWhenPossible,

        [LocalizedDescription(typeof(FormStrings), "ProxiedStreamingProfileDescription")]
        Proxied,
    }

    public class ProfileViewModel
    {
        public string DefaultProfile { get; set; }
        public List<String> AvailableProfiles { get; set; }
    }

    public class SettingsViewModel
    {
        public List<SelectListItem> TVGroups
        {
            get
            {
                return Connections.Current.TAS.GetGroups()
                    .Select(x => new SelectListItem() { Text = x.GroupName, Value = x.Id.ToString() })
                    .ToList();
            }
        }

        public IEnumerable<SelectListItem> TVShowDatabases
        {
            get
            {
                return Connections.Current.MAS.GetServiceDescription().AvailableTvShowLibraries
                    .Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        public IEnumerable<SelectListItem> MovieDatabases
        {
            get
            {
                return Connections.Current.MAS.GetServiceDescription().AvailableMovieLibraries
                    .Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        public IEnumerable<SelectListItem> MusicDatabases
        {
            get
            {
                return Connections.Current.MAS.GetServiceDescription().AvailableMusicLibraries
                    .Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
            }
        }

        public IEnumerable<SelectListItem> Skins
        {
            get
            {
                var items = Composer.Instance.GetInstalledSkins()
                    .Select(x => new SelectListItem() { Text = x, Value = x })
                    .ToList();
                items.Add(new SelectListItem() { Text = FormStrings.DefaultSkinName, Value = "default" });
                return items;
            }
        }

        public IEnumerable<SelectListItem> Languages
        {
            get
            {
                return CultureDatabase.GetAvailableTranslations(UIStrings.ResourceManager)
                    .Select(x => new SelectListItem() { Text = x.DisplayName, Value = x.Name });
            }
        }

        public bool ShowMASConfiguration
        {
            get
            {
                return Connections.Current.HasMASConnection;
            }
        }

        public bool ShowTASConfiguration
        {
            get
            {
                return Connections.Current.HasTASConnection;
            }
        }

        [LocalizedDisplayName(typeof(FormStrings), "DefaultTVGroup")]
        [ListChoice("TVGroups", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidTVGroup")]
        public int? SelectedGroup { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "TVShowDatabase")]
        [ListChoice("TVShowDatabases", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidTVShowDatabase")]
        public int? TVShowProvider { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "MovieDatabase")]
        [ListChoice("MovieDatabases", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidMovieDatabase")]
        public int? MovieProvider { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "MusicDatabase")]
        [ListChoice("MusicDatabases", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidMusicDatabase")]
        public int? MusicProvider { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "StreamType")]
        public StreamTypeWithDescription StreamType { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "VLCPlayerEnableDeinterlacing")]
        public bool EnableDeinterlace { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "EnableAlbumPlayer")]
        public bool EnableAlbumPlayer { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "Skin")]
        [Required(ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidSkin")]
        [ListChoice("Skins", AllowNull = false, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidSkin")]
        public string Skin { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "Language")]
        [Required(ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidLanguage")]
        [ListChoice("Languages", AllowNull = false, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidLanguage")]
        public string Language { get; set; }

        public List<string> Platforms { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "DefaultMusicStreamingProfile")]
        public Dictionary<string, ProfileViewModel> AudioProfiles { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "DefaultMediaStreamingProfile")]
        public Dictionary<string, ProfileViewModel> VideoProfiles { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "DefaultTVStreamingProfile")]
        public Dictionary<string, ProfileViewModel> TvProfiles { get; set; }

        public SettingsViewModel()
        {
            Platforms = Configuration.StreamingPlatforms.GetPlatforms();
            AudioProfiles = new Dictionary<string, ProfileViewModel>();
            VideoProfiles = new Dictionary<string, ProfileViewModel>();
            TvProfiles = new Dictionary<string, ProfileViewModel>();

            foreach (string platform in Platforms)
            {
                AudioProfiles.Add(platform, new ProfileViewModel()
                {
                    DefaultProfile = Configuration.StreamingPlatforms.GetDefaultAudioProfileForPlatform(platform),
                    AvailableProfiles = GetProfilesForPlatform(platform, Connections.Current.MASStreamControl, StreamTarget.GetAudioTargets())
                });
                VideoProfiles.Add(platform, new ProfileViewModel()
                {
                    DefaultProfile = Configuration.StreamingPlatforms.GetDefaultVideoProfileForPlatform(platform),
                    AvailableProfiles = GetProfilesForPlatform(platform, Connections.Current.MASStreamControl, StreamTarget.GetVideoTargets())
                });
                TvProfiles.Add(platform, new ProfileViewModel() 
                {
                    DefaultProfile = Configuration.StreamingPlatforms.GetDefaultTvProfileForPlatform(platform),
                    AvailableProfiles = GetProfilesForPlatform(platform, Connections.Current.TASStreamControl, StreamTarget.GetVideoTargets())
                });
            }
        }

        public SettingsViewModel(Config.WebMediaPortal model) 
            : this()
        {
		    Skin = model.Skin;
            Language = model.DefaultLanguage;
            StreamType = (StreamTypeWithDescription)model.StreamType;
            EnableDeinterlace = model.EnableDeinterlace;
            EnableAlbumPlayer = model.EnableAlbumPlayer;
            SelectedGroup = model.DefaultGroup;

            if (ShowMASConfiguration)
            {
                var serviceDesc = Connections.Current.MAS.GetServiceDescription();
                MovieProvider = GetCurrentProvider(model.MovieProvider, serviceDesc.DefaultMovieLibrary);
                MusicProvider = GetCurrentProvider(model.MusicProvider, serviceDesc.DefaultMusicLibrary);
                TVShowProvider = GetCurrentProvider(model.TVShowProvider, serviceDesc.DefaultTvShowLibrary);
            }
        }

        public void SaveToConfiguration()
        {
            Configuration.WebMediaPortal.StreamType = (Config.StreamType)StreamType;
            Configuration.WebMediaPortal.EnableDeinterlace = EnableDeinterlace;
            Configuration.WebMediaPortal.EnableAlbumPlayer = EnableAlbumPlayer;
            Configuration.WebMediaPortal.DefaultGroup = SelectedGroup;
            Configuration.WebMediaPortal.TVShowProvider = TVShowProvider;
            Configuration.WebMediaPortal.MusicProvider = MusicProvider;
            Configuration.WebMediaPortal.MovieProvider = MovieProvider;
            Configuration.WebMediaPortal.Skin = Skin;
            Configuration.WebMediaPortal.DefaultLanguage = Language;

            foreach (string platform in Platforms)
            {
                Configuration.StreamingPlatforms.SetDefaultProfileForPlatform(StreamingProfileType.Audio, platform, AudioProfiles[platform].DefaultProfile);
                Configuration.StreamingPlatforms.SetDefaultProfileForPlatform(StreamingProfileType.Video, platform, VideoProfiles[platform].DefaultProfile);
                Configuration.StreamingPlatforms.SetDefaultProfileForPlatform(StreamingProfileType.Tv, platform, TvProfiles[platform].DefaultProfile);
            }
            
            Configuration.Save();
        }

        private List<String> GetProfilesForPlatform(string platform, IWebStreamingService service, IEnumerable<StreamTarget> targets)
        {
            List<string> profiles = new List<string>();

            Configuration.StreamingPlatforms.GetValidTargetsForPlatform(platform)
                .Intersect(targets.Select(x => x.Name))
                .ToList()
                .ForEach(target => profiles.AddRange(service.GetTranscoderProfilesForTarget(target)
                    .Select(x => x.Name)
                    .Where(x => !profiles.Contains(x))));

            profiles.Sort();
            return profiles;            
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

    public class ServiceAddressesViewModel
    {
        [LocalizedDisplayName(typeof(FormStrings), "MultiseatTAS")]
        public string TAS { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "MultiseatMAS")]
        public string MAS { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "LogOnUsername")]
        public string Username { get; set; }

        [LocalizedDisplayName(typeof(FormStrings), "LogOnPassword")]
        public string Password { get; set; }

        public ServiceAddressesViewModel()
        {
            TAS = Configuration.WebMediaPortal.TASUrl;
            MAS = Configuration.WebMediaPortal.MASUrl;
            Username = Configuration.WebMediaPortal.ServiceUsername;
            Password = Configuration.WebMediaPortal.ServicePassword;
        }
    }
}