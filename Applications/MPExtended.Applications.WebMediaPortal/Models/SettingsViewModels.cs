#region Copyright (C) 2012-2013 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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
using System.Web.Mvc;

using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Code.Composition;
using MPExtended.Applications.WebMediaPortal.Mvc;
using MPExtended.Applications.WebMediaPortal.Strings;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Config;
using MPExtended.Libraries.Service.Util;

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

  public enum MusicLayoutTypeWithDescription
  {
    [LocalizedDescription(typeof(FormStrings), "ArtistLayoutDescription")]
    Artist,

    [LocalizedDescription(typeof(FormStrings), "AlbumLayoutDescription")]
    Albums
  }

  public class PlatformViewModel
  {
    public string Name { get; set; }

    [LocalizedDisplayName(typeof(FormStrings), "DefaultMusicStreamingProfile")]
    [ListChoice("AudioProfiles", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidMusicProfile")]
    public string Audio { get; set; }

    [LocalizedDisplayName(typeof(FormStrings), "DefaultMediaStreamingProfile")]
    [ListChoice("VideoProfiles", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidMediaProfile")]
    public string Video { get; set; }

    [LocalizedDisplayName(typeof(FormStrings), "DefaultTVStreamingProfile")]
    [ListChoice("TvProfiles", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidTVProfile")]
    public string Tv { get; set; }

    public IEnumerable<SelectListItem> AudioProfiles { get { return GetProfileSelectList(StreamingProfileType.Audio); } }
    public IEnumerable<SelectListItem> VideoProfiles { get { return GetProfileSelectList(StreamingProfileType.Video); } }
    public IEnumerable<SelectListItem> TvProfiles { get { return GetProfileSelectList(StreamingProfileType.Tv); } }

    private IEnumerable<SelectListItem> GetProfileSelectList(StreamingProfileType type)
    {
      var service = type == StreamingProfileType.Tv ? Connections.Current.TASStreamControl : Connections.Current.MASStreamControl;
      var defaultProfile = Configuration.StreamingPlatforms.GetDefaultProfileForPlatform(type, Name);
      var targets = type == StreamingProfileType.Audio ? StreamTarget.GetAllTargets() : StreamTarget.GetVideoTargets();
      var supportedTargets = Configuration.StreamingPlatforms.GetValidTargetsForPlatform(Name).Intersect(targets.Select(x => x.Name));
      return ProfileModel.GetProfilesForTargets(service, supportedTargets)
          .Select(x => new SelectListItem() { Text = x.Name, Value = x.Name, Selected = x.Name == defaultProfile })
          .OrderBy(x => x.Text);
    }
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

    public IEnumerable<SelectListItem> PictureDatabases
    {
      get
      {
        return Connections.Current.MAS.GetServiceDescription().AvailablePictureLibraries
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

    [LocalizedDisplayName(typeof(FormStrings), "PictureDatabase")]
    [ListChoice("PictureDatabases", AllowNull = true, ErrorMessageResourceType = typeof(FormStrings), ErrorMessageResourceName = "ErrorNoValidPictureDatabase")]
    public int? PicturesProvider { get; set; }

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

    [LocalizedDisplayName(typeof(FormStrings), "MusicLayout")]
    public MusicLayoutTypeWithDescription MusicLayout { get; set; }


    public List<PlatformViewModel> Platforms { get; set; }

    public SettingsViewModel()
    {
    }

    public SettingsViewModel(Config.WebMediaPortal model)
    {
      Platforms = Configuration.StreamingPlatforms
          .Select(x => new PlatformViewModel() { Name = x.Name })
          .ToList();

      Skin = model.Skin;
      Language = model.DefaultLanguage;
      StreamType = (StreamTypeWithDescription)model.StreamType;
      EnableDeinterlace = model.EnableDeinterlace;
      EnableAlbumPlayer = model.EnableAlbumPlayer;
      SelectedGroup = model.DefaultGroup;
      MusicLayout = (MusicLayoutTypeWithDescription)model.MusicLayout;

      if (ShowMASConfiguration)
      {
        var serviceDesc = Connections.Current.MAS.GetServiceDescription();
        MovieProvider = GetCurrentProvider(model.MovieProvider, serviceDesc.DefaultMovieLibrary);
        MusicProvider = GetCurrentProvider(model.MusicProvider, serviceDesc.DefaultMusicLibrary);
        TVShowProvider = GetCurrentProvider(model.TVShowProvider, serviceDesc.DefaultTvShowLibrary);
        PicturesProvider = GetCurrentProvider(model.PicturesProvider, serviceDesc.DefaultPictureLibrary);
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
      Configuration.WebMediaPortal.PicturesProvider = PicturesProvider;
      Configuration.WebMediaPortal.Skin = Skin;
      Configuration.WebMediaPortal.DefaultLanguage = Language;
      Configuration.WebMediaPortal.MusicLayout = (Config.MusicLayoutType)MusicLayout;

      var profileTypes = new Dictionary<StreamingProfileType, Func<PlatformViewModel, string>>()
            {
                { StreamingProfileType.Audio, x => x.Audio },
                { StreamingProfileType.Video, x => x.Video },
                { StreamingProfileType.Tv, x => x.Tv },
            };

      foreach (var platform in Platforms)
      {
        foreach (var pt in profileTypes)
        {
          if (!String.IsNullOrEmpty(pt.Value.Invoke(platform)))
            Configuration.StreamingPlatforms.SetDefaultProfileForPlatform(pt.Key, platform.Name, pt.Value.Invoke(platform));
        }
      }

      Configuration.Save();
    }

    private int GetCurrentProvider(int? setting, int defaultValue)
    {
      return setting == null ? defaultValue : setting.Value;
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