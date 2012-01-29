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
using System.IO;
using System.Linq;
using System.Data.SQLite;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.PlugIns.MAS.MPVideos
{
    [Export(typeof(IMovieLibrary))]
    [ExportMetadata("Name", "MP MyVideo")]
    [ExportMetadata("Id", 7)]
    public class MPVideos : Database, IMovieLibrary
    {
        public bool Supported { get; set; }

        [ImportingConstructor]
        public MPVideos(IPluginData data)
        {
            DatabasePath = data.GetConfiguration("MP MyVideo")["database"];
            Supported = File.Exists(DatabasePath);
        }

        private List<WebActor> ActorReader(SQLiteDataReader reader, int idx)
        {
            return ((IList<string>)DataReaders.ReadPipeList(reader, idx)).Select(x => new WebActor() { Name = x }).ToList();
        }

        private LazyQuery<T> LoadMovies<T>() where T : WebMovieBasic, new()
        {
            string sql =
                "SELECT m.idMovie, i.strTitle, i.iYear, i.fRating, i.runtime, i.IMDBID, i.strPlot, i.strPictureURL, " +
                    "GROUP_CONCAT(p.strPath || f.strFilename, '|') AS fullpath, " +
                    "GROUP_CONCAT(a.strActor, '|') AS actors, " +
                    "GROUP_CONCAT(g.strGenre, '|') AS genres " + 
                "FROM movie m " +
                "INNER JOIN movieinfo i ON m.idMovie = i.idMovie " +
                "LEFT JOIN files f ON m.idMovie = f.idMovie " +
                "INNER JOIN path p ON f.idPath = p.idPath " +
                "LEFT JOIN actorlinkmovie alm ON m.idMovie = alm.idMovie " +
                "INNER JOIN actors a ON alm.idActor = a.idActor " +
                "LEFT JOIN genrelinkmovie glm ON m.idMovie = glm.idMovie " +
                "INNER JOIN genre g ON glm.idGenre = g.idGenre " + 
                "WHERE %where " +
                "GROUP BY m.idMOvie, i.strTitle, i.iYear, i.fRating, i.runtime, i.IMDBID, i.strPlot, i.strPictureURL";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>()
            {
                new SQLFieldMapping("m", "idMovie", "Id", DataReaders.ReadIntAsString),
                new SQLFieldMapping("fullpath", "Path", DataReaders.ReadPipeList),
                new SQLFieldMapping("actors", "Actors", ActorReader),
                new SQLFieldMapping("genres", "Genres", DataReaders.ReadPipeList),
                new SQLFieldMapping("i", "strPictureURL", "Artwork", ArtworkRetriever.ArtworkReader),
                new SQLFieldMapping("i", "strTitle", "Title", DataReaders.ReadString),
                new SQLFieldMapping("i", "iYear", "Year", DataReaders.ReadInt32),
                new SQLFieldMapping("i", "fRating", "Rating", DataReaders.ReadStringAsFloat),
                new SQLFieldMapping("i", "runtime", "Runtime", DataReaders.ReadInt32),
                new SQLFieldMapping("i", "IMDBID", "IMDBId", DataReaders.ReadString),
                new SQLFieldMapping("i", "strPlot", "Summary", DataReaders.ReadString),
            });
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

        public IEnumerable<WebGenre> GetAllGenres()
        {
            string sql = "SELECT strGenre FROM genre";
            return new LazyQuery<WebGenre>(this, sql, new List<SQLFieldMapping>()
            {
                new SQLFieldMapping("strGenre", "Title", DataReaders.ReadString)
            });
        }

        public IEnumerable<WebCategory> GetAllCategories()
        {
            return new List<WebCategory>();
        }

        public WebFileInfo GetFileInfo(string path)
        {
            if(path.StartsWith("http://"))
            {
                return ArtworkRetriever.GetFileInfo(path);
            }

            return new WebFileInfo(new FileInfo(path));
        }

        public Stream GetFile(string path)
        {
            if (path.StartsWith("http://"))
            {
                return ArtworkRetriever.GetStream(path);
            }

            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public IEnumerable<WebSearchResult> Search(string text)
        {
            OpenDatabase();
            var param = new SQLiteParameter("@search", "%" + text + "%");
            string sql = "SELECT idMovie, strTitle, iYear, strGenre FROM movieinfo WHERE strTitle LIKE @search";
            IEnumerable<WebSearchResult> titleResults = ReadList<WebSearchResult>(sql, delegate (SQLiteDataReader reader) 
            {
                string title = reader.ReadString(1);
                string genres = reader.ReadString(3);
                return new WebSearchResult()
                {
                    Type = WebMediaType.Movie,
                    Id = reader.ReadIntAsString(0),
                    Title = title,
                    Score = (int)Math.Round(40 + (decimal)text.Length / title.Length * 40),
                    Details = new SerializableDictionary<string>()
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
                    Details = new SerializableDictionary<string>()
                    {
                        { "Year", reader.ReadIntAsString(3) },
                        { "Genres", genres == "unknown" ? String.Empty : genres }
                    }
                };
            }, param);

            CloseDatabase();
            return titleResults.Concat(actorResults);
        }

        public SerializableDictionary<string> GetExternalMediaInfo(WebMediaType type, string id)
        {
            return new SerializableDictionary<string>()
            {
                { "Type", "myvideos" },
                { "Id", id }
            };
        }
    }
}
