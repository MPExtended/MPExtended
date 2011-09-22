using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;
using System.Data.SQLite;
namespace MPExtended.PlugIns.MAS.XBMCMusic
{
    [Export(typeof(IMusicLibrary))]
    [ExportMetadata("Database", "XBMCMusic")]
    [ExportMetadata("Version", "1.0.0.0")]
    public class XBMCMusic : Database, IMusicLibrary
    {
        XBMCMusicDB _db = new XBMCMusicDB();

        public IEnumerable<WebMusicTrackBasic> GetAllTracks()
        {
            string sql = "SELECT m.id, m.date_added, m.backdropfullpath, m.alternatecovers, m.genres, m.score, m.runtime, m.title, m.year, " +
                           "GROUP_CONCAT(l.fullpath, '|') AS path, " +
                           "m.directors, m.writers, m.actors, m.summary, m.language " +
                        "FROM movie_info m " +
                        "INNER JOIN local_media__movie_info AS i ON i.movie_info_id = m.id " +
                        "INNER JOIN local_media AS l ON l.id = i.local_media_id AND l.ignored = 0 " +
                        "WHERE %where " +
                        "GROUP BY m.id, m.date_added, m.backdropfullpath, m.coverfullpath, m.genres, m.score, m.runtime, m.title, m.year, " +
                           "m.directors, m.writers, m.actors, m.summary, m.language " +
                        "%order";
            return new LazyQuery<WebMusicTrackBasic>(this, sql, new List<SQLFieldMapping>() {
                new SQLFieldMapping("m", "id", "Id", DataReaders.ReadIntAsString),
                new SQLFieldMapping("m", "date_added", "DateAdded", DataReaders.ReadDateTime),
                new SQLFieldMapping("m", "backdropfullpath", "BackdropPath", DataReaders.ReadStringAsList),
                new SQLFieldMapping("m", "alternatecovers", "CoverPath", DataReaders.ReadPipeList),
                new SQLFieldMapping("m", "genres", "Genres", DataReaders.ReadPipeList),
                new SQLFieldMapping("m", "score", "Rating", DataReaders.ReadFloat),
                new SQLFieldMapping("m", "runtime", "Runtime", DataReaders.ReadInt32),
                new SQLFieldMapping("m", "title", "Title", DataReaders.ReadString),
                new SQLFieldMapping("m", "year", "Year", DataReaders.ReadInt32),
                new SQLFieldMapping("", "path", "Path", DataReaders.ReadPipeList),
                new SQLFieldMapping("m", "directors", "Directors", DataReaders.ReadPipeList),
                new SQLFieldMapping("m", "writers", "Writers", DataReaders.ReadPipeList),
                new SQLFieldMapping("m", "actors", "Actors", DataReaders.ReadPipeList),
                new SQLFieldMapping("m", "summary", "Summary", DataReaders.ReadString),
                new SQLFieldMapping("m", "language", "Language", DataReaders.ReadString)
            });
        }

        public IEnumerable<WebMusicAlbumBasic> GetAllAlbums()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WebMusicArtistBasic> GetAllArtists()
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

        public IEnumerable<WebMusicTrackBasic> GetTracksByAlbumId(string albumId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WebGenre> GetAllGenres()
        {
            string sql = "SELECT DISTINCT strGenre FROM genre";
            return ReadList<IEnumerable<string>>(sql, delegate(SQLiteDataReader reader)
            {
                return reader.ReadPipeList(0);
            })
                .SelectMany(x => x)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new WebGenre() { Name = x });
        }

        public System.IO.DirectoryInfo GetSourceRootDirectory()
        {
            throw new NotImplementedException();
        }

        IEnumerable<WebMusicTrackBasic> IMusicLibrary.GetAllTracks()
        {
            throw new NotImplementedException();
        }

        IEnumerable<WebMusicAlbumBasic> IMusicLibrary.GetAllAlbums()
        {
            throw new NotImplementedException();
        }

        IEnumerable<WebMusicArtistBasic> IMusicLibrary.GetAllArtists()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WebMusicTrackDetailed> GetAllTracksDetailed()
        {
            throw new NotImplementedException();
        }

        public WebMusicTrackDetailed GetTrackDetailedById(string trackId)
        {
            throw new NotImplementedException();
        }

        IEnumerable<WebGenre> IMusicLibrary.GetAllGenres()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WebCategory> GetAllCategories()
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream GetCover(string albumId, int offset)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream GetBackdrop(string albumId, int offset)
        {
            throw new NotImplementedException();
        }

        public bool IsLocalFile(string path)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream GetFile(string path)
        {
            throw new NotImplementedException();
        }
    }
}
