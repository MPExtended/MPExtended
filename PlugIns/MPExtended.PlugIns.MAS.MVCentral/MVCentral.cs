#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Data.SQLite;
using System.Linq;
using System.IO;
using System.Text;
using System.ComponentModel.Composition;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Music;

namespace MPExtended.PlugIns.MAS.MVCentral
{
    [Export(typeof(IMusicLibrary))]
    [ExportMetadata("Name", "mvCentral")]
    [ExportMetadata("Id", 12)]
    public partial class MVCentral : Database, IMusicLibrary
    {
        private class Setting
        {
            public string Key { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string Type { get; set; }
        }

        public bool Supported { get; private set; }
        private bool hasAlbums = true;

        [ImportingConstructor]
        public MVCentral(IPluginData data)
        {
            var config = data.GetConfiguration("mvCentral");
            if (config.ContainsKey("database") && File.Exists(config["database"]))
            {
                DatabasePath = config["database"];
                Supported = true;
                ReadSettings();
            }
            else
            {
                Supported = false;
            }
        }

        private void ReadSettings()
        {
            string sql = "SELECT s.key, s.name, s.value, s.type " +
                         "FROM settings s";
            var settings = new LazyQuery<Setting>(this, sql, new List<SQLFieldMapping>()
            {
                new SQLFieldMapping("s", "key", "Key", DataReaders.ReadString),
                new SQLFieldMapping("s", "name", "Name", DataReaders.ReadString),
                new SQLFieldMapping("s", "value", "Value", DataReaders.ReadString),
                new SQLFieldMapping("s", "type", "Type", DataReaders.ReadString),
            });

            var disableAlbumSupportList = settings.Where(s => s.Key == "disable_album_support").ToList();
            hasAlbums = disableAlbumSupportList.Any() ? disableAlbumSupportList.First().Value != "True" : true;
        }

        [MergeListReader]
        private WebArtworkDetailed ArtworkReader(SQLiteDataReader reader, int index, object param)
        {
            string path = (string)DataReaders.ReadString(reader, index);
            if (String.IsNullOrEmpty(path))
            {
                return null;
            }
            return new WebArtworkDetailed()
            {
                Type = (WebFileType)param,
                Path = path,
                Offset = 0,
                Filetype = Path.GetExtension(path).Substring(1),
                Rating = 1,
                Id = path.GetHashCode().ToString()
            };
        }

        #region Tracks
        public IEnumerable<WebMusicTrackBasic> GetAllTracks()
        {
            return LoadAllTracks<WebMusicTrackBasic>();
        }

        public IEnumerable<WebMusicTrackDetailed> GetAllTracksDetailed()
        {
            return LoadAllTracks<WebMusicTrackDetailed>();
        }

        public WebMusicTrackBasic GetTrackBasicById(string trackId)
        {
            return LoadAllTracks<WebMusicTrackBasic>().First(x => x.Id == trackId);
        }

        public WebMusicTrackDetailed GetTrackDetailedById(string trackId)
        {
            return LoadAllTracks<WebMusicTrackDetailed>().First(x => x.Id == trackId);
        }

        protected LazyQuery<T> LoadAllTracks<T>() where T : WebMusicTrackBasic, new()
        {
            Dictionary<string, WebMusicArtistBasic> artists = GetAllArtists().ToDictionary(x => x.Id, x => x);

            // Unavailable fields: TrackNumber, Year, Genres, 
            string sql = "SELECT t.id AS track_id, t.date_added, t.track, t.rating, t.artfullpath, a.id AS album_id, a.album, p.id AS artist_id, p.artist, " +
                            "GROUP_CONCAT(l.fullpath, '|') AS path, MIN(l.duration) AS duration " +
                         "FROM track_info t " +
                         "LEFT JOIN local_media__track_info lt ON lt.track_info_id = t.id " +
                         "LEFT JOIN local_media l ON l.id = lt.local_media_id " +
                         "LEFT JOIN album_info__track_info at ON at.track_info_id = t.id " +
                         "LEFT JOIN album_info a ON at.album_info_id = a.id " +
                         "LEFT JOIN artist_info__track_info pt ON pt.track_info_id = t.id " +
                         "LEFT JOIN artist_info p ON pt.artist_info_id = p.id " +
                         "WHERE %where " +
                         "GROUP BY t.id, t.date_added, t.track, t.rating, a.id, a.album, p.id, p.artist ";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>()
            {
                new SQLFieldMapping("", "track_id", "Id", DataReaders.ReadString),
                new SQLFieldMapping("", "path", "Path", DataReaders.ReadPipeList),
                new SQLFieldMapping("t", "date_added", "DateAdded", DataReaders.ReadDateTime),
                new SQLFieldMapping("", "artist_id", "ArtistId", DataReaders.ReadStringAsList),
                new SQLFieldMapping("p", "artist", "Artist", DataReaders.ReadStringAsList),
                new SQLFieldMapping("", "album_id", "AlbumId", DataReaders.ReadString),
                new SQLFieldMapping("a", "album", "Album", DataReaders.ReadString),
                new SQLFieldMapping("t", "track", "Title", DataReaders.ReadString),
                new SQLFieldMapping("l", "duration", "Duration", PlayTimeReader),
                new SQLFieldMapping("t", "rating", "Rating", DataReaders.ReadFloat),
                new SQLFieldMapping("t", "artfullpath", "Artwork", ArtworkReader, WebFileType.Cover)
            }, delegate(T item)
            {
                if (item is WebMusicTrackDetailed)
                {
                    WebMusicTrackDetailed det = item as WebMusicTrackDetailed;
                    det.Artists = det.ArtistId.Where(x => artists.ContainsKey(x)).Select(x => artists[x]).ToList();
                }
                return item;
            });
        }

