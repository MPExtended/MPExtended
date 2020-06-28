#region Copyright (C) 2012-2013 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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
using System.Text;
using MediaPortal.Playlists;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.Playlist;

namespace MPExtended.PlugIns.MAS.MPMusic
{
  [Export(typeof(IMusicLibrary))]
  [Export(typeof(IPlaylistLibrary))]
  [ExportMetadata("Name", "MP MyMusic")]
  [ExportMetadata("Id", 4)]
  public class MPMusic : Database, IMusicLibrary, IPlaylistLibrary
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
      Dictionary<string, WebMusicArtistBasic> artists = null;
      Dictionary<Tuple<string, string>, IList<WebArtworkDetailed>> artwork = new Dictionary<Tuple<string, string>, IList<WebArtworkDetailed>>();

      string sql = "SELECT idTrack, strAlbumArtist, strAlbum, strArtist, iTrack, strTitle, strPath, iDuration, iYear, strGenre, iRating, iDisc, " +
                   "CASE WHEN TRIM(strFullCodec) = '' THEN strFileType ELSE strFullCodec END AS Codec " +
                   "FROM tracks t " +
                   "WHERE %where";
      return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>() {
                new SQLFieldMapping("t", "idTrack", "Id", DataReaders.ReadIntAsString),
                new SQLFieldMapping("t", "strArtist", "Artist", DataReaders.ReadPipeList),
                new SQLFieldMapping("t", "strArtist", "ArtistId", DataReaders.ReadPipeList),
                new SQLFieldMapping("t", "strAlbumArtist", "AlbumArtist", DataReaders.ReadPipeListAsString),
                new SQLFieldMapping("t", "strAlbumArtist", "AlbumArtistId", DataReaders.ReadPipeListAsString),
                new SQLFieldMapping("t", "strAlbum", "Album", DataReaders.ReadString),
                new SQLFieldMapping("t", "strAlbum", "AlbumId", AlbumIdReader),
                new SQLFieldMapping("t", "strTitle", "Title", DataReaders.ReadString),
                new SQLFieldMapping("t", "iTrack", "TrackNumber", DataReaders.ReadInt32),
                new SQLFieldMapping("t", "strPath", "Path", DataReaders.ReadStringAsList),
                new SQLFieldMapping("t", "strGenre", "Genres", DataReaders.ReadPipeList),
                new SQLFieldMapping("t", "iYear", "Year", DataReaders.ReadInt32),
                new SQLFieldMapping("t", "iDuration", "Duration", DataReaders.ReadInt32),
                new SQLFieldMapping("t", "iRating", "Rating", DataReaders.ReadInt32),
                new SQLFieldMapping("t", "dateAdded", "DateAdded", DataReaders.ReadDateTime),
                new SQLFieldMapping("t", "iDisc", "DiscNumber", DataReaders.ReadInt32),
                new SQLFieldMapping("t", "Codec", "Codec", DataReaders.ReadString)
            }, delegate (T item)
            {
              if (item is WebMusicTrackDetailed)
              {
                if (artists == null)
                  artists = GetAllArtists().ToDictionary(x => x.Id, x => x);

                WebMusicTrackDetailed det = item as WebMusicTrackDetailed;
                det.Artists = det.ArtistId.Where(x => artists.ContainsKey(x)).Select(x => artists[x]).ToList();
                if (artists.ContainsKey(det.AlbumArtist))
                  det.AlbumArtistObject = artists[det.AlbumArtist];
              }

              // if there is no artist, we can't load album artwork, so just skip without artwork
              if (item.Artist.Count == 0)
                return item;

              // for now, use album artwork also for songs
              var tuple = new Tuple<string, string>(item.Artist.Distinct().First(), item.Album);
              if (!artwork.ContainsKey(tuple))
              {
                artwork[tuple] = new List<WebArtworkDetailed>();
                int i = 0;
                var files = new string[] {
                        PathUtil.StripInvalidCharacters(string.Join(" _ ", item.Artist) + "-" + item.Album + "L.jpg", '_'),
                        PathUtil.StripInvalidCharacters(string.Join(" _ ", item.Artist) + "-" + item.Album + ".jpg", '_')
                    }
                    .Select(x => Path.Combine(configuration["cover"], "Albums", x))
                    .Where(x => File.Exists(x))
                    .Distinct();
                foreach (var path in files)
                {
                  artwork[tuple].Add(new WebArtworkDetailed()
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

                // why isn't there an IList<T>.AddRange() method?
                (item.Artwork as List<WebArtwork>).AddRange(artwork[tuple]);
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
                      "MAX(i.iRating) AS rating, " +
                      "CASE WHEN TRIM(strFullCodec) = '' THEN strFileType ELSE strFullCodec END AS Codec " +
                   "FROM tracks t " +
                   "LEFT JOIN albuminfo i ON t.strAlbum = i.strAlbum AND t.strArtist LIKE '%' || i.strArtist || '%' " +
                   "GROUP BY t.strAlbum, t.strAlbumArtist, t.strFileType " +
                   "HAVING %where ";
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
                new SQLFieldMapping("", "date", "DateAdded", DataReaders.ReadDateTime),
                new SQLFieldMapping("", "year", "Year", DataReaders.ReadInt32),
                new SQLFieldMapping("", "rating", "Rating", DataReaders.ReadInt32),
                new SQLFieldMapping("", "Codec", "Codec", DataReaders.ReadString),
            }, delegate (WebMusicAlbumBasic album)
            {
              if (!string.IsNullOrEmpty(album.AlbumArtist))
              {
                string albumArtist = album.AlbumArtist.Trim('|').Trim();
                int i = 0;
                string[] filenames = new string[] {
                        PathUtil.StripInvalidCharacters(albumArtist + "-" + album.Title + "L.jpg", '_'),
                        PathUtil.StripInvalidCharacters(albumArtist + "-" + album.Title + ".jpg", '_')
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

              if (album.Artists.Count() > 0)
              {
                int i = 0;
                string[] filenames = new string[] {
                        PathUtil.StripInvalidCharacters(string.Join(" _ ", album.Artists) + "-" + album.Title + "L.jpg", '_'),
                        PathUtil.StripInvalidCharacters(string.Join(" _ ", album.Artists) + "-" + album.Title + ".jpg", '_')
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

    private List<WebArtwork> GetArtworkForArtist(string name)
    {
      var artwork = new List<WebArtwork>();

      int i = 0;
      string[] filenames = new string[] {
                        PathUtil.StripInvalidCharacters(name + "L.jpg", '_'),
                        PathUtil.StripInvalidCharacters(name + ".jpg", '_')
                    };
      foreach (string file in filenames)
      {
        string path = Path.Combine(configuration["cover"], "Artists", file);
        if (File.Exists(path))
        {
          artwork.Add(new WebArtworkDetailed()
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

      return artwork;
    }

    public IEnumerable<WebMusicArtistBasic> GetAllArtists()
    {
      string sql = "SELECT strArtist, 0 AS hasAlbums FROM tracks GROUP BY strArtist " +
                   "UNION " +
                   "SELECT strAlbumArtist, 1 AS hasAlbums FROM tracks GROUP BY strAlbumArtist ";
      return ReadList(sql, delegate (SQLiteDataReader reader)
      {
        return reader.ReadPipeList(0).Select(x => new { Artist = x, HasAlbums = reader.ReadBoolean(1) });
      })
          .SelectMany(x => x)
          .GroupBy(x => x.Artist)
          .Select(x =>
          {
            return new WebMusicArtistBasic()
            {
              Id = x.Key,
              Title = x.Key,
              HasAlbums = x.Any(y => y.HasAlbums),
              Artwork = GetArtworkForArtist(x.Key)
            };
          });
    }

    public IEnumerable<WebMusicArtistDetailed> GetAllArtistsDetailed()
    {
      // pre-load advanced info
      string infoSql = "SELECT strArtist, strStyles, strTones, strAMGBio FROM artistinfo";
      var detInfo = ReadList<DetailedArtistInfo>(infoSql, delegate (SQLiteDataReader reader)
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
      string sql = "SELECT strArtist, 0 AS hasAlbums, GROUP_CONCAT(strGenre, '|') AS genres " +
                      "FROM tracks " +
                      "GROUP BY strArtist " +
                   "UNION " +
                      "SELECT strAlbumArtist, 1 AS hasAlbums, GROUP_CONCAT(strGenre, '|') AS genres " +
                      "FROM tracks " +
                      "GROUP BY strAlbumArtist";
      return ReadList(sql, delegate (SQLiteDataReader reader)
      {
        var hasAlbums = reader.ReadBoolean(1);
        var genres = reader.ReadPipeList(2);
        return reader.ReadPipeList(0).Select(x => new
        {
          Artist = x,
          HasAlbums = hasAlbums,
          Genres = genres
        });
      })
          .SelectMany(x => x)
          .GroupBy(x => x.Artist)
          .Select(x =>
          {
            var artist = new WebMusicArtistDetailed();
            artist.Id = x.Key;
            artist.HasAlbums = x.Any(y => y.HasAlbums);
            artist.Title = x.Key;

            artist.Genres = x.SelectMany(y => y.Genres).ToList();
            artist.Artwork = GetArtworkForArtist(x.Key);

            if (detInfo.ContainsKey(x.Key))
            {
              artist.Styles = detInfo[x.Key].Styles;
              artist.Tones = detInfo[x.Key].Tones;
              artist.Biography = detInfo[x.Key].Biography;
            }

            return artist;
          });
    }

    public WebMusicArtistDetailed GetArtistDetailedById(string artistId)
    {
      return GetAllArtistsDetailed().Where(x => x.Id == artistId).First();
    }

    public WebMusicArtistBasic GetArtistBasicById(string artistId)
    {
      return GetAllArtists().Where(x => x.Id == artistId).First();
    }
    #endregion

    public IEnumerable<WebSearchResult> Search(string text)
    {
      using (DatabaseConnection connection = OpenConnection())
      {
        SQLiteParameter param = new SQLiteParameter("@search", "%" + text + "%");

        string artistSql = "SELECT DISTINCT strArtist, strAlbumArtist, strAlbum FROM tracks WHERE strArtist LIKE @search";
        IEnumerable<WebSearchResult> artists = ReadList<IEnumerable<WebSearchResult>>(artistSql, delegate (SQLiteDataReader reader)
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
                            Details = new WebDictionary<string>()
                            {
                                { "Artist", albumArtists.First() },
                                { "ArtistId", albumArtists.First() }
                            }
                        }
            });
        }, param).SelectMany(x => x);

        string songSql = "SELECT DISTINCT idTrack, strTitle, strAlbumArtist, strAlbum, iDuration, iYear FROM tracks WHERE strTitle LIKE @search";
        IEnumerable<WebSearchResult> songs = ReadList<WebSearchResult>(songSql, delegate (SQLiteDataReader reader)
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
            Details = new WebDictionary<string>()
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
            "SELECT DISTINCT t.strAlbumArtist, t.strAlbum, " +
                "CASE WHEN i.iYear ISNULL THEN MIN(t.iYear) ELSE i.iYear END AS year " +
            "FROM tracks t " +
            "LEFT JOIN albuminfo i ON t.strAlbum = i.strAlbum AND t.strArtist LIKE '%' || i.strArtist || '%' " +
            "WHERE t.strAlbum LIKE @search " +
            "GROUP BY t.strAlbumArtist, t.strAlbum ";
        IEnumerable<WebSearchResult> albums = ReadList<IEnumerable<WebSearchResult>>(albumsSql, delegate (SQLiteDataReader reader)
        {
          string title = reader.ReadString(1);
          IEnumerable<string> artistList = reader.ReadPipeList(0);
          var albumResult = new WebSearchResult()
          {
            Type = WebMediaType.MusicAlbum,
            Id = (string)AlbumIdReader(reader, 1),
            Title = title,
            Score = (int)Math.Round(40 + (decimal)text.Length / title.Length * 40),
            Details = new WebDictionary<string>()
            {
                        { "Artist", artistList.First().Trim() },
                        { "ArtistId", artistList.First().Trim() },
                        { "Year", reader.ReadIntAsString(2) }
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
      return ReadList<IEnumerable<string>>(sql, delegate (SQLiteDataReader reader)
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
      return new WebFileInfo(PathUtil.StripFileProtocolPrefix(path));
    }

    public Stream GetFile(string path)
    {
      return new FileStream(path, FileMode.Open, FileAccess.Read);
    }

    public WebDictionary<string> GetExternalMediaInfo(WebMediaType type, string id)
    {
      if (type == WebMediaType.MusicAlbum)
      {
        var album = GetAlbumBasicById(id);
        return new WebDictionary<string>()
                {
                    { "Type", "mpmusic album" },
                    { "Album", album.Title },
                    { "Artist", album.AlbumArtist }
                };
      }
      else if (type == WebMediaType.MusicTrack)
      {
        return new WebDictionary<string>()
                {
                    { "Type", "mpmusic track" },
                    { "Id", GetTrackBasicById(id).Id }
                };
      }
      else if (type == WebMediaType.MusicArtist)
      {
        return new WebDictionary<string>()
                {
                    { "Type", "mpmusic artist" },
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

    #region Playlists
    public IEnumerable<WebPlaylist> GetPlaylists()
    {
      return Directory.GetFiles(configuration["playlist"])
          .Where(p => PlayListFactory.IsPlayList(p))
          .Select(p => GetPlaylist(p))
          .Where(p => p != null)
          .ToList();
    }

    private WebPlaylist GetPlaylist(string path)
    {
      PlayList mpPlaylist = new PlayList();
      IPlayListIO factory = PlayListFactory.CreateIO(path);
      if (factory.Load(mpPlaylist, path))
      {
        WebPlaylist webPlaylist = new WebPlaylist() { Id = EncodeTo64(Path.GetFileName(path)), Title = mpPlaylist.Name, Path = new List<string>() { path } };
        webPlaylist.ItemCount = mpPlaylist.Count;
        return webPlaylist;
      }
      else
      {
        Log.Warn("Couldn't parse playlist " + path);
        return null;
      }
    }

    public IEnumerable<WebPlaylistItem> GetPlaylistItems(string playlistId)
    {
      PlayList mpPlaylist = new PlayList();
      string path = GetPlaylistPath(playlistId);
      IPlayListIO factory = PlayListFactory.CreateIO(path);
      if (factory.Load(mpPlaylist, path))
      {
        var tracks = LoadAllTracks<WebMusicTrackBasic>().ToList();

        return mpPlaylist
            .Select(x =>
                {
                  var track = tracks.FirstOrDefault(t => t.Path.Contains(x.FileName));
                  return new WebPlaylistItem()
                  {
                    Title = x.Description,
                    Duration = x.Duration,
                    Path = new List<string>() { x.FileName },
                    Type = WebMediaType.MusicTrack,
                    Id = track != null ? track.Id : null,
                    DateAdded = track != null ? track.DateAdded : new DateTime(1970, 1, 1),
                    Artwork = track != null ? track.Artwork : new List<WebArtwork>(),
                  };
                })
            .ToList();
      }
      else if (new FileInfo(path).Length == 0)
      {
        // for newly created playlists, return an empty list, to make sure that a CreatePlaylist call followed by an AddItem works
        return new List<WebPlaylistItem>();
      }
      else
      {
        Log.Warn("Couldn't load playlist {0}", playlistId);
        return null;
      }
    }

    public bool SavePlaylist(string playlistId, IEnumerable<WebPlaylistItem> playlistItems)
    {
      String path = GetPlaylistPath(playlistId);
      PlayList mpPlaylist = new PlayList();
      IPlayListIO factory = PlayListFactory.CreateIO(path);

      foreach (WebPlaylistItem i in playlistItems)
      {
        PlayListItem mpItem = new PlayListItem(i.Title, i.Path[0], i.Duration);
        mpItem.Type = PlayListItem.PlayListItemType.Audio;
        mpPlaylist.Add(mpItem);
      }

      return factory.Save(mpPlaylist, path);
    }

    private string GetPlaylistPath(string playlistId)
    {
      return Path.Combine(configuration["playlist"], DecodeFrom64(playlistId));
    }

    public string CreatePlaylist(string playlistName)
    {
      try
      {
        string fileName = playlistName + ".m3u";
        string path = Path.Combine(configuration["playlist"], fileName);

        PlayList mpPlaylist = new PlayList();
        IPlayListIO factory = PlayListFactory.CreateIO(path);
        factory.Save(mpPlaylist, path);

        return EncodeTo64(fileName);
      }
      catch (Exception ex)
      {
        Log.Error("Unable to create playlist " + playlistName, ex);
      }

      return null;
    }

    public bool DeletePlaylist(string playlistId)
    {
      try
      {
        string fileName = GetPlaylistPath(playlistId);
        File.Delete(fileName);
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("Unable to delete playlist " + playlistId, ex);
        return false;
      }
    }
    #endregion
  }
}
