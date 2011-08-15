using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Music;


namespace MPExtended.PlugIns.MAS.MPMusic
{
    [Export(typeof(IMusicLibrary))]
    [ExportMetadata("Database", "MPMyMusic")]
    public class MPMusic : IMusicLibrary
    {
        private MPMusicDB _db = null;
        public MPMusic()
        {
            _db = new MPMusicDB();
        }

        public IList<WebMusicTrackBasic> GetAllTracks()
        {
            return _db.GetAllMusicTracks();
        }

        public IList<WebMusicAlbumBasic> GetAllAlbums()
        {
            return _db.GetAllAlbums();
        }

        public IList<WebMusicArtistBasic> GetAllArtists()
        {
            throw new NotImplementedException();
        }

        public WebMusicTrackBasic GetTrackBasicById(string trackId)
        {
            throw new NotImplementedException();
        }

        public WebMusicAlbumBasic GetAlbumBasicById(string albumId)
        {
            throw new NotImplementedException();
        }

        public WebMusicArtistBasic GetArtistBasicById(string artistId)
        {
            throw new NotImplementedException();
        }

        public IList<WebMusicTrackBasic> GetTracksByAlbumId(string albumId)
        {
            throw new NotImplementedException();
        }

        public IList<string> GetAllGenres()
        {
            throw new NotImplementedException();
        }

        public System.IO.DirectoryInfo GetSourceRootDirectory()
        {
            throw new NotImplementedException();
        }
    }
}
