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
using MPExtended.Libraries.Client;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class MusicLibraryController : BaseController
    {
        public ActionResult Index()
        {
            var artistList = MPEServices.MAS.GetAllMusicArtistsDetailed(Settings.ActiveSettings.MusicProvider);
            if (artistList != null)
            {
                return View(artistList);
            }
            return null;
        }

        public ActionResult Albums(string artist)
        {
            var artistObj = MPEServices.MAS.GetMusicArtistDetailedById(Settings.ActiveSettings.MusicProvider, artist);
            var albumList = MPEServices.MAS.GetMusicAlbumsBasicForArtist(Settings.ActiveSettings.MusicProvider, artist);
            return View(new ArtistViewModel()
            {
                Artist = artistObj,
                Albums = albumList
            });
        }

        public ActionResult Album(string album)
        {
            var albumObj = MPEServices.MAS.GetMusicAlbumBasicById(Settings.ActiveSettings.MusicProvider, album);
            var trackList = MPEServices.MAS.GetMusicTracksDetailedForAlbum(Settings.ActiveSettings.MusicProvider, album, WebSortField.Title, WebSortOrder.Asc);

            return View(new AlbumViewModel()
            {
                Album = albumObj,
                Tracks = trackList
            });
        }

        public ActionResult AlbumImage(string album, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.MusicAlbum, album, WebArtworkType.Cover, "images/default/album.png");
        }
    }
}