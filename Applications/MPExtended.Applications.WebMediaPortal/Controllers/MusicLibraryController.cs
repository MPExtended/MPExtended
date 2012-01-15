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
using MPExtended.Libraries.Client;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [Authorize]
    public class MusicLibraryController : BaseController
    {
        public ActionResult Index()
        {
            var artistList = MPEServices.MAS.GetAllMusicArtistsBasic(Settings.ActiveSettings.MusicProvider);
            if (artistList != null)
            {
                return View(artistList);
            }
            return null;
        }

        public ActionResult Albums(string artist)
        {
            var albumList = MPEServices.MAS.GetMusicAlbumsBasicForArtist(Settings.ActiveSettings.MusicProvider, artist);
            if (albumList != null)
            {
                return View(albumList);
            }
            return null;
        }

        public ActionResult Tracks(string album)
        {
            var trackList = MPEServices.MAS.GetMusicTracksBasicForAlbum(Settings.ActiveSettings.MusicProvider, album, SortBy.Title, OrderBy.Asc);
            if (trackList != null)
            {
                return View(trackList);
            }
            return null;
        }

        public ActionResult Play(string track)
        {
            var musicTrack = MPEServices.MAS.GetMusicTrackDetailedById(Settings.ActiveSettings.MusicProvider, track);
            if (musicTrack != null)
            {
                return View(musicTrack);
            }
            return null;
        }

        public ActionResult AlbumImage(string album)
        {
            var image = MPEServices.MASStream.GetArtwork(WebStreamMediaType.MusicAlbum, Settings.ActiveSettings.MusicProvider, album, WebArtworkType.Cover, 0);
            return File(image, "image/jpg");
        }
    }
}