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
using System.Text.RegularExpressions;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.PlugIns.MAS.MPVideos
{
    [Export(typeof(IMovieLibrary))]
    [ExportMetadata("Name", "MP MyVideo")]
    [ExportMetadata("Id", 7)]
    public class MPVideos : Database, IMovieLibrary
    {
        private Dictionary<string, string> configuration;
        private Dictionary<string, string> fanartconfiguration;
        public bool Supported { get; set; }

        [ImportingConstructor]
        public MPVideos(IPluginData data)
        {
            configuration = data.GetConfiguration("MP MyVideo");
            fanartconfiguration = data.GetConfiguration("FanartHandler");
            DatabasePath = configuration["database"];
            Supported = File.Exists(DatabasePath);
        }

        private List<WebArtwork> GetArtworkForCollection(string title)
        {
          var artwork = new List<WebArtwork>();
          if (string.IsNullOrEmpty(title))
            return artwork;

          // Poster
          int i = 0;
          var files = new string[] {
                            PathUtil.StripInvalidCharacters(string.Format("{0}{1}", title, "L.jpg"), '_'),
                            PathUtil.StripInvalidCharacters(string.Format("{0}{1}", title, ".jpg"), '_')
                    }
            .Select(x => Path.Combine(configuration["cover"], "Collection", x))
            .Where(x => File.Exists(x))
            .Distinct();
          foreach (string file in files)
          {
            artwork.Add(new WebArtworkDetailed()
            {
              Type = WebFileType.Cover,
              Offset = i++,
              Path = file,
              Rating = 1,
              Id = file.GetHashCode().ToString(),
              Filetype = Path.GetExtension(file).Substring(1)
            });
          }

          // Backdrops
          i = 0;
          string thumbfolder = Path.Combine(fanartconfiguration["thumb"], "Skin Fanart", "Scraper", "Movies");
          if (Directory.Exists(thumbfolder))
          {
            files = Directory.GetFiles(thumbfolder, PathUtil.StripInvalidCharacters(string.Format("{0}{{*}}.jpg", title), '_'))
              .Where(x => File.Exists(x))
              .Distinct();
            foreach (string file in files)
            {
              artwork.Add(new WebArtworkDetailed()
              {
                Type = WebFileType.Backdrop,
                Offset = i++,
                Path = file,
                Rating = 1,
                Id = file.GetHashCode().ToString(),
                Filetype = Path.GetExtension(file).Substring(1)
              });
            }
          }

          // ClearArt
          string logofolder = Path.Combine(fanartconfiguration["thumb"], "ClearArt", "MoviesCollections");
          if (Directory.Exists(thumbfolder))
          {
            string file = Path.Combine(logofolder, PathUtil.StripInvalidCharacters(string.Format("{0}.png", title), '_'));
            if (File.Exists(file))
            {
              artwork.Add(new WebArtworkDetailed()
              {
                Type = WebFileType.Logo,
                Offset = 0,
                Path = file,
                Rating = 1,
                Id = file.GetHashCode().ToString(),
                Filetype = Path.GetExtension(file).Substring(1)
              });
            }
          }

          return artwork;
        }

        private List<WebArtwork> GetArtworkForMovie(WebMovieBasic item)
        {
          var artwork = new List<WebArtwork>();

          // Poster
          int i = 0;
          var files = new string[] {
                  PathUtil.StripInvalidCharacters(string.Format("{0}{{{1}}}{2}", item.Title, item.Id, "L.jpg"), '_'),
                  PathUtil.StripInvalidCharacters(string.Format("{0}{{{1}}}{2}", item.Title, item.Id, ".jpg"), '_')
          }
            .Select(x => Path.Combine(configuration["cover"], "Title", x))
            .Where(x => File.Exists(x))
            .Distinct();
          if (files == null || files.Count() == 0)
          {
            (artwork as List<WebArtwork>).AddRange(item.Artwork);
          }
          else
          {
            foreach (string file in files)
            {
              artwork.Add(new WebArtworkDetailed()
              {
                Type = WebFileType.Cover,
                Offset = i++,
                Path = file,
                Rating = 1,
                Id = file.GetHashCode().ToString(),
                Filetype = Path.GetExtension(file).Substring(1)
              });
            }
          }

          // Backdrops
          i = 0;
          string thumbfolder = Path.Combine(fanartconfiguration["thumb"], "Skin Fanart", "Scraper", "Movies");
          if (Directory.Exists(thumbfolder))
          {
            files = Directory.GetFiles(thumbfolder, string.Format("{0}{{*}}.jpg", item.Id))
              .Where(x => File.Exists(x))
              .Distinct();
            foreach (string file in files)
            {
              artwork.Add(new WebArtworkDetailed()
              {
                Type = WebFileType.Backdrop,
                Offset = i++,
                Path = file,
                Rating = 1,
                Id = file.GetHashCode().ToString(),
                Filetype = Path.GetExtension(file).Substring(1)
              });
            }
          }

          // ClearArt
          string logofolder = Path.Combine(fanartconfiguration["thumb"], "ClearArt", "Movies");
          if (Directory.Exists(thumbfolder))
          {
            string imdbid = item.ExternalId.Where(x => x.Site == "IMDB").FirstOrDefault()?.Id ?? string.Empty;
            if (!string.IsNullOrEmpty(imdbid))
            {
              string file = Path.Combine(logofolder, string.Format("{0}.png", imdbid));
              if (File.Exists(file))
              {
                artwork.Add(new WebArtworkDetailed()
                {
                  Type = WebFileType.Logo,
                  Offset = 0,
                  Path = file,
                  Rating = 1,
                  Id = file.GetHashCode().ToString(),
                  Filetype = Path.GetExtension(file).Substring(1)
                });
              }
            }
          }

          return artwork;
        }

        private List<WebActor> ActorReader(SQLiteDataReader reader, int idx)
        {
            return ((IList<string>)DataReaders.ReadPipeList(reader, idx))
                     .Select(s => Regex.Match(s, @"(?<title>.+?)(?<id>nm\d{3,})?$"))
                     .Where (m => m.Success)
                       .Select(m => new WebActor() { Title = m.Groups["title"].Value, IMDBId = m.Groups["id"].Value})
                       .ToList();
        }

        private List<string> CreditsReader(SQLiteDataReader reader, int idx)
        {
            return ((string)DataReaders.ReadString(reader, idx))
                .Split('/')
                .Select(x => x.Trim())
                .ToList();
        }

        [MergeListReader]
        private List<WebExternalId> ExternalIdReader(SQLiteDataReader reader, int idx, object site)
        {
            List<WebExternalId> list = new List<WebExternalId>();
            string val = (string)DataReaders.ReadString(reader, idx);
            if(!String.IsNullOrEmpty(val))
            {
                list.Add(new WebExternalId()
                {
                    Site = (string)site,
                    Id = val
                });
            }
            return list;
        }
        
        [MergeListReader]
        private List<WebCollection> CollectionReader(SQLiteDataReader reader, int idx)
        {
            return ((IList<WebCollection>)DataReaders.ReadPipeList(reader, idx)).Select(x => new WebCollection() { Id = x, Title = x, Artwork = GetArtworkForCollection(x) }).ToList();
        }
        
        private LazyQuery<T> LoadMovies<T>() where T : WebMovieBasic, new()
        {
            string mp13Fields = Mediaportal.GetVersion() >= Mediaportal.MediaPortalVersion.MP1_3 ? "i.language, i.strDirector, i.dateAdded, i.strTagLine, " : String.Empty;
            string mp117Fields = Mediaportal.GetVersion() >= Mediaportal.MediaPortalVersion.MP1_17 ? "i.TMDBNumber, i.mpaa, i.MPAAText, i.Awards, REPLACE(i.studios,' / ','|') as studios, " : String.Empty;
            string sql =
                "SELECT m.idMovie, i.strTitle, i.iYear, i.fRating, i.runtime, i.IMDBID, i.strPlot, i.strPictureURL, i.strCredits, i.iswatched, r.stoptime, " + 
                    mp13Fields + mp117Fields +
                    "GROUP_CONCAT(p.strPath || f.strFilename, '|') AS fullpath, " +
                    "GROUP_CONCAT(a.strActor || a.IMDBActorID, '|') AS actors, " +
                    (Mediaportal.GetVersion() >= Mediaportal.MediaPortalVersion.MP1_17 ?
                    "GROUP_CONCAT(u.strGroup, '|') AS groups,  " +
                    "GROUP_CONCAT(c.strCollection, '|') AS collections, " : string.Empty
                    ) +
                    "GROUP_CONCAT(g.strGenre, '|') AS genres, m.timeswatched " +
                "FROM movie m " +
                "INNER JOIN movieinfo i ON m.idMovie = i.idMovie " +
                "LEFT JOIN files f ON m.idMovie = f.idMovie " +
                "LEFT JOIN path p ON f.idPath = p.idPath " +
                "LEFT JOIN resume r ON f.idFile = r.idFile " +
                "LEFT JOIN actorlinkmovie alm ON m.idMovie = alm.idMovie " +
                "LEFT JOIN actors a ON alm.idActor = a.idActor " +
                (Mediaportal.GetVersion() >= Mediaportal.MediaPortalVersion.MP1_17 ?
                "LEFT JOIN moviecollectionlinkmovie clm ON m.idMovie = clm.idMovie " +
                "LEFT JOIN moviecollection c ON clm.idCollection = c.idCollection " +
                "LEFT JOIN usergrouplinkmovie ulm ON m.idMovie = ulm.idMovie " +
                "LEFT JOIN usergroup u ON ulm.idGroup = u.idGroup " : string.Empty
                ) +
                "LEFT JOIN genrelinkmovie glm ON m.idMovie = glm.idMovie " +
                "LEFT JOIN genre g ON glm.idGenre = g.idGenre " +
                "WHERE %where " +
                "GROUP BY m.idMovie, i.strTitle, i.iYear, i.fRating, i.runtime, i.IMDBID, i.strPlot, i.strPictureURL";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>()
            {
                new SQLFieldMapping("m", "idMovie", "Id", DataReaders.ReadIntAsString),
                new SQLFieldMapping("fullpath", "Path", DataReaders.ReadPipeList),
                new SQLFieldMapping("r", "stoptime", "Stoptime", DataReaders.ReadString),
                new SQLFieldMapping("actors", "Actors", ActorReader),
                new SQLFieldMapping("genres", "Genres", DataReaders.ReadPipeList),
                new SQLFieldMapping("i", "strPictureURL", "Artwork", ArtworkRetriever.ArtworkReader),
                new SQLFieldMapping("i", "strTitle", "Title", DataReaders.ReadString),
                new SQLFieldMapping("i", "iYear", "Year", DataReaders.ReadInt32),
                new SQLFieldMapping("i", "fRating", "Rating", DataReaders.ReadStringAsFloat),
                new SQLFieldMapping("i", "runtime", "Runtime", DataReaders.ReadInt32),
                new SQLFieldMapping("i", "strPlot", "Summary", DataReaders.ReadString),
                new SQLFieldMapping("i", "strCredits", "Writers", CreditsReader),
                new SQLFieldMapping("i", "iswatched", "Watched", DataReaders.ReadBoolean),
                new SQLFieldMapping("i", "language", "Language", DataReaders.ReadString),
                new SQLFieldMapping("i", "strDirector", "Directors", DataReaders.ReadStringAsList),
                new SQLFieldMapping("groups", "Groups", DataReaders.ReadPipeList),
                new SQLFieldMapping("collections", "Collections", CollectionReader),
                new SQLFieldMapping("i", "dateAdded", "DateAdded", DataReaders.ReadDateTime),
                new SQLFieldMapping("u", "timeswatched", "TimesWatched", DataReaders.ReadInt32),
                new SQLFieldMapping("i", "IMDBID", "ExternalId", ExternalIdReader, "IMDB"),
                new SQLFieldMapping("i", "TMDBNumber", "ExternalId", ExternalIdReader, "TMDB"),
                new SQLFieldMapping("i", "mpaa", "MPAARating", DataReaders.ReadString),
                new SQLFieldMapping("i", "MPAAText", "MPAAText", DataReaders.ReadString),
                new SQLFieldMapping("i", "Awards", "Awards", DataReaders.ReadString),
                new SQLFieldMapping("studios", "Studios", DataReaders.ReadPipeList),
                new SQLFieldMapping("i", "strTagLine", "Tagline", DataReaders.ReadString),
            }, delegate (T item)
            {
              if (item is WebMovieBasic)
              {
                item.Artwork = GetArtworkForMovie(item);
              }
              return item;
            });
        }

    public WebBoolResult SetWathcedStatus(string id, Boolean isWatched)
    {
      Log.Info("SetWathcedStatus provider = 7 idmovie = {0} isWatched = {1}", id, isWatched);
      try
      {
        String strSQL = String.Format("update movie set watched = '{1}' where idMovie = {0}", id, isWatched);

        Execute(strSQL);

        int intWatched = 0;
        if (isWatched)
        {
          intWatched = 1;
        }

        strSQL = String.Format("update movieinfo set iswatched = {1} where idMovie = {0}", id, intWatched);
        Execute(strSQL);
      }
      catch (SQLiteException ex)
      {
        Log.Error("SetWathcedStatus id = {0} exception {1}", id, ex.Message);
        return false;
      }
      return true;
    }

    public WebBoolResult SetMovieStoptime(string id, int stopTime, Boolean isWatched, int watchedPercent)
    {
      Log.Info("SetMovieStoptime provider = 7 idmovie = {0} stopTime = {1} isWatched = {2}", id, stopTime, isWatched);
      try
      {
        string strSQL = String.Format("select stoptime from resume WHERE idFile = (select idFile from files where idMovie = {0})", id);
        Boolean ret;

        using (DatabaseConnection connection = OpenConnection())
        {
          using (Query query = new Query(connection, strSQL))
          {
            ret = query.Reader.Read();
          }
        }

        if (isWatched)
        {
          strSQL = String.Format("update movie set watched = '{1}', iwatchedPercent = {2}, timeswatched = (select timeswatched+1 from movie where idMovie = {0}) where idMovie = {0}", id, isWatched, watchedPercent);
        }
        else
        {
          strSQL = String.Format("update movie set iwatchedPercent = {1} where idMovie = {0}", id, watchedPercent);
        }

        Execute(strSQL);

        int intWatched = 0;
        if (isWatched)
        {
          intWatched = 1;
        }

        strSQL = String.Format("update movieinfo set iswatched = {1} where idMovie = {0}", id, intWatched);
        Execute(strSQL);

        if (ret)
        {
          strSQL = String.Format("update resume set stoptime = {1} WHERE idFile=(select idFile from files where idMovie = {0})", id, stopTime);
        }
        else
        {
          strSQL = String.Format("insert into resume (idFile, stopTime, resumeData, bdtitle) values ((select idFile from files where idMovie={0}), {1}, '-', 1000)", id, stopTime);
        }

        Execute(strSQL);

        return true;
      }
      catch (SQLiteException ex)
      {
        Log.Error("SetMovieStoptime id = {0} exception {1}", id, ex.Message);
        return false;
      }
    }

    public IEnumerable<WebMovieBasic> GetAllMovies()
    {
        return LoadMovies<WebMovieBasic>();
    }

    public IEnumerable<WebMovieDetailed> GetAllMoviesDetailed()
    {
        return LoadMovies<WebMovieDetailed>();
    }

    public WebMovieBasic GetMovieBasicById(string movieId)
    {
       return LoadMovies<WebMovieBasic>().Where(x => x.Id == movieId).First();
    }

    public WebMovieDetailed GetMovieDetailedById(string movieId)
    {
       return LoadMovies<WebMovieDetailed>().Where(x => x.Id == movieId).First();
    }
    
    public WebCollection GetCollectionById(string title)
    {
       return GetAllCollections().Where(x => x.Id == title).First();
    }
        
        public IEnumerable<WebGenre> GetAllGenres()
        {
            string sql = "SELECT strGenre FROM genre WHERE idGenre in (SELECT idGenre FROM genrelinkmovie)";
            return new LazyQuery<WebGenre>(this, sql, new List<SQLFieldMapping>()
            {
                new SQLFieldMapping("strGenre", "Title", DataReaders.ReadString)
            });
        }

        public IEnumerable<WebCategory> GetAllCategories()
        {
            string sql = "SELECT strGroup, strGroupDescription FROM usergroup WHERE idGroup in (SELECT idGroup FROM usergrouplinkmovie)";
            return new LazyQuery<WebCategory>(this, sql, new List<SQLFieldMapping>()
            {
                new SQLFieldMapping("strGenre", "Title", DataReaders.ReadString),
                new SQLFieldMapping("strGroupDescription", "Description", DataReaders.ReadString)
            });
        }

        public IEnumerable<WebCollection> GetAllCollections()
        {
            string sql = "SELECT strCollection FROM moviecollection WHERE idCollection in (SELECT idCollection FROM moviecollectionlinkmovie)";
            return new LazyQuery<WebCollection>(this, sql, new List<SQLFieldMapping>()
            {
                new SQLFieldMapping("strCollection", "Title", DataReaders.ReadString)
            }, delegate (WebCollection item)
            {
                item.Id = item.Title;
                item.Artwork = GetArtworkForCollection(item.Title);
                return item;
            });
        }

        public WebFileInfo GetFileInfo(string path)
        {
            if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                return ArtworkRetriever.GetFileInfo(path);
            }

            return new WebFileInfo(PathUtil.StripFileProtocolPrefix(path));
        }

        public Stream GetFile(string path)
        {
            if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                return ArtworkRetriever.GetStream(path);
            }

            return new FileStream(PathUtil.StripFileProtocolPrefix(path), FileMode.Open, FileAccess.Read);
        }

        public IEnumerable<WebSearchResult> Search(string text)
        {
            using (DatabaseConnection connection = OpenConnection())
            {
                var param = new SQLiteParameter("@search", "%" + text + "%");
                string sql = "SELECT idMovie, strTitle, iYear, strGenre FROM movieinfo WHERE strTitle LIKE @search";
                IEnumerable<WebSearchResult> titleResults = ReadList<WebSearchResult>(sql, delegate(SQLiteDataReader reader)
                {
                    string title = reader.ReadString(1);
                    string genres = reader.ReadString(3);
                    return new WebSearchResult()
                    {
                        Type = WebMediaType.Movie,
                        Id = reader.ReadIntAsString(0),
                        Title = title,
                        Score = (int)Math.Round(40 + (decimal)text.Length / title.Length * 40),
                        Details = new WebDictionary<string>()
                    {
                        { "Year", reader.ReadIntAsString(2) },
                        { "Genres", genres == "unknown" ? String.Empty : genres }
                    }
                    };
                }, param);

                string actorSql = "SELECT a.strActor, mi.idMovie, mi.strTitle, mi.iYear, mi.strGenre " +
                                  "FROM actors a " +
                                  "LEFT JOIN actorlinkmovie alm ON alm.idActor = a.idActor " +
                                  "INNER JOIN movieinfo mi ON alm.idMovie = mi.idMovie " +
                                  "WHERE a.strActor LIKE @search";
                IEnumerable<WebSearchResult> actorResults = ReadList<WebSearchResult>(actorSql, delegate(SQLiteDataReader reader)
                {
                    string genres = reader.ReadString(4);
                    return new WebSearchResult()
                    {
                        Type = WebMediaType.Movie,
                        Id = reader.ReadIntAsString(1),
                        Title = reader.ReadString(2),
                        Score = (int)Math.Round(40 + (decimal)text.Length / reader.ReadString(0).Length * 30),
                        Details = new WebDictionary<string>()
                    {
                        { "Year", reader.ReadIntAsString(3) },
                        { "Genres", genres == "unknown" ? String.Empty : genres }
                    }
                    };
                }, param);

                return titleResults.Concat(actorResults);
            }
        }

        public WebDictionary<string> GetExternalMediaInfo(WebMediaType type, string id)
        {
            if (type == WebMediaType.Collection)
            {
                return new WebDictionary<string>()
                {
                    { "Type", "myvideos collection" },
                    { "Id", GetCollectionById(id).Id }
                };            
            }
            
            return new WebDictionary<string>()
            {
                { "Type", "myvideos" },
                { "Id", id }
            };
        }
    }
}
