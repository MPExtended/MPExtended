using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.ServiceModel;
using WebMediaPortal.Code;
using WebMediaPortal.Services;
using MPExtended.Services.MediaAccessService.Interfaces;
namespace WebMediaPortal.Controllers
{
     [Authorize]
    public class MusicLibraryController : Controller
    {
      

        public ActionResult Index()
        {
            try
            {
                var artistList = WebServices.MediaAccessService.GetAllArtists();
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
                var albumList = WebServices.MediaAccessService.GetAlbumsByArtist(artist);
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
                var trackList = WebServices.MediaAccessService.GetSongsOfAlbum(album, artist, SortBy.Name, OrderBy.Asc);
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
                var musicTrack = WebServices.MediaAccessService.GetMusicTrack(track);
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
                byte[] image = System.IO.File.ReadAllBytes(WebServices.MediaAccessService.GetAlbum(artist, album).CoverPathL);
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
