#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.PlugIns.MAS.MovingPictures
{
    [Export(typeof(IMovieLibrary))]
    [ExportMetadata("Name", "Moving Pictures")]
    [ExportMetadata("Id", 3)]
    public partial class MPMovingPictures : Database, IMovieLibrary
    {
        // TODO: according to the devs movingpictures is quite easy usable from outside MP. Investigate using that way:
        // - it's better for compatibility
        // - it's less code for us
        // - we can more easily add new features (see for example the horrible TMDB implementation below)

        public bool Supported { get; set; }

        [ImportingConstructor]
        public MPMovingPictures(IPluginData data)
        {
            DatabasePath = data.GetConfiguration("Moving Pictures")["database"];
            Supported = File.Exists(DatabasePath);
        }

        [MergeListReader]
        private List<WebArtworkDetailed> CoverReader(SQLiteDataReader reader, int idx)
        {
            int i = 0;
            var preferred = (string)DataReaders.ReadString(reader, idx + 1);
            return (DataReaders.ReadPipeList(reader, idx) as List<string>).Select(x => new WebArtworkDetailed()
            {
                Offset = i++,
                Type = WebFileType.Cover,
                Path = x,
                Id = x.GetHashCode().ToString(),
                Filetype = Path.GetExtension(x).Substring(1),
                Rating = x == preferred ? 2 : 1
            }).ToList();
        }

        [MergeListReader]
        private List<WebArtworkDetailed> BackdropReader(SQLiteDataReader reader, int idx)
        {
            int i = 0;
            return (DataReaders.ReadStringAsList(reader, idx) as List<string>).Select(x => new WebArtworkDetailed()
            {
                Offset = i++,
                Type = WebFileType.Backdrop,
                Path = x,
                Id = x.GetHashCode().ToString(),
                Filetype = Path.GetExtension(x).Substring(1),
                Rating = 1
            }).ToList();
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

        private List<WebActor> ActorReader(SQLiteDataReader reader, int idx)
        {
            return ((IList<string>)DataReaders.ReadPipeList(reader, idx)).Select(x => new WebActor() { Title = x }).ToList();
        }

        private LazyQuery<T> GetAllMovies<T>() where T : WebMovieBasic, new()
        {
            string sql = "SELECT DISTINCT m.id, m.date_added, m.genres, m.score, m.runtime, m.title, m.year, " +
                            "GROUP_CONCAT(l.fullpath, '|') AS path, " +
                            "m.directors, m.writers, m.actors, m.summary, m.language, m.tagline, m.imdb_id, m.tagline, s.identifier, " +
                            "m.backdropfullpath, m.alternatecovers, m.coverfullpath," +
                            "u.watched, u.resume_time, u.watched " + 
                         "FROM movie_info m " +
                         "INNER JOIN local_media__movie_info AS i ON i.movie_info_id = m.id " +
                         "INNER JOIN local_media AS l ON l.id = i.local_media_id AND l.ignored = 0 " +
                         "LEFT JOIN user_movie_settings u ON u.id = m.id " + 
                         "LEFT JOIN " +
                                "(SELECT smi.identifier AS identifier, smi.movie AS movie " +
                                " FROM " +
                                    "(SELECT smi.movie AS movie, MAX(smi.id) AS id " +
                                    " FROM source_movie_info smi " +
                                    " LEFT JOIN source_info si ON smi.source = si.id " + 
                                    " WHERE si.providertype LIKE 'MediaPortal.Plugins.MovingPictures.DataProviders.TheMovieDbProvider, MovingPictures, %' " + 
                                    " GROUP BY smi.movie) AS sid " +
                                " LEFT JOIN source_movie_info smi ON sid.id = smi.id " +
                                ") s " +
                            "ON s.movie = m.id AND s.identifier != '' " +
                         "WHERE %where " +
                         "GROUP BY m.id, m.date_added, m.backdropfullpath, m.alternatecovers, m.genres, m.score, m.runtime, m.title, m.year, " +
                            "m.directors, m.writers, m.actors, m.summary, m.language, m.tagline, m.imdb_id, s.identifier " +
                         "%order";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>() {
                new SQLFieldMapping("m", "id", "Id", DataReaders.ReadIntAsString),
                new SQLFieldMapping("m", "date_added", "DateAdded", DataReaders.ReadDateTime),
                new SQLFieldMapping("m", "backdropfullpath", "Artwork", BackdropReader),
                new SQLFieldMapping("m", "alternatecovers", "Artwork", CoverReader),
                new SQLFieldMapping("m", "genres", "Genres", DataReaders.ReadPipeList),
                new SQLFieldMapping("m", "score", "Rating", DataReaders.ReadFloat),
                new SQLFieldMapping("m", "runtime", "Runtime", DataReaders.ReadInt32),
                new SQLFieldMapping("m", "title", "Title", DataReaders.ReadString),
                new SQLFieldMapping("m", "year", "Year", DataReaders.ReadInt32),
                new SQLFieldMapping("", "path", "Path", DataReaders.ReadPipeList),
                new SQLFieldMapping("m", "directors", "Directors", DataReaders.ReadPipeList),
                new SQLFieldMapping("m", "writers", "Writers", DataReaders.ReadPipeList),
                new SQLFieldMapping("m", "actors", "Actors", ActorReader),
                new SQLFieldMapping("m", "summary", "Summary", DataReaders.ReadString),
                new SQLFieldMapping("m", "language", "Language", DataReaders.ReadString),
                new SQLFieldMapping("m", "tagline", "Tagline", DataReaders.ReadString),
                new SQLFieldMapping("m", "imdb_id", "ExternalId", ExternalIdReader, "IMDB"),
                new SQLFieldMapping("s", "identifier", "ExternalId", ExternalIdReader, "TMDB"),
                new SQLFieldMapping("u", "watched", "Watched", DataReaders.ReadBoolean),
                new SQLFieldMapping("u", "watched", "TimesWatched", DataReaders.ReadInt32),
                new SQLFieldMapping("u", "resume_time", "Stoptime", DataReaders.ReadString),
            });
        }

    public IEnumerable<WebMovieBasic> GetAllMovies()
        {
            return GetAllMovies<WebMovieBasic>();
        }

        public IEnumerable<WebMovieDetailed> GetAllMoviesDetailed()
        {
            return GetAllMovies<WebMovieDetailed>();
        }

        public WebMovieBasic GetMovieBasicById(string movieId)
        {
            return GetAllMovies<WebMovieBasic>().Where(x => x.Id == movieId).First();
        }

        public WebMovieDetailed GetMovieDetailedById(string movieId)
        {
            return GetAllMovies<WebMovieDetailed>().Where(x => x.Id == movieId).First();
        }

        public IEnumerable<WebGenre> GetAllGenres()
        {
            string sql = "SELECT DISTINCT genres FROM movie_info";
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
            return new WebFileInfo(PathUtil.StripFileProtocolPrefix(path));
        }

        public Stream GetFile(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public WebDictionary<string> GetExternalMediaInfo(WebMediaType type, string id)
        {
            return new WebDictionary<string>()
            {
                { "Type", "moving pictures" },
                { "Id", id }
            };
        }

    public WebBoolResult SetWathcedStatus(string id, Boolean isWatched)
    {
      Log.Info("SetWathcedStatus provider = 3 idmovie = {0} isWatched = {1}", id, isWatched);
      try
      {
        string strSQL = String.Format("select watched from user_movie_settings WHERE id = {0} and user = 1", id);
        Boolean ret;

        using (DatabaseConnection connection = OpenConnection())
        {
          using (Query query = new Query(connection, strSQL))
          {
            ret = query.Reader.Read();
          }
        }

        if (!ret)
        {
          Log.Error("SetWathcedStatus missing info in user_movie_settings");
          return false;
        }

        if (isWatched)
        {
          strSQL = String.Format("update user_movie_settings set watched = 1 WHERE id = {0} and user = 1", id);
        }
        else
        {
          strSQL = String.Format("update user_movie_settings set watched = 0 WHERE id = {0} and user = 1", id);
        }

        Execute(strSQL);

        return true;
      }
      catch (SQLiteException ex)
      {
        Log.Error("SetWathcedStatus id = {0} exception {1}", id, ex.Message);
        return false;
      }
    }

    public WebBoolResult SetMovieStoptime(string id, int stopTime, Boolean isWatched, int watchedPercent)
    {
      Log.Info("SetMovieStoptime provider = 3 idmovie = {0} stopTime = {1} isWatched = {2}", id, stopTime, isWatched);
      try
      {
        string strSQL = String.Format("select watched from user_movie_settings WHERE id = {0} and user = 1", id);
        Boolean ret;

        using (DatabaseConnection connection = OpenConnection())
        {
          using (Query query = new Query(connection, strSQL))
          {
            ret = query.Reader.Read();
          }
        }

        if (!ret)
        {
          Log.Error("SetMovieStoptime missing info in user_movie_settings");
          return false;
        }

        if (isWatched)
        {
          strSQL = String.Format("update user_movie_settings set watched = (select watched+1 from user_movie_settings  WHERE id = {0} and user = 1), resume_time = {1} WHERE id = {0} and user = 1", id, stopTime);
        }
        else
        {
          strSQL = String.Format("update user_movie_settings set resume_time = {1} WHERE id = {0} and user = 1", id, stopTime);
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
  }
}
