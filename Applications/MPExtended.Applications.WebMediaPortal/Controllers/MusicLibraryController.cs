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
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
     [Authorize]
    public class MusicLibraryController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                var artistList = MPEServices.MAS.GetAllMusicArtistsBasic(Settings.ActiveSettings.MusicProvider);
                if (artistList != null)
                {
                    return View(artistList);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MusicLibrary.Index", ex);
            }
            return View("Error");

        }

        public ActionResult Albums(string id)
        {
            try
            {
                var albumList = MPEServices.MAS.GetMusicAlbumsBasicForArtist(Settings.ActiveSettings.MusicProvider, id);
                if (albumList != null)
                {
                    return View(albumList);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MusicLibrary.Albums", ex);
            }
            return View("Error");
        }

        public ActionResult Tracks(string id)
        {
            try
            {
                var trackList = MPEServices.MAS.GetMusicTracksBasicForAlbum(Settings.ActiveSettings.MusicProvider, id, SortBy.Title, OrderBy.Asc);
                if (trackList != null)
                {
                    return View(trackList);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MusicLibrary.Tracks", ex);
            }
            return View("Error");
        }

        public ActionResult Play(string id)
        {
            try
            {
                var musicTrack = MPEServices.MAS.GetMusicTrackDetailedById(Settings.ActiveSettings.MusicProvider, id);
                if (musicTrack != null)
                {
                    return View(musicTrack);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MusicLibrary.Play", ex);
            }
            return View("Error");
        }
        public ActionResult AlbumImage(string id)
        {
            try
            {
                var image = MPEServices.MASStream.GetArtwork(WebStreamMediaType.MusicAlbum, Settings.ActiveSettings.MusicProvider, id, WebArtworkType.Cover, 0);
                return File(image, "image/jpg");
            }
            catch (Exception ex)
            {
                Log.Error("Exception in MusicLibrary.Image", ex);
            }
            return null;
        }

    }
}
