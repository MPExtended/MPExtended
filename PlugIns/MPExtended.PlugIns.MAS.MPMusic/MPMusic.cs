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
using System.ComponentModel.Composition;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Music;

namespace MPExtended.PlugIns.MAS.MPMusic
{
    [Export(typeof(IMusicLibrary))]
    [ExportMetadata("Name", "MP MyMusic")]
    [ExportMetadata("Id", 4)]
    public class MPMusic : Database, IMusicLibrary
    {
        private Dictionary<string, string> configuration;
        public bool Supported { get; set; }

        [ImportingConstructor]
        public MPMusic(IPluginData data)
        {
            configuration = data.GetConfiguration("MP MyMusic");
            DatabasePath = configuration["database"];
            Supported = File.Exists(DatabasePath);
        }

        #region Tracks
        private LazyQuery<T> LoadAllTracks<T>() where T : WebMusicTrackBasic, new()
        {
            Dictionary<string, WebMusicArtistBasic> artists = GetAllArtists().ToDictionary(x => x.Id, x => x);

            string sql = "SELECT idTrack, strAlbumArtist, strAlbum, strArtist, iTrack, strTitle, strPath, iDuration, iYear, strGenre, iRating " +
                         "FROM tracks t " + 
                         "WHERE %where";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>() {
                new SQLFieldMapping("t", "idTrack", "Id", DataReaders.ReadIntAsString),
                new SQLFieldMapping("t", "strArtist", "Artist", DataReaders.ReadPipeList),
                new SQLFieldMapping("t", "strArtist", "ArtistId", DataReaders.ReadPipeList),
                new SQLFieldMapping("t", "strAlbum", "Album", DataReaders.ReadString),
                new SQLFieldMapping("t", "strAlbum", "AlbumId", AlbumIdReader),
                new SQLFieldMapping("t", "strTitle", "Title", DataReaders.ReadString),
                new SQLFieldMapping("t", "iTrack", "TrackNumber", DataReaders.ReadInt32),
                new SQLFieldMapping("t", "strPath", "Path", DataReaders.ReadStringAsList),
                new SQLFieldMapping("t", "strGenre", "Genres", DataReaders.ReadPipeList),
                new SQLFieldMapping("t", "iYear", "Year", DataReaders.ReadInt32),
                new SQLFieldMapping("t", "iDuration", "Duration", DataReaders.ReadInt32),
                new SQLFieldMapping("t", "iRating", "Rating", DataReaders.ReadInt32),
                new SQLFieldMapping("t", "dateAdded", "DateAdded", DataReaders.ReadDateTime)
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
            return LoadAllTracks<WebMusicTrackBasic>().Where(x => x.Id == trackId).First();
        }

        public WebMusicTrackDetailed GetTrackDetailedById(string trackId)
        {
            return LoadAllTracks<WebMusicTrackDetailed>().Where(x => x.Id == trackId).First();
        }
        #endregion

        #region Albums
        public IEnumerable<WebMusicAlbumBasic> GetAllAlbums()
        {
            string sql = "SELECT DISTINCT t.strAlbumArtist, t.strAlbum, " +
                            "GROUP_CONCAT(t.strArtist, '|') AS artists, GROUP_CONCAT(t.strGenre, '|') AS genre, GROUP_CONCAT(t.strComposer, '|') AS composer, " +
                            "MIN(t.dateAdded) AS date, " +
                            "CASE WHEN i.iYear ISNULL THEN MIN(t.iYear) ELSE i.iYear END AS year, " +
                            "MAX(i.iRating) AS rating " + 
                         "FROM tracks t " +
                         "LEFT JOIN albuminfo i ON t.strAlbum = i.strAlbum AND t.strArtist LIKE '%' || i.strArtist || '%' " +
                         "WHERE %where " + 
                         "GROUP BY t.strAlbum, t.strAlbumArtist ";
            return new LazyQuery<WebMusicAlbumBasic>(this, sql, new List<SQLFieldMapping>()
            {
                new SQLFieldMapping("t", "strAlbum", "Id", AlbumIdReader),
                new SQLFieldMapping("t", "strAlbum", "Title", DataReaders.ReadString),
                new SQLFieldMapping("t", "strAlbumArtist", "AlbumArtist", DataReaders.ReadPipeListAsString),
                new SQLFieldMapping("t", "strAlbumArtist", "AlbumArtistId", DataReaders.ReadPipeListAsString),
                new SQLFieldMapping("t", "artists", "Artists", DataReaders.ReadPipeList),
                new SQLFieldMapping("t", "artists", "ArtistsId", DataReaders.ReadPipeList),
                new SQLFieldMapping("t", "genre", "Genres", DataReaders.ReadPipeList),
                new SQLFieldMapping("t", "composer", "Composer", DataReaders.ReadPipeList),
                new SQLFieldMapping("t", "date", "DateAdded", DataReaders.ReadDateTime),
                new SQLFieldMapping("", "year", "Year", DataReaders.ReadInt32),
                new SQLFieldMapping("", "rating", "Rating", DataReaders.ReadInt32),
            }, delegate(WebMusicAlbumBasic album)
            {
                if(album.Artists.Count() > 0) 
                {
                    int i = 0;
                    string[] filenames = new string[] {
                        PathUtil.StripInvalidCharacters(album.Artists.Distinct().First() + "-" + album.Title + "L.jpg", '_'),
                        PathUtil.StripInvalidCharacters(album.Artists.Distinct().First() + "-" + album.Title + ".jpg", '_')
                    };
                    foreach (string file in filenames)
                    {
                        string path = Path.Combine(configuration["cover"], "Albums", file);
                        if (File.Exists(path))
                        {
                            album.Artwork.Add(new WebArtworkDetailed()
                            {
                                Type = WebFileType.Cover,
                                Offset = i++,
                                Path = path,
                                Rating = 1,
                                Id = path.GetHashCode().ToString(),
                                Filetype = Path.GetExtension(path).Substring(1)
                            });
                        }
                    }
                }
                return album;
            });
        }

        public WebMusicAlbumBasic GetAlbumBasicById(string albumId)
        {
            return (GetAllAlbums() as LazyQuery<WebMusicAlbumBasic>).Where(x => x.Id == albumId).First();
        }
        #endregion

        #region Artists
        private class DetailedArtistInfo
        {
            public string Name { get; set; }
            public string Styles { get; set; }
            public string Tones { get; set; }
            public string Biography { get; set; }
        }

        public IEnumerable<WebMusicArtistDetailed> GetAllArtistsDetailed()
        {
            // pre-load advanced info
            string infoSql = "SELECT strArtist, strStyles, strTones, strAMGBio FROM artistinfo";
            var detInfo = ReadList<DetailedArtistInfo>(infoSql, delegate(SQLiteDataReader reader)
            {
                return new DetailedArtistInfo()
                {
                    Name = reader.ReadString(0),
                    Styles = reader.ReadString(1),
                    Tones = reader.ReadString(2),
                    Biography = reader.ReadString(3),
                };
            }).ToDictionary(x => x.Name, x => x);

            // then load all artists
            string sql = "SELECT DISTINCT strArtist FROM tracks " +
                         "UNION " +
                         "SELECT DISTINCT stralbumArtist FROM tracks ";
            return ReadList<IEnumerable<string>>(sql, delegate(SQLiteDataReader reader)
            {
                return reader.ReadPipeList(0);
            })
                .SelectMany(x => x)
                .Distinct()
                .OrderBy(x => x)
                .Select(x =>
                {
                    var artist = new WebMusicArtistDetailed();
                    artist.Id = x;
                    artist.Title = x;

                    if (detInfo.ContainsKey(x))
                    {
                        artist.Styles = detInfo[x].Styles;
                        artist.Tones = detInfo[x].Tones;
                        artist.Biography = detInfo[x].Biography;
                    }

                    int i = 0;
                    string[] filenames = new string[] {
                        PathUtil.StripInvalidCharacters(x + "L.jpg", '_'),
                        PathUtil.StripInvalidCharacters(x + ".jpg", '_')
                    };
                    foreach (string file in filenames)
                    {
                        string path = Path.Combine(configuration["cover"], "Artists", file);
                        if (File.Exists(path))
                        {
                            artist.Artwork.Add(new WebArtworkDetailed()
                            {
                                Type = WebFileType.Cover,
                                Offset = i++,
                                Path = path,
                                Rating = 1,
                                Id = path.GetHashCode().ToString(),
                                Filetype = Path.GetExtension(path).Substring(1)
                            });
                        }
                    }

                    return artist;
                });
        }

        public IEnumerable<WebMusicArtistBasic> GetAllArtists()
        {
            return GetAllArtistsDetailed().Select(x => ArtistDetailedToArtistBasic(x));
        }

        public WebMusicArtistDetailed GetArtistDetailedById(string artistId)
        {
            return GetAllArtistsDetailed().Where(x => x.Id == artistId).First();
        }

        public WebMusicArtistBasic GetArtistBasicById(string artistId)
        {
            return ArtistDetailedToArtistBasic(GetAllArtistsDetailed().Where(x => x.Id == artistId).First());
        }

        private WebMusicArtistBasic ArtistDetailedToArtistBasic(WebMusicArtistDetailed det)
        {
            return new WebMusicArtistBasic()
            {
                Artwork = det.Artwork,
                Id = det.Id,
                PID = det.PID,
                Title = det.Title
            };
        }
        #endregion

        public IEnumerable<WebSearchResult> Search(string text)
        {
            using (DatabaseConnection connection = OpenConnection())
            {
                SQLiteParameter param = new SQLiteParameter("@search", "%" + text + "%");

                string artistSql = "SELECT DISTINCT strArtist, strAlbumArtist, strAlbum FROM tracks WHERE strArtist LIKE @search";
                IEnumerable<WebSearchResult> artists = ReadList<IEnumerable<WebSearchResult>>(artistSql, delegate(SQLiteDataReader reader)
                {
                    IEnumerable<string> albumArtists = reader.ReadPipeList(1);
                    return reader.ReadPipeList(0)
                        .Where(name => name.Contains(text))
                        .Select(name => new WebSearchResult()
                        {
                            Type = WebMediaType.MusicArtist,
                            Id = name,
                            Title = name,
                            Score = (int)Math.Round(40 + (decimal)text.Length / name.Length * 40)
                        })
                        .Concat(new[] { new WebSearchResult() 
                        {
                            Type = WebMediaType.MusicAlbum,
                            Id = (string)AlbumIdReader(reader, 2),
                            Title = reader.ReadString(2),
                            Score = (int)Math.Round(20 + (decimal)text.Length / reader.ReadString(0).Length * 40),
                            Details = new SerializableDictionary<string>()
                            {
                                { "Artist", albumArtists.First() },
                                { "ArtistId", albumArtists.First() }
                            }
                        }
                    });
                }, param).SelectMany(x => x);

                string songSql = "SELECT DISTINCT idTrack, strTitle, strAlbumArtist, strAlbum, iDuration, iYear FROM tracks WHERE strTitle LIKE @search";
                IEnumerable<WebSearchResult> songs = ReadList<WebSearchResult>(songSql, delegate(SQLiteDataReader reader)
                {
                    IEnumerable<string> allArtists = reader.ReadPipeList(2);
                    string title = reader.ReadString(1);
                    string artist = allArtists.Count() == 0 ? String.Empty : allArtists.First();
                    return new WebSearchResult()
                    {
                        Type = WebMediaType.MusicTrack,
                        Id = reader.ReadIntAsString(0),
                        Title = title,
                        Score = (int)Math.Round(40 + (decimal)text.Length / title.Length * 40),
                        Details = new SerializableDictionary<string>()
                    {
                        { "Artist", artist },
                        { "ArtistId", artist },
                        { "Album", reader.ReadString(3) },
                        { "AlbumId", (string)AlbumIdReader(reader, 3) },
                        { "Duration", reader.ReadIntAsString(4) },
                        { "Year", reader.ReadIntAsString(5) }
                    }
                    };
                }, param);

                string albumsSql =
                    "SELECT DISTINCT strAlbumArtist, strAlbum " +
                    "FROM tracks " +
                    "WHERE strAlbum LIKE @search";
                IEnumerable<WebSearchResult> albums = ReadList<IEnumerable<WebSearchResult>>(albumsSql, delegate(SQLiteDataReader reader)
                {
                    string title = reader.ReadString(1);
                    IEnumerable<string> artistList = reader.ReadPipeList(0);
                    var albumResult = new WebSearchResult()
                    {
                        Type = WebMediaType.MusicAlbum,
                        Id = (string)AlbumIdReader(reader, 1),
                        Title = title,
                        Score = (int)Math.Round(40 + (decimal)text.Length / title.Length * 40),
                        Details = new SerializableDictionary<string>()
                    {
                        { "Artist", artistList.First().Trim() },
                        { "ArtistId", artistList.First().Trim() }
                    }
                    };

                    string allArtists = String.Join("", artistList);
                    return artistList
                        .Select(name => new WebSearchResult()
                        {
                            Type = WebMediaType.MusicArtist,
                            Id = name,
                            Title = name,
                            Score = (int)Math.Round((decimal)name.Length / allArtists.Length * 30)
                        }).Concat(new[] { albumResult });
                }, param).SelectMany(x => x);

                return artists.Concat(songs).Concat(albums);
            }
        }

        public IEnumerable<WebGenre> GetAllGenres()
        {
            string sql = "SELECT DISTINCT strGenre FROM tracks";
            return ReadList<IEnumerable<string>>(sql, delegate(SQLiteDataReader reader)
            {
                return reader.ReadPipeList(0);
            })
                    .SelectMany(x => x)
                    .Distinct()
                    .OrderBy(x => x)
                    .Select(x => new WebGenre() { Title = x });
        }

        public IEnumerable<WebCategory> GetAllCategories()
        {
            return new List<WebCategory>();
        }

        public WebFileInfo GetFileInfo(string path)
        {
            return new WebFileInfo(new FileInfo(path));
        }

        public Stream GetFile(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public SerializableDictionary<string> GetExternalMediaInfo(WebMediaType type, string id)
        {
            if (type == WebMediaType.MusicAlbum)
            {
                var album = GetAlbumBasicById(id);
                return new SerializableDictionary<string>()
                {
                    { "Type", "mpmusic album" },
                    { "Album", album.Title },
                    { "Artist", album.AlbumArtist }
                };
            }
            else if (type == WebMediaType.MusicTrack)
            {
                return new SerializableDictionary<string>()
                {
                    { "Type", "mpmusic track" },
                    { "Id", GetTrackBasicById(id).Id }
                };
            }
            else if (type == WebMediaType.MusicArtist)
            {
                return new SerializableDictionary<string>()
                {
                    { "Type", "mpmusic album" },
                    { "Artist", GetArtistBasicById(id).Title }
                };
            }

            throw new ArgumentException();
        }

        [AllowSQLCompare("trim(%table.strAlbum || '_MPE_' || %table.strAlbumArtist) = trim(%prepared)")]
        private object AlbumIdReader(SQLiteDataReader reader, int index)
        {
            // make sure you always select strAlbumArtist, strAlbum when using this method
            return reader.ReadString(index) + "_MPE_" + reader.ReadString(index - 1);
        }

        private string GetLargeAlbumCover(string artistName, string albumName)
        {
            if (String.IsNullOrEmpty(artistName) || String.IsNullOrEmpty(albumName))
                return string.Empty;

            artistName = PathUtil.StripInvalidCharacters(artistName, '_');
            albumName = PathUtil.StripInvalidCharacters(albumName, '_');

            string thumbDir = configuration["cover"];
            return Path.Combine(thumbDir, "Albums", artistName + "-" + albumName + "L.jpg");
        }

        private string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        private string DecodeFrom64(string encodedData)
        {
            try
            {
                byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
                return Encoding.UTF8.GetString(encodedDataAsBytes);
            }
            catch (FormatException)
            {
                return String.Empty;
            }
        }
    }
}
