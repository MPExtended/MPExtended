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
using System.Text;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.PlugIns.MAS.MPTVSeries
{
    [Export(typeof(ITVShowLibrary))]
    [ExportMetadata("Name", "MP-TVSeries")]
    [ExportMetadata("Id", 6)]
    public partial class MPTVSeries : Database, ITVShowLibrary
    {
        private Dictionary<string, string> configuration;
        public bool Supported { get; set; }

        [ImportingConstructor]
        public MPTVSeries(IPluginData data)
        {
            this.configuration = data.GetConfiguration("MP-TVSeries");
            this.DatabasePath = configuration["database"];
            Supported = File.Exists(DatabasePath);
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

            string sql = 
                    "SELECT DISTINCT s.ID, MIN(l.Parsed_Name) AS parsed_name, s.Pretty_Name, s.Genre, s.BannerFileNames, STRFTIME('%Y', s.FirstAired) AS year, " +
                        "s.PosterFileNames, s.fanart, s.Actors, s.Summary, s.Network, s.AirsDay, s.AirsTime, s.Runtime, s.Rating, s.ContentRating, s.Status, " +
                        "s.IMDB_ID, s.added " +
                    "FROM online_series AS s " +
                    "INNER JOIN local_series AS l ON s.ID = l.ID AND l.Hidden = 0 " +
                    "WHERE s.ID != 0 AND s.HasLocalFiles = 1 AND %where " +
                    "GROUP BY s.ID, s.Pretty_Name, s.Genre, s.BannerFileNames, s.FirstAired, " +
                        "s.PosterFileNames, s.fanart, s.Actors, s.Summary, s.Network, s.AirsDay, s.AirsTime, s.Runtime, s.Rating, s.ContentRating, s.Status, " +
                        "s.IMDB_ID, s.added " +
                    "%order";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>() {
                new SQLFieldMapping("s", "ID", "Id", DataReaders.ReadIntAsString),
                new SQLFieldMapping("s", "ID", "ExternalId", CustomReaders.ExternalIdReader, new ExternalSiteReaderParameters(DataReaders.ReadIntAsString, "TVDB")),
                new SQLFieldMapping("s", "Pretty_Name", "Title", CustomReaders.FixNameReader),
                new SQLFieldMapping("s", "Genre", "Genres", DataReaders.ReadPipeList),
                new SQLFieldMapping("s", "BannerFileNames", "Artwork", CustomReaders.ArtworkReader, new ArtworkReaderParameters(WebFileType.Banner, configuration["banner"])),
                new SQLFieldMapping("", "year", "Year", DataReaders.ReadStringAsInt),
                new SQLFieldMapping("s", "PosterFileNames", "Artwork", CustomReaders.ArtworkReader, new ArtworkReaderParameters(WebFileType.Poster, configuration["banner"])),
                new SQLFieldMapping("s", "fanart", "Artwork", CustomReaders.ArtworkReader, new ArtworkReaderParameters(WebFileType.Backdrop, configuration["fanart"])),
                new SQLFieldMapping("s", "Actors", "Actors", CustomReaders.ActorReader),
                new SQLFieldMapping("s", "Rating", "Rating", DataReaders.ReadFloat),
                new SQLFieldMapping("s", "ContentRating", "ContentRating", DataReaders.ReadString),
                new SQLFieldMapping("s", "Summary", "Summary", DataReaders.ReadString),
                new SQLFieldMapping("s", "Status", "Status", DataReaders.ReadString),
                new SQLFieldMapping("s", "Network", "Network", DataReaders.ReadString),
                new SQLFieldMapping("s", "AirsDay", "AirsDay", DataReaders.ReadString),
                new SQLFieldMapping("s", "AirsTime", "AirsTime", DataReaders.ReadString),
                new SQLFieldMapping("s", "Runtime", "Runtime", DataReaders.ReadInt32),
                new SQLFieldMapping("s", "IMDB_ID", "ExternalId", CustomReaders.ExternalIdReader, new ExternalSiteReaderParameters(DataReaders.ReadString, "IMDB")),
                new SQLFieldMapping("s", "added", "DateAdded", DataReaders.ReadDateTime)
            }, delegate (T obj) 
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
            // preload watched count
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

            // and load seasons
            string sql = 
                    "SELECT DISTINCT s.ID, s.SeriesID, s.SeasonIndex, STRFTIME('%Y', MIN(e.FirstAired)) AS year, " +
                        "s.BannerFileNames " +
                    "FROM season s " +
                    "LEFT JOIN online_episodes e ON e.SeasonIndex = s.SeasonIndex AND e.SeriesID = s.SeriesID " +
                    "WHERE %where " +
                    "GROUP BY s.ID, s.SeriesID, s.SeasonIndex " + 
                    "%order";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>() {
                new SQLFieldMapping("s", "ID", "Id", DataReaders.ReadString),
                new SQLFieldMapping("s", "SeasonIndex", "SeasonNumber", DataReaders.ReadInt32),
                new SQLFieldMapping("s", "SeriesID", "ShowId", DataReaders.ReadIntAsString),
                new SQLFieldMapping("", "year", "Year", DataReaders.ReadStringAsInt),
                new SQLFieldMapping("s", "BannerFileNames", "Artwork", CustomReaders.ArtworkReader, new ArtworkReaderParameters(WebFileType.Banner, configuration["banner"]))
            }, delegate (T obj) {
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
                        "e.GuestStars, e.Director, e.Writer, e.IMDB_ID, e.Summary, " +
                        "MIN(ls.Parsed_Name) AS parsed_name, os.Pretty_Name AS pretty_name " + 
                    "FROM online_episodes e " +
                    "INNER JOIN local_episodes l ON e.CompositeID = l.CompositeID " +
                    "INNER JOIN online_series os ON os.ID = e.SeriesID " +
                    "INNER JOIN local_series ls ON ls.ID = e.SeriesID " +
                    "WHERE e.Hidden = 0 AND %where " +
                    "GROUP BY e.EpisodeID, e.SeriesID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                         "e.FirstAired, e.GuestStars, e.Director, e.Writer " +
                    "%order";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>()
            {
                new SQLFieldMapping("e", "EpisodeID", "Id", DataReaders.ReadIntAsString),
                new SQLFieldMapping("e", "EpisodeID", "ExternalId", CustomReaders.ExternalIdReader, new ExternalSiteReaderParameters(DataReaders.ReadIntAsString, "TVDB")),
                new SQLFieldMapping("e", "EpisodeName", "Title", DataReaders.ReadString),
                new SQLFieldMapping("e", "SeriesID", "ShowId", DataReaders.ReadIntAsString),
                new SQLFieldMapping("e", "EpisodeIndex", "EpisodeNumber", DataReaders.ReadInt32),
                new SQLFieldMapping("e", "SeasonIndex", "SeasonId", CustomReaders.ReadSeasonID),
                new SQLFieldMapping("e", "SeasonIndex", "SeasonNumber", DataReaders.ReadInt32),
                new SQLFieldMapping("", "filename", "Path", DataReaders.ReadPipeList),
                new SQLFieldMapping("e", "FirstAired", "FirstAired", DataReaders.ReadDateTime),
                new SQLFieldMapping("e", "Watched", "Watched", DataReaders.ReadBoolean),
                new SQLFieldMapping("e", "Rating", "Rating", DataReaders.ReadFloat),
                new SQLFieldMapping("e", "thumbFilename", "Artwork", CustomReaders.ArtworkReader, new ArtworkReaderParameters(WebFileType.Banner, configuration["banner"])),
                new SQLFieldMapping("e", "GuestStars", "GuestStars", DataReaders.ReadPipeList),
                new SQLFieldMapping("e", "Director", "Directors", DataReaders.ReadPipeList),
                new SQLFieldMapping("e", "Writer", "Writers", DataReaders.ReadPipeList),
                new SQLFieldMapping("e", "IMDB_ID", "ExternalId", CustomReaders.ExternalIdReader, new ExternalSiteReaderParameters(DataReaders.ReadString, "IMDB")),
                new SQLFieldMapping("e", "Summary", "Summary", DataReaders.ReadString),
                new SQLFieldMapping("", "pretty_name", "Show", CustomReaders.FixNameReader)
            });
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
                    .Select(x => new WebGenre() { Title = x });
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

        public SerializableDictionary<string> GetExternalMediaInfo(WebMediaType type, string id)
        {
            if (type == WebMediaType.TVSeason)
            {
                var season = GetSeasonBasic(id);
                return new SerializableDictionary<string>()
                {
                    { "Type", "mptvseries season" },
                    { "SeasonIndex", season.SeasonNumber.ToString() },
                    { "ShowId", season.ShowId },
                };
            }

            return new SerializableDictionary<string>()
            {
                { "Type", type == WebMediaType.TVShow ? "mptvseries show" : "mptvseries episode" },
                { "Id", id.ToString() }
            };
        }
    }
}