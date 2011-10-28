#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.PlugIns.MAS.MPTVSeries
{
    [Export(typeof(ITVShowLibrary))]
    [ExportMetadata("Name", "MP-TVSeries")]
    [ExportMetadata("Type", typeof(MPTVSeries))]
    public class MPTVSeries : Database, ITVShowLibrary
    {
        private IPluginData data;
        private SQLFieldMapping.ReadValue fixBannerPathReader;

        [ImportingConstructor]
        public MPTVSeries(IPluginData data)
        {
            this.data = data;
            this.fixBannerPathReader = delegate(SQLiteDataReader reader, int index)
            {
                return ((IEnumerable<string>)DataReaders.ReadPipeList(reader, index)).Select(x => this.CreateImagePath("banner", x)).ToList();
            };
        }

        public void Init()
        {
            DatabasePath = data.Configuration["database"];
        }

        private string CreateImagePath(string type, string dbPath)
        {
            string rootDir = data.Configuration[type];
            return Path.Combine(rootDir, dbPath.Replace('/', '\\'));
        }

        public IEnumerable<WebTVShowBasic> GetAllTVShowsBasic()
        {
            return GetAllTVShows<WebTVShowBasic>();
        }

        public IEnumerable<WebTVShowDetailed> GetAllTVShowsDetailed()
        {
            return GetAllTVShows<WebTVShowDetailed>();
        }

        public WebTVShowDetailed GetTVShowDetailed(string seriesId)
        {
            return GetAllTVShows<WebTVShowDetailed>().Where(x => x.Id == seriesId).First();
        }

        public WebTVShowBasic GetTVShowBasic(string seriesId)
        {
            return GetAllTVShows<WebTVShowBasic>().Where(x => x.Id == seriesId).First();
        }

        private class WatchedCount
        {
            public int WatchedEpisodes { get; set; }
            public int UnwatchedEpisodes { get; set; }

            public WatchedCount()
            {
                WatchedEpisodes = 0;
                UnwatchedEpisodes = 0;
            }
        }

        private LazyQuery<T> GetAllTVShows<T>() where T : WebTVShowBasic, new()
        {
            // pre-read watched information
            var watchedCount = new Dictionary<string, WatchedCount>();
            string watchQuery =
                "SELECT e.SeriesID, e.Watched, COUNT(*) AS cnt " + 
                "FROM online_episodes e " +
                "INNER JOIN local_episodes l ON e.CompositeID = l.CompositeID " +
                "WHERE e.Hidden = 0 " + 
                "GROUP BY e.Watched, e.SeriesID";
            ReadList<bool>(watchQuery, delegate(SQLiteDataReader reader) {
                var seriesId = reader.GetInt32(0).ToString();
                if (!watchedCount.ContainsKey(seriesId))
                {
                    watchedCount[seriesId] = new WatchedCount();
                }

                var watched = reader.GetInt32(1);
                var count = reader.GetInt32(2);
                if (watched == 0)
                {
                    watchedCount[seriesId].UnwatchedEpisodes = count;
                }
                else
                {
                    watchedCount[seriesId].WatchedEpisodes = count;
                }

                return true;
            });


            SQLFieldMapping.ReadValue fixNameReader = delegate(SQLiteDataReader reader, int index)
            {
                // MPTvSeries does some magic with the name: if it's empty in the online series, use the Parsed_Name from the local series. I prefer
                // a complete database, but we can't fix that easily. See DB Classes/DBSeries.cs:359 in MPTvSeries source
                string data = reader.ReadString(index);
                if(data.Length > 0)
                    return data;
                return reader.ReadString(index - 1);
            };

            SQLFieldMapping.ReadValue fixFanartPathReader = delegate(SQLiteDataReader reader, int index)
            {
                return ((IEnumerable<string>)DataReaders.ReadPipeList(reader, index)).Select(x => this.CreateImagePath("fanart", x)).ToList();
            };

            string sql = 
                    "SELECT DISTINCT s.ID, l.Parsed_Name, s.Pretty_Name, s.Genre, s.BannerFileNames, STRFTIME('%Y', s.FirstAired) AS year, " +
                        "s.PosterFileNames, s.fanart, s.Actors, s.Summary, s.Network, s.AirsDay, s.AirsTime, s.Runtime, s.Rating, s.ContentRating, s.Status, " +
                        "s.IMDB_ID, s.added " +
                    "FROM online_series AS s " +
                    "INNER JOIN local_series AS l ON s.ID = l.ID AND l.Hidden = 0 " +
                    "WHERE s.ID != 0 AND s.HasLocalFiles = 1 AND %where " +
                    "%order";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>() {
                new SQLFieldMapping("s", "ID", "Id", DataReaders.ReadIntAsString),
                new SQLFieldMapping("s", "ID", "TVDBId", DataReaders.ReadIntAsString),
                new SQLFieldMapping("s", "Pretty_Name", "Title", fixNameReader),
                new SQLFieldMapping("s", "Genre", "Genres", DataReaders.ReadPipeList),
                new SQLFieldMapping("s", "BannerFileNames", "BannerPaths", fixBannerPathReader),
                new SQLFieldMapping("", "year", "Year", DataReaders.ReadStringAsInt),
                new SQLFieldMapping("s", "PosterFileNames", "PosterPaths", fixBannerPathReader),
                new SQLFieldMapping("s", "fanart", "BackdropPaths", fixFanartPathReader),
                new SQLFieldMapping("s", "Actors", "Actors", DataReaders.ReadPipeList),
                new SQLFieldMapping("s", "Rating", "Rating", DataReaders.ReadFloat),
                new SQLFieldMapping("s", "ContentRating", "ContentRating", DataReaders.ReadString),
                new SQLFieldMapping("s", "Summary", "Summary", DataReaders.ReadString),
                new SQLFieldMapping("s", "Status", "Status", DataReaders.ReadString),
                new SQLFieldMapping("s", "Network", "Network", DataReaders.ReadString),
                new SQLFieldMapping("s", "AirsDay", "AirsDay", DataReaders.ReadString),
                new SQLFieldMapping("s", "AirsTime", "AirsTime", DataReaders.ReadString),
                new SQLFieldMapping("s", "Runtime", "Runtime", DataReaders.ReadInt32),
                new SQLFieldMapping("s", "IMDB_ID", "IMDBId", DataReaders.ReadString),
                new SQLFieldMapping("s", "added", "DateAdded", DataReaders.ReadDateTime)
            }, delegate(T obj)
            {
                // cannot rely on information provided by MPTVSeries here because they count different
                var eps = (LazyQuery<WebTVEpisodeBasic>)(GetAllEpisodes<WebTVEpisodeBasic>().Where(x => x.ShowId == obj.Id)); // and the nice way is... ? 
                obj.EpisodeCount = watchedCount[obj.Id].WatchedEpisodes + watchedCount[obj.Id].UnwatchedEpisodes;
                obj.UnwatchedEpisodeCount = watchedCount[obj.Id].UnwatchedEpisodes;
                return obj;
            });
        }


        public IEnumerable<WebTVSeasonBasic> GetAllSeasonsBasic()
        {
            return GetAllSeasons<WebTVSeasonBasic>();
        }

        public IEnumerable<WebTVSeasonDetailed> GetAllSeasonsDetailed()
        {
            return GetAllSeasons<WebTVSeasonDetailed>();
        }

        public WebTVSeasonBasic GetSeasonBasic(string seasonId)
        {
            return GetAllSeasons<WebTVSeasonBasic>().Where(x => x.Id == seasonId).First();
        }

        public WebTVSeasonDetailed GetSeasonDetailed(string seasonId)
        {
            return GetAllSeasons<WebTVSeasonDetailed>().Where(x => x.Id == seasonId).First();
        }

        private LazyQuery<T> GetAllSeasons<T>() where T : WebTVSeasonBasic, new()
        {
            string csql = "SELECT SeriesID, SeasonIndex, COUNT(*) AS count FROM online_episodes GROUP BY SeriesID, SeasonIndex";
            var episodeCountTable = ReadList<KeyValuePair<string, int>>(csql, delegate(SQLiteDataReader reader)
            {
                return new KeyValuePair<string, int>(reader.ReadIntAsString(0) + "_s" + reader.ReadIntAsString(1), reader.ReadInt32(2));
            }).ToDictionary(x => x.Key, x => x.Value);

            string wsql = "SELECT SeriesID, SeasonIndex, COUNT(*) AS count FROM online_episodes WHERE Watched = 0 GROUP BY SeriesID, SeasonIndex";
            var episodeUnwatchedCountTable = ReadList<KeyValuePair<string, int>>(wsql, delegate(SQLiteDataReader reader)
            {
                return new KeyValuePair<string, int>(reader.ReadIntAsString(0) + "_s" + reader.ReadIntAsString(1), reader.ReadInt32(2));
            }).ToDictionary(x => x.Key, x => x.Value);

            string sql = 
                    "SELECT DISTINCT s.ID, s.SeriesID, s.SeasonIndex, STRFTIME('%Y', e.FirstAired) AS year, " +
                        "s.BannerFileNames " +
                    "FROM season s " +
                    "INNER JOIN online_episodes e ON e.EpisodeIndex = 1 AND e.SeasonIndex = s.SeasonIndex AND e.SeriesID = s.SeriesID " +
                    "WHERE %where " +
                    "%order";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>() {
                new SQLFieldMapping("s", "ID", "Id", DataReaders.ReadString),
                new SQLFieldMapping("s", "SeasonIndex", "SeasonNumber", DataReaders.ReadInt32),
                new SQLFieldMapping("s", "SeriesID", "ShowId", DataReaders.ReadIntAsString),
                new SQLFieldMapping("", "year", "Year", DataReaders.ReadStringAsInt),
                new SQLFieldMapping("s", "BannerFileNames", "BannerPaths", fixBannerPathReader)
            }, delegate(T obj)
            {
                obj.EpisodeCount = episodeCountTable.ContainsKey(obj.Id) ? episodeCountTable[obj.Id] : 0;
                obj.UnwatchedEpisodeCount = episodeUnwatchedCountTable.ContainsKey(obj.Id) ? episodeUnwatchedCountTable[obj.Id] : 0;
                return obj;
            });
        }

        public IEnumerable<WebTVEpisodeBasic> GetAllEpisodesBasic()
        {
            return GetAllEpisodes<WebTVEpisodeBasic>();
        }

        public IEnumerable<WebTVEpisodeDetailed> GetAllEpisodesDetailed()
        {
            return GetAllEpisodes<WebTVEpisodeDetailed>();
        }

        public WebTVEpisodeBasic GetEpisodeBasic(string episodeId)
        {
            return GetAllEpisodes<WebTVEpisodeBasic>().Where(x => x.Id == episodeId).First();
        }

        public WebTVEpisodeDetailed GetEpisodeDetailed(string episodeId)
        {
            return GetAllEpisodes<WebTVEpisodeDetailed>().Where(x => x.Id == episodeId).First();
        }       

        private LazyQuery<T> GetAllEpisodes<T>() where T : WebTVEpisodeBasic, new()
        {
            string sql =                     
                    "SELECT e.EpisodeID, e.EpisodeName, e.EpisodeIndex, e.SeriesID, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                        "e.FirstAired, GROUP_CONCAT(l.EpisodeFilename, '|') AS filename, " +
                        "e.GuestStars, e.Director, e.Writer, e.IMDB_ID, e.Summary " +
                    "FROM online_episodes e " +
                    "INNER JOIN local_episodes l ON e.CompositeID = l.CompositeID " +
                    "WHERE e.Hidden = 0 AND %where " +
                    "GROUP BY e.EpisodeID, e.SeriesID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                         "e.FirstAired, e.GuestStars, e.Director, e.Writer " +
                    "%order";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>()
            {
                new SQLFieldMapping("e", "EpisodeID", "Id", DataReaders.ReadIntAsString),
                new SQLFieldMapping("e", "EpisodeID", "TVDBId", DataReaders.ReadIntAsString),
                new SQLFieldMapping("e", "EpisodeName", "Title", DataReaders.ReadString),
                new SQLFieldMapping("e", "SeriesID", "ShowId", DataReaders.ReadIntAsString),
                new SQLFieldMapping("e", "EpisodeIndex", "EpisodeNumber", DataReaders.ReadInt32),
                new SQLFieldMapping("e", "SeasonIndex", "SeasonId", ReadSeasonID),
                new SQLFieldMapping("e", "SeasonIndex", "SeasonNumber", DataReaders.ReadInt32),
                new SQLFieldMapping("", "filename", "Path", DataReaders.ReadPipeList),
                new SQLFieldMapping("e", "FirstAired", "FirstAired", DataReaders.ReadDateTime),
                new SQLFieldMapping("e", "Watched", "Watched", DataReaders.ReadBoolean),
                new SQLFieldMapping("e", "Rating", "Rating", DataReaders.ReadFloat),
                new SQLFieldMapping("e", "thumbFilename", "BannerPaths", fixBannerPathReader),
                new SQLFieldMapping("e", "GuestStars", "GuestStars", DataReaders.ReadPipeList),
                new SQLFieldMapping("e", "Director", "Directors", DataReaders.ReadPipeList),
                new SQLFieldMapping("e", "Writer", "Writers", DataReaders.ReadPipeList),
                new SQLFieldMapping("e", "IMDB_ID", "IMDBId", DataReaders.ReadString),
                new SQLFieldMapping("e", "Summary", "Summary", DataReaders.ReadString)
            });
        }

        [AllowSQLCompare("(%table.SeriesID || '_s' || %table.SeasonIndex) = %prepared")]
        [AllowSQLSort("%table.SeriesID %order, %table.SeasonIndex %order")]
        private string ReadSeasonID(SQLiteDataReader reader, int offset)
        {
            return DataReaders.ReadIntAsString(reader, offset - 1) + "_s" + DataReaders.ReadIntAsString(reader, offset);
        }


        public IEnumerable<WebSearchResult> Search(string text)
        {
            SQLiteParameter param = new SQLiteParameter("@search", "%" + text + "%");
            string showSql = "SELECT ID, Pretty_Name FROM online_series WHERE Pretty_Name LIKE @search";
            IEnumerable<WebSearchResult> shows = ReadList<WebSearchResult>(showSql, delegate(SQLiteDataReader reader)
            {
                string title = reader.ReadString(1);
                return new WebSearchResult()
                {
                    Type = WebMediaType.TVShow,
                    Id = reader.ReadIntAsString(0),
                    Title = title,
                    Score = (int)Math.Round((decimal)text.Length / title.Length * 100)
                };
            }, param);

            string episodeSql = "SELECT CompositeID, EpisodeName FROM online_episodes WHERE EpisodeName LIKE @search";
            IEnumerable<WebSearchResult> episodes = ReadList<WebSearchResult>(episodeSql, delegate(SQLiteDataReader reader)
            {
                string title = reader.ReadString(1);
                return new WebSearchResult()
                {
                    Type = WebMediaType.TVEpisode,
                    Id = reader.ReadString(0),
                    Title = title,
                    Score = (int)Math.Round((decimal)text.Length / title.Length * 100)
                };
            }, param);

            return shows.Union(episodes);
        }

        public IEnumerable<WebGenre> GetAllGenres()
        {
            string sql = "SELECT DISTINCT Genre FROM online_series";
            return ReadList<IEnumerable<string>>(sql, delegate(SQLiteDataReader reader)
                {
                    return reader.ReadPipeList(0);
                })
                    .SelectMany(x => x)
                    .Distinct()
                    .OrderBy(x => x)
                    .Select(x => new WebGenre() { Name = x });
        }

        public IEnumerable<WebCategory> GetAllCategories()
        {
            return new List<WebCategory>();
        }

        public WebFileInfo GetFileInfo(string path)
        {
            return new WebFileInfo(path);
        }

        public Stream GetFile(string path)
        {
            return new FileStream(path, FileMode.Open);
        }

        public WebExternalMediaInfo GetExternalMediaInfo(WebMediaType type, string id)
        {
            if (type == WebMediaType.TVSeason)
            {
                var season = GetSeasonBasic(id);
                return new WebExternalMediaInfoSeason()
                {
                    Type = "mptvseries season",
                    SeasonIndex = season.SeasonNumber,
                    ShowId = Int32.Parse(season.ShowId)
                };
            }

            return new WebExternalMediaInfoId()
            {
                Type = type == WebMediaType.TVShow ? "mptvseries show" : "mptvseries episode",
                Id = id
            };
        }
    }
}