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

        [ImportingConstructor]
        public MPTVSeries(IPluginData data) : base(data.Configuration["database"])
        {
            this.data = data;
        }

        private string CreateImagePath(string type, string dbPath)
        {
            string rootDir = data.Configuration[type];
            return Path.Combine(rootDir, dbPath.Replace('/', '\\'));
        }

        #region Shows
        public IEnumerable<WebTVShowBasic> GetAllTVShowsBasic()
        {
            string sql = "SELECT DISTINCT s.ID, s.Pretty_Name, l.Parsed_Name, s.Genre, s.BannerFileNames, STRFTIME('%Y', s.FirstAired) AS year " +
                         "FROM online_series AS s " +
                         "INNER JOIN local_series AS l ON s.ID = l.ID AND l.Hidden = 0 " +
                         "WHERE s.ID != 0 AND s.HasLocalFiles = 1";
            List<int> alreadyDone = new List<int>();
            return ReadList<WebTVShowBasic>(
                sql,
                delegate(SQLiteDataReader reader)
                {
                    if (alreadyDone.Contains(reader.ReadInt32(0)))
                        return null;
                    return CreateWebTVShow<WebTVShowBasic>(reader);
                }
            );
        }

        public IEnumerable<WebTVShowDetailed> GetAllTVShowsDetailed()
        {
            string sql = "SELECT DISTINCT s.ID, s.Pretty_Name, l.Parsed_Name, s.Genre, s.BannerFileNames, STRFTIME('%Y', s.FirstAired) AS year, " +
                            "s.PosterFileNames, s.fanart, s.Actors " +
                         "FROM online_series AS s " +
                         "INNER JOIN local_series AS l ON s.ID = l.ID AND l.Hidden = 0 " +
                         "WHERE s.ID != 0 AND s.HasLocalFiles = 1";
            List<int> alreadyDone = new List<int>();
            return ReadList<WebTVShowDetailed>(
                sql,
                delegate(SQLiteDataReader reader)
                {
                    if (alreadyDone.Contains(reader.ReadInt32(0)))
                        return null;
                    return CreateWebTVShowDetailed(reader);
                }
            );
        }

        public WebTVShowDetailed GetTVShowDetailed(string seriesId)
        {
            string sql = "SELECT DISTINCT s.ID, s.Pretty_Name, l.Parsed_Name, s.Genre, s.BannerFileNames, STRFTIME('%Y', s.FirstAired) AS year, " +
                            "s.PosterFileNames, s.fanart, s.Actors " +
                         "FROM online_series AS s " +
                         "INNER JOIN local_series AS l ON s.ID = l.ID AND l.Hidden = 0 " +
                         "WHERE s.ID != 0 AND s.HasLocalFiles = 1 AND s.ID = @seriesId";
            return ReadRow<WebTVShowDetailed>(
                sql,
                delegate(SQLiteDataReader reader)
                {
                    return CreateWebTVShowDetailed(reader);
                },
                new SQLiteParameter("@seriesId", seriesId)
            );
        }

        private T CreateWebTVShow<T>(SQLiteDataReader reader) where T : WebTVShowBasic, new()
        {
            return new T()
            {
                Id = reader.ReadInt32(0).ToString(),
                Title = !String.IsNullOrEmpty(reader.ReadString(1)) ? reader.ReadString(1) : reader.ReadString(2),
                Genres = reader.ReadPipeList(3),
                BannerPaths = reader.ReadPipeList(4).Select(y => CreateImagePath("banner", y)).ToList(),
                Year = Int32.Parse(reader.ReadString(5)),

                // TODO
                DateAdded = new DateTime(1970, 1, 1),
                IsProtected = false,
                UserDefinedCategories = new List<string>(),
            };
        }

        private WebTVShowDetailed CreateWebTVShowDetailed(SQLiteDataReader reader)
        {
            WebTVShowDetailed show = CreateWebTVShow<WebTVShowDetailed>(reader);
            show.PosterPaths = reader.ReadPipeList(6).Select(y => CreateImagePath("banner", y)).ToList();
            show.FanArtPaths = new List<string>() { CreateImagePath("fanart", reader.ReadString(7)) };
            show.Actors = reader.ReadPipeList(8);
            return show;
        }
        #endregion

        #region Seasons
        public IEnumerable<WebTVSeasonBasic> GetAllSeasonsBasic(string seriesId)
        {
            string sql = "SELECT DISTINCT s.ID, s.SeasonIndex, s.SeriesID, s.BannerFileNames, STRFTIME('%Y', e.FirstAired) AS year " +
                         "FROM season s " +
                         "LEFT JOIN online_episodes e ON e.EpisodeIndex = 1 AND e.SeasonIndex = s.SeasonIndex AND e.SeriesID = @seriesId " + 
                         "WHERE s.SeriesID = @seriesId";
            return ReadList<WebTVSeasonBasic>(
                sql,
                delegate(SQLiteDataReader reader)
                {
                    return CreateWebTVSeason<WebTVSeasonBasic>(reader);
                },
                new SQLiteParameter("@seriesId", seriesId)
            );
        }

        public IEnumerable<WebTVSeasonDetailed> GetAllSeasonsDetailed(string seriesId)
        {
            string sql = "SELECT DISTINCT ID, SeasonIndex, SeriesID, STRFTIME('%Y', e.FirstAired) AS year, " +
                            "BannerFileNames " +
                         "FROM season " +
                         "LEFT JOIN online_episodes e ON e.EpisodeIndex = 1 AND e.SeasonIndex = s.SeasonIndex AND e.SeriesID = @seriesId " + 
                         "WHERE SeriesID = @seriesId";
            return ReadList<WebTVSeasonDetailed>(
                sql,
                delegate(SQLiteDataReader reader)
                {
                    return CreateWebTVSeasonDetailed(reader);
                },
                new SQLiteParameter("@seriesId", seriesId)
            );
        }

        public WebTVSeasonDetailed GetSeasonDetailed(string seriesId, string seasonId)
        {
            string sql = "SELECT DISTINCT ID, SeasonIndex, SeriesID, STRFTIME('%Y', e.FirstAired) AS year, " +
                            "BannerFileNames " +
                         "FROM season " +
                         "LEFT JOIN online_episodes e ON e.EpisodeIndex = 1 AND e.SeasonIndex = s.SeasonIndex AND e.SeriesID = @seriesId " + 
                         "WHERE SeriesID = @seriesId AND ID = @seasonId";
            return ReadRow<WebTVSeasonDetailed>(
                sql,
                delegate(SQLiteDataReader reader)
                {
                    return CreateWebTVSeasonDetailed(reader);
                },
                new SQLiteParameter("@seriesId", seriesId),
                new SQLiteParameter("@seasonId", seasonId)
            );
        }

        private T CreateWebTVSeason<T>(SQLiteDataReader reader) where T : WebTVSeasonBasic, new()
        {
            return new T()
            {
                Id = reader.ReadString(0),
                SeasonNumber = reader.ReadInt32(1),
                ShowId = reader.ReadInt32(2).ToString(),
                Year = Int32.Parse(reader.ReadString(3)), // strftime returns an string

                // TODO
                Title = String.Empty,
                DateAdded = new DateTime(1970, 1, 1),
                IsProtected = false,
            };
        }

        private WebTVSeasonDetailed CreateWebTVSeasonDetailed(SQLiteDataReader reader)
        {
            WebTVSeasonDetailed season = CreateWebTVSeason<WebTVSeasonDetailed>(reader);

            // unavailable
            season.BackdropPaths = new List<string>();
            season.BannerPaths = reader.ReadPipeList(3).Select(x => CreateImagePath("banner", x)).ToList();
            season.PosterPaths = new List<string>();

            return season;
        }
        #endregion

        #region Episodes
        public IEnumerable<WebTVEpisodeBasic> GetAllEpisodesBasic()
        {
            string sql = "SELECT e.EpisodeID, e.SeriesID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                            "e.FirstAired, GROUP_CONCAT(l.EpisodeFilename, '|') AS filename " +
                         "FROM online_episodes e " +
                         "INNER JOIN local_episodes l ON e.CompositeID = l.CompositeID " +
                         "WHERE e.Hidden = 0 " +
                         "GROUP BY e.EpisodeID, e.SeriesID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                            "e.FirstAired, e.GuestStars, e.Director, e.Writer";
            return ReadList<WebTVEpisodeBasic>(
                sql,
                delegate(SQLiteDataReader reader)
                {
                    var a = CreateWebTVEpisode<WebTVEpisodeBasic>(reader);
                    return a;
                }
            );
        }

        public IEnumerable<WebTVEpisodeDetailed> GetAllEpisodesDetailed()
        {
            string sql = "SELECT e.EpisodeID, e.SeriesID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                            "e.FirstAired, GROUP_CONCAT(l.EpisodeFilename, '|') AS filename, e.GuestStars, e.Director, e.Writer " +
                         "FROM online_episodes e " +
                         "INNER JOIN local_episodes l ON e.CompositeID = l.CompositeID " +
                         "WHERE e.Hidden = 0 " +
                         "GROUP BY e.EpisodeID, e.SeriesID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                            "e.FirstAired, e.GuestStars, e.Director, e.Writer";
            return ReadList<WebTVEpisodeDetailed>(
                sql,
                delegate(SQLiteDataReader reader)
                {
                    return CreateWebTVEpisodeDetailed(reader);
                }
            );
        }

        public WebTVEpisodeDetailed GetEpisodeDetailed(string episodeId)
        {
            string sql = "SELECT e.EpisodeID, e.SeriesID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                            "e.FirstAired, GROUP_CONCAT(l.EpisodeFilename, '|') AS filename, e.GuestStars, e.Director, e.Writer " +
                         "FROM online_episodes e " +
                         "INNER JOIN local_episodes l ON e.CompositeID = l.CompositeID " +
                         "WHERE e.Hidden = 0 AND e.EpisodeID = @episodeId " +
                         "GROUP BY e.EpisodeID, e.SeriesID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                            "e.FirstAired, e.GuestStars, e.Director, e.Writer";
            return ReadRow<WebTVEpisodeDetailed>(
                sql,
                delegate(SQLiteDataReader reader)
                {
                    return CreateWebTVEpisodeDetailed(reader);
                },
                new SQLiteParameter("@episodeId", episodeId)
            );
        }

        private T CreateWebTVEpisode<T>(SQLiteDataReader reader) where T : WebTVEpisodeBasic, new()
        {
            return new T() {
                Id = reader.ReadInt32(0).ToString(),
                ShowId = reader.ReadInt32(1).ToString(),
                Title = reader.ReadString(2),
                EpisodeNumber = reader.ReadInt32(3),
                SeasonId = reader.ReadInt32(1).ToString() + "_s" + reader.ReadInt32(4).ToString(),
                Watched = reader.ReadBoolean(5),
                Rating = reader.ReadFloat(6),
                BannerPaths = new List<string>() { CreateImagePath("banner", reader.ReadString(7)) },
                FirstAired = reader.ReadDateTime(8),
                Path = reader.ReadPipeList(9),

                // TODO
                DateAdded = new DateTime(1970, 1, 1),
                IsProtected = false,
            };
        }

        private WebTVEpisodeDetailed CreateWebTVEpisodeDetailed(SQLiteDataReader reader)
        {
            WebTVEpisodeDetailed ep = CreateWebTVEpisode<WebTVEpisodeDetailed>(reader);

            ep.GuestStars = reader.ReadPipeList(10);
            ep.Directors = reader.ReadPipeList(11);
            ep.Writers = reader.ReadPipeList(12);
                
            // unavailable
            ep.FanArtPaths = new List<string>();           

            return ep;
        }
        #endregion

        #region Misc
        public IEnumerable<WebGenre> GetAllGenres()
        {
            string sql = "SELECT DISTINCT Genre FROM online_series";
            List<string> genres = new List<string>();
            ReadList<WebGenre>(sql, delegate(SQLiteDataReader reader)
            {
                genres.AddRange(reader.ReadPipeList(0));
                return null;
            });

            return genres.Distinct().Select(x => new WebGenre() { Name = x });
        }

        public IEnumerable<WebCategory> GetAllCategories()
        {
            return new List<WebCategory>();
        }
        #endregion

        #region Retrieval
        public Stream GetBanner(string seriesId, int offset)
        {
            throw new NotImplementedException();
        }

        public Stream GetPoster(string seriesId, int offset)
        {
            throw new NotImplementedException();
        }

        public Stream GetBackdrop(string seriesId, int offset)
        {
            throw new NotImplementedException();
        }

        public Stream GetSeasonBanner(string seriesId, string seasonId, int offset)
        {
            throw new NotImplementedException();
        }

        public Stream GetSeasonPoster(string seriesId, string seasonId, int offset)
        {
            throw new NotImplementedException();
        }

        public Stream GetSeasonBackdrop(string seriesId, string seasonId, int offset)
        {
            return new FileStream(GetSeasonDetailed(seriesId, seasonId).BackdropPaths[offset], FileMode.Open);
        }
        #endregion
    }
}
