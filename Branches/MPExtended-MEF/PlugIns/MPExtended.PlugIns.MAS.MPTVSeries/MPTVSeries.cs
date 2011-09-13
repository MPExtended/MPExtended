#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
    [ExportMetadata("Database", "MPTVSeries")]
    public class MPTVSeries : Database, ITVShowLibrary
    {
        private IPluginData data;
        private SQLFieldMapping.ReadValue fixBannerPathReader;

        [ImportingConstructor]
        public MPTVSeries(IPluginData data) : base(data.Configuration["database"])
        {
            this.data = data;
            this.fixBannerPathReader = delegate(SQLiteDataReader reader, int index)
            {
                return ((IEnumerable<string>)DataReaders.ReadPipeList(reader, index)).Select(x => this.CreateImagePath("banner", x)).ToList();
            };
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

        private IEnumerable<T> GetAllTVShows<T>() where T: WebTVShowBasic, new()
        {
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
                        "s.PosterFileNames, s.fanart, s.Actors " +
                    "FROM online_series AS s " +
                    "INNER JOIN local_series AS l ON s.ID = l.ID AND l.Hidden = 0 " +
                    "WHERE s.ID != 0 AND s.HasLocalFiles = 1 AND %where " +
                    "%order";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>() {
                new SQLFieldMapping("s", "ID", "Id", DataReaders.ReadIntAsString),
                new SQLFieldMapping("s", "Pretty_Name", "Title", fixNameReader),
                new SQLFieldMapping("s", "Genre", "Genres", DataReaders.ReadPipeList),
                new SQLFieldMapping("s", "BannerFileNames", "BannerPaths", fixBannerPathReader),
                new SQLFieldMapping("", "year", "Year", DataReaders.ReadStringAsInt),
                new SQLFieldMapping("s", "PosterFileNames", "PosterPaths", fixBannerPathReader),
                new SQLFieldMapping("s", "fanart", "BackdropPaths", fixFanartPathReader),
                new SQLFieldMapping("s", "Actors", "Actors", DataReaders.ReadPipeList),
            });
        }

        public WebTVShowDetailed GetTVShowDetailed(string seriesId)
        {
            return GetAllTVShowsDetailed().Where(x => x.Id == seriesId).First();
        }

        public IEnumerable<WebTVSeasonBasic> GetAllSeasonsBasic(string seriesId)
        {
            return GetAllSeasons<WebTVSeasonBasic>(seriesId);
        }

        public IEnumerable<WebTVSeasonDetailed> GetAllSeasonsDetailed(string seriesId)
        {
            return GetAllSeasons<WebTVSeasonDetailed>(seriesId);
        }

        public IEnumerable<T> GetAllSeasons<T>(string seriesId) where T : WebTVSeasonBasic, new()
        {
            string sql = 
                    "SELECT DISTINCT s.ID, s.SeasonIndex, s.SeriesID, STRFTIME('%Y', e.FirstAired) AS year, " +
                        "BannerFileNames " +
                    "FROM season s " +
                    "LEFT JOIN online_episodes e ON e.EpisodeIndex = 1 AND e.SeasonIndex = s.SeasonIndex AND e.SeriesID = @seriesId " +
                    "WHERE s.SeriesID = @seriesId AND %where " +
                    "%order";
            var parameters = new SQLiteParameter[] { new SQLiteParameter("@seriesId", seriesId) };
            return new LazyQuery<T>(this, sql, parameters, new List<SQLFieldMapping>() {
                new SQLFieldMapping("s", "ID", "Id", DataReaders.ReadString),
                new SQLFieldMapping("s", "SeasonIndex", "SeasonNumber", DataReaders.ReadInt32),
                new SQLFieldMapping("s", "SeriesID", "ShowId", DataReaders.ReadIntAsString),
                new SQLFieldMapping("", "year", "Year", DataReaders.ReadStringAsInt),
                new SQLFieldMapping("", "BannerFileNames", "BannerPaths", fixBannerPathReader)
            });
        }

        public WebTVSeasonDetailed GetSeasonDetailed(string seriesId, string seasonId)
        {
            return GetAllSeasonsDetailed(seriesId).Where(x => x.Id == seasonId).First();
        }

        public IEnumerable<WebTVEpisodeBasic> GetAllEpisodesBasic()
        {
            return GetAllEpisodes<WebTVEpisodeBasic>();
        }

        public IEnumerable<WebTVEpisodeDetailed> GetAllEpisodesDetailed()
        {
            return GetAllEpisodes<WebTVEpisodeDetailed>();
        }

        private IEnumerable<T> GetAllEpisodes<T>() where T : WebTVEpisodeBasic, new()
        {
            SQLFieldMapping.ReadValue seasonIdReader = delegate (SQLiteDataReader reader, int index) { 
                return reader.ReadIntAsString(index - 1) + "_s" + reader.ReadIntAsString(index); 
            };
            string sql =                     
                    "SELECT e.EpisodeID, e.EpisodeName, e.EpisodeIndex, e.SeriesID, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                        "e.FirstAired, GROUP_CONCAT(l.EpisodeFilename, '|') AS filename, " +
                        "e.GuestStars, e.Director, e.Writer " +
                    "FROM online_episodes e " +
                    "INNER JOIN local_episodes l ON e.CompositeID = l.CompositeID " +
                    "WHERE e.Hidden = 0 AND %where " +
                    "GROUP BY e.EpisodeID, e.SeriesID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                         "e.FirstAired, e.GuestStars, e.Director, e.Writer " +
                    "%order";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>()
            {
                new SQLFieldMapping("e", "EpisodeID", "Id", DataReaders.ReadIntAsString),
                new SQLFieldMapping("e", "SeriesID", "ShowId", DataReaders.ReadIntAsString),
                new SQLFieldMapping("e", "EpisodeName", "Title", DataReaders.ReadString),
                new SQLFieldMapping("e", "EpisodeIndex", "EpisodeNumber", DataReaders.ReadInt32),
                new SQLFieldMapping("e", "SeasonIndex", "SeasonId", 
                    delegate (SQLiteDataReader reader, int index) { return reader.ReadIntAsString(index - 1) + "_s" + reader.ReadIntAsString(index); }),
                new SQLFieldMapping("", "filename", "Path", DataReaders.ReadPipeList),
                new SQLFieldMapping("e", "FirstAired", "FirstAired", DataReaders.ReadDateTime),
                new SQLFieldMapping("e", "Watched", "Watched", DataReaders.ReadBoolean),
                new SQLFieldMapping("e", "Rating", "Rating", DataReaders.ReadFloat),
                new SQLFieldMapping("e", "thumbFilename", "BannerPaths", fixBannerPathReader),
                new SQLFieldMapping("e", "GuestStars", "GuestStars", DataReaders.ReadPipeList),
                new SQLFieldMapping("e", "Director", "Directors", DataReaders.ReadPipeList),
                new SQLFieldMapping("e", "Writer", "Writers", DataReaders.ReadPipeList)
            });
        }

        public WebTVEpisodeDetailed GetEpisodeDetailed(string episodeId)
        {
            return GetAllEpisodesDetailed().Where(x => x.Id == episodeId).First();
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

        public Stream GetBanner(string seriesId, int offset)
        {
            return new FileStream(GetTVShowDetailed(seriesId).BannerPaths[offset], FileMode.Open);
        }

        public Stream GetPoster(string seriesId, int offset)
        {
            return new FileStream(GetTVShowDetailed(seriesId).PosterPaths[offset], FileMode.Open);
        }

        public Stream GetBackdrop(string seriesId, int offset)
        {
            return new FileStream(GetTVShowDetailed(seriesId).BackdropPaths[offset], FileMode.Open);
        }

        public Stream GetSeasonBanner(string seriesId, string seasonId, int offset)
        {
            return new FileStream(GetSeasonDetailed(seriesId, seasonId).BannerPaths[offset], FileMode.Open);
        }

        public Stream GetSeasonPoster(string seriesId, string seasonId, int offset)
        {
            return new FileStream(GetSeasonDetailed(seriesId, seasonId).PosterPaths[offset], FileMode.Open);
        }

        public Stream GetSeasonBackdrop(string seriesId, string seasonId, int offset)
        {
            return new FileStream(GetSeasonDetailed(seriesId, seasonId).BackdropPaths[offset], FileMode.Open);
        }
    }
}