        private object PlayTimeReader(SQLiteDataReader reader, int idx)
        {
            return reader.ReadInt32(idx) / 1000;
        }
        #endregion

        #region Albums
        public IEnumerable<WebMusicAlbumBasic> GetAllAlbums()
        {
            return LoadAllAlbums<WebMusicAlbumBasic>();
        }

        public WebMusicAlbumBasic GetAlbumBasicById(string albumId)
        {
            return LoadAllAlbums<WebMusicAlbumBasic>().First(x => x.Id == albumId);
        }

        protected LazyQuery<T> LoadAllAlbums<T>() where T : WebMusicAlbumBasic, new()
        {
            // Unavailable fields: Genres, Composer

            string sql = "SELECT a.id, a.album, a.date_added, a.yearreleased, a.rating, a.artfullpath, " +
                            "MIN(p.id) AS albumartist_id, MIN(p.artist) AS albumartist_name, " +  // FIXME: this is incorrect
                            "GROUP_CONCAT(p.id, '|') AS artist_id, GROUP_CONCAT(p.artist, '|') AS artist_name " +
                         "FROM album_info a " +
                         "LEFT JOIN album_info__track_info at ON at.album_info_id = a.id " +
                         "LEFT JOIN artist_info__track_info pt ON pt.track_info_id = at.track_info_id " +
                         "LEFT JOIN artist_info p ON pt.artist_info_id = p.id " +
                         "GROUP BY a.id, a.album, a.date_added, a.yearreleased, a.rating, a.artfullpath " +
                         "HAVING COUNT(p.id) > 0 AND %where "; // We abuse SQL a bit here due to the inavailability of a album_info__artist_info table
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>()
            {
                new SQLFieldMapping("a", "id", "Id", DataReaders.ReadString),
                new SQLFieldMapping("a", "album", "Title", DataReaders.ReadString),
                new SQLFieldMapping("", "albumartist_id", "AlbumArtistId", AlbumArtistReader),
                new SQLFieldMapping("", "albumartist_name", "AlbumArtist", AlbumArtistReader),
                new SQLFieldMapping("", "artist_id", "ArtistsId", DataReaders.ReadPipeList),
                new SQLFieldMapping("", "artist_name", "Artists", DataReaders.ReadPipeList),
                new SQLFieldMapping("a", "date_added", "DateAdded", DataReaders.ReadDateTime),
                new SQLFieldMapping("a", "yearreleased", "Year", DataReaders.ReadInt32),
                new SQLFieldMapping("a", "rating", "Rating", DataReaders.ReadFloat),
                new SQLFieldMapping("a", "artfullpath", "Artwork", ArtworkReader, WebFileType.Cover),
            });
        }

        [AllowSQLCompare("('|' || %field || '|') LIKE '%|' || %prepared || '|%'")]
        private object AlbumArtistReader(SQLiteDataReader reader, int idx)
        {
            return ((List<string>)reader.ReadPipeList(idx)).FirstOrDefault();
        }
        #endregion

        #region Artists
        public IEnumerable<WebMusicArtistBasic> GetAllArtists()
        {
            return LoadAllArtists<WebMusicArtistBasic>();
        }

        public IEnumerable<WebMusicArtistDetailed> GetAllArtistsDetailed()
        {
            return LoadAllArtists<WebMusicArtistDetailed>();
        }

        public WebMusicArtistBasic GetArtistBasicById(string artistId)
        {
            return LoadAllArtists<WebMusicArtistBasic>().First(x => x.Id == artistId);
        }

        public WebMusicArtistDetailed GetArtistDetailedById(string artistId)
        {
            return LoadAllArtists<WebMusicArtistDetailed>().First(x => x.Id == artistId);
        }

        protected LazyQuery<T> LoadAllArtists<T>() where T : WebMusicArtistBasic, new()
        {
            string sql = "SELECT a.id, a.artist, a.tones, a.styles, a.biocontent, a.artfullpath " +
                         "FROM artist_info a " +
                         "WHERE %where ";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>()
            {
                new SQLFieldMapping("a", "id", "Id", DataReaders.ReadString),
                new SQLFieldMapping("a", "artist", "Title", DataReaders.ReadString),
                new SQLFieldMapping("a", "tones", "Tones", DataReaders.ReadString),
                new SQLFieldMapping("a", "styles", "Styles", DataReaders.ReadString),
                new SQLFieldMapping("a", "biocontent", "Biography", DataReaders.ReadString),
                new SQLFieldMapping("a", "artfullpath", "Artwork", ArtworkReader, WebFileType.Cover),
            }, delegate(T item)
            {
                item.HasAlbums = hasAlbums;
                return item;
            });
        }
        #endregion

        public IEnumerable<WebGenre> GetAllGenres()
        {
            return new List<WebGenre>();
        }

        public IEnumerable<WebCategory> GetAllCategories()
        {
            return new List<WebCategory>();
        }

        public WebFileInfo GetFileInfo(string path)
        {
            return new WebFileInfo(PathUtil.StripFileProtocolPrefix(path));
        }

        public Stream GetFile(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public WebDictionary<string> GetExternalMediaInfo(WebMediaType type, string id)
        {
            var dict = new WebDictionary<string>();
            dict.Add("Id", id);
            if (type == WebMediaType.MusicAlbum) dict.Add("Type", "mvcentral album");
            if (type == WebMediaType.MusicArtist) dict.Add("Type", "mvcentral artist");
            if (type == WebMediaType.MusicTrack) dict.Add("Type", "mvcentral track");
            return dict;
        }
    }
}
