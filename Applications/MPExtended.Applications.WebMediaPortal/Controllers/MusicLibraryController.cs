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
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class MusicLibraryController : BaseController
    {
        public ActionResult Index()
        {
            var artistList = Connections.Current.MAS.GetMusicArtistsDetailed(Settings.ActiveSettings.MusicProvider);
            if (artistList == null)
                return new HttpNotFoundResult();
            return View(artistList);
        }

        public ActionResult Albums(string artist)
        {
            var artistObj = Connections.Current.MAS.GetMusicArtistDetailedById(Settings.ActiveSettings.MusicProvider, artist);
            var albumList = Connections.Current.MAS.GetMusicAlbumsBasicForArtist(Settings.ActiveSettings.MusicProvider, artist);
            return View(new ArtistViewModel()
            {
                Artist = artistObj,
                Albums = albumList
            });
        }

        public ActionResult Album(string album)
        {
            var albumObj = Connections.Current.MAS.GetMusicAlbumBasicById(Settings.ActiveSettings.MusicProvider, album);
            if (albumObj == null)
                return new HttpNotFoundResult();
            var trackList = Connections.Current.MAS.GetMusicTracksDetailedForAlbum(Settings.ActiveSettings.MusicProvider, album, WebSortField.Title, WebSortOrder.Asc);
            var model = new AlbumViewModel()
            {
                Album = albumObj,
                Tracks = trackList
            };

            if (Settings.ActiveSettings.EnableAlbumPlayer)
            {
                return View("AlbumPlayer", model);
            }
            else
            {
                return View("Album", model);
            }
        }

        public ActionResult AlbumImage(string album, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.MusicAlbum, album, WebFileType.Cover, "images/default/album.png");
        }

        public ActionResult ArtistImage(string artist, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.MusicArtist, artist, WebFileType.Cover, "images/default/artist.png");
        }

        public ActionResult TrackImage(string track, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.MusicTrack, track, WebFileType.Cover, "images/default/track.png");
        }

        public ActionResult Track(string track)
        {
            var trackObj = Connections.Current.MAS.GetMusicTrackDetailedById(Settings.ActiveSettings.MusicProvider, track);
            if (trackObj == null)
                return new HttpNotFoundResult();
            return View(trackObj);
        }
    }
}