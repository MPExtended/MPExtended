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
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
  [ServiceAuthorize]
  public class MusicLibraryController : BaseController
  {
    public ActionResult Index()
    {
      if (Settings.ActiveSettings.MusicLayout == Libraries.Service.Config.MusicLayoutType.Artist)
      {
        return Artist();
      }
      else
      {
        return RedirectToAction("Albums", "MusicLibrary", new { });
      }
    }

    public ActionResult Artist()
    {
      var artistList = Connections.Current.MAS.GetMusicArtistsDetailed(Settings.ActiveSettings.MusicProvider);
      if (artistList == null)
        return new HttpNotFoundResult();
      return View("Index", artistList.Where(x => !String.IsNullOrEmpty(x.Title)));
    }

    public ActionResult Albums(string artist)
    {
      WebMusicArtistDetailed artistObj = new WebMusicArtistDetailed();
      IList<WebMusicAlbumBasic> albumList;

      if (string.IsNullOrEmpty(artist))
        albumList = Connections.Current.MAS.GetMusicAlbumsBasic(Settings.ActiveSettings.MusicProvider);
      else
      {
        artistObj = Connections.Current.MAS.GetMusicArtistDetailedById(Settings.ActiveSettings.MusicProvider, artist);
        albumList = Connections.Current.MAS.GetMusicAlbumsBasicForArtist(Settings.ActiveSettings.MusicProvider, artist);
      }

      return View(new ArtistViewModel()
      {
        Artist = artistObj,
        Albums = albumList.Where(x => !String.IsNullOrEmpty(x.Title))
      });
    }

    public ActionResult Album(string album, string codec = null)
    {
      var albumObj = Connections.Current.MAS.GetMusicAlbumBasicById(Settings.ActiveSettings.MusicProvider, album);
      if (albumObj == null)
        return new HttpNotFoundResult();
      var trackList = Connections.Current.MAS.GetMusicTracksDetailedForAlbum(Settings.ActiveSettings.MusicProvider, album, null, WebSortField.Title, WebSortOrder.Asc);
      var model = new AlbumViewModel()
      {
        Album = albumObj,
        Tracks = trackList.Where(x => !String.IsNullOrEmpty(x.Title) && string.IsNullOrEmpty(codec) ? true : x.Codec == codec)
      };

      return View(AlbumPlayerViewModel.EnableAlbumPlayerForUserAgent(Request.UserAgent) ? "AlbumPlayer" : "Album", model);
    }

    public ActionResult AlbumImage(string album, int width = 0, int height = 0)
    {
      return Images.ReturnFromService(WebMediaType.MusicAlbum, album, WebFileType.Cover, width, height, "Images/default/album.png");
    }

    public ActionResult ArtistImage(string artist, int width = 0, int height = 0)
    {
      return Images.ReturnFromService(WebMediaType.MusicArtist, artist, WebFileType.Cover, width, height, "Images/default/artist.png");
    }

    public ActionResult TrackImage(string track, int width = 0, int height = 0)
    {
      return Images.ReturnFromService(WebMediaType.MusicTrack, track, WebFileType.Cover, width, height, "Images/default/track.png");
    }

    public ActionResult Track(string track)
    {
      var trackObj = Connections.Current.MAS.GetMusicTrackDetailedById(Settings.ActiveSettings.MusicProvider, track);
      if (trackObj == null)
        return new HttpNotFoundResult();
      return View(trackObj);
    }

    public ActionResult ArtistFanart(string artist, int width = 0, int height = 0, int num = -1)
    {
      return Images.ReturnFromService(WebMediaType.MusicArtist, artist, WebFileType.Backdrop, width, height, "Images/default/artist-fanart.png", num);
    }

    public ActionResult ArtistLogo(string artist, int width = 0, int height = 0)
    {
      return Images.ReturnFromService(WebMediaType.MusicArtist, artist, WebFileType.Logo, width, height, "Images/default/artist-logo.png");
    }

  }
}