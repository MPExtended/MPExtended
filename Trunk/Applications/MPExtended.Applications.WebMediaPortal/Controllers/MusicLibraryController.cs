#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.ServiceModel;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Libraries.ServiceLib;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
     [Authorize]
    public class MusicLibraryController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                var artistList = MPEServices.NetPipeMediaAccessService.GetAllArtists();
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

        public ActionResult Albums(string artist)
        {
            try
            {
                var albumList = MPEServices.NetPipeMediaAccessService.GetAlbumsByArtist(artist);
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

        public ActionResult Tracks(string album,string artist)
        {
            try
            {
                var trackList = MPEServices.NetPipeMediaAccessService.GetSongsOfAlbum(album, artist, SortBy.Name, OrderBy.Asc);
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

        public ActionResult Play(int track)
        {
            try
            {
                var musicTrack = MPEServices.NetPipeMediaAccessService.GetMusicTrack(track);
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
        public ActionResult Image(string album, string artist)
        {
            try
            {
                byte[] image = System.IO.File.ReadAllBytes(MPEServices.NetPipeMediaAccessService.GetAlbum(artist, album).CoverPathL);
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
