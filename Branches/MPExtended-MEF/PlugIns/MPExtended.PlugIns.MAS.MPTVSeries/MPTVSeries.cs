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
using System.Data.SQLite;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.IO;
using MPExtended.Libraries.MASPlugin;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.PlugIns.MAS.MPTVSeries
{
    [Export(typeof(ITVShowLibrary))]
    [ExportMetadata("Database", "MPTVSeries")]
    public class MPTVSeries : Database, ITVShowLibrary
    {
        public MPTVSeries() :
            base(Configuration.GetPluginConfiguration()["database"])
        {
        }

        private string CreateImagePath(string type, string dbPath)
        {
            string rootDir = Configuration.GetPluginConfiguration()[type];
            return Path.Combine(rootDir, dbPath.Replace('/', '\\'));
        }

        #region Shows
        public IEnumerable<WebTVShowBasic> GetAllTVShowsBasic()
        {
            return GetAllTVShowsDetailed().Cast<WebTVShowBasic>();
        }

        public IEnumerable<WebTVShowDetailed> GetAllTVShowsDetailed()
        {
            string sql = "SELECT DISTINCT s.ID, s.Pretty_Name, l.Parsed_Name, s.Genre, s.BannerFileNames, s.PosterFileNames, s.fanart " +
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
                    return CreateWebTVShow<WebTVShowDetailed>(reader);
                }
            );
        }

        public WebTVShowDetailed GetTVShowDetailed(string seriesId)
        {
            string sql = "SELECT DISTINCT s.ID, s.Pretty_Name, l.Parsed_Name, s.Genre, s.BannerFileNames, s.PosterFileNames, s.fanart, s.Actors " +
                         "FROM online_series AS s " +
                         "INNER JOIN local_series AS l ON s.ID = l.ID AND l.Hidden = 0 " +
                         "WHERE s.ID != 0 AND s.HasLocalFiles = 1 AND s.ID = @seriesId";
            return ReadRow<WebTVShowDetailed>(
                sql,
                delegate(SQLiteDataReader reader)
                {
                    return CreateWebTVShow<WebTVShowDetailed>(reader);
                },
                new SQLiteParameter("@seriesId", seriesId)
            );
        }

        private T CreateWebTVShow<T>(SQLiteDataReader reader) where T : WebTVShowBasic, new()
        {
            return new T()
            {
                Id = reader.ReadString(0),
                Title = !String.IsNullOrEmpty(reader.ReadString(1)) ? reader.ReadString(1) : reader.ReadString(2),
                Genres = reader.ReadPipeList(3),
                BannerPaths = reader.ReadPipeList(4).Select(y => CreateImagePath("banner", y)).ToList(),
                PosterPaths = reader.ReadPipeList(5).Select(y => CreateImagePath("banner", y)).ToList(),
                FanArtPaths = new List<string>() { CreateImagePath("fanart", reader.ReadString(6)) },
                Actors = reader.ReadPipeList(7),

                // TODO
                DateAdded = new DateTime(1970, 1, 1),
                IsProtected = false,
                UserDefinedCategories = new List<string>(),
            };
        }
        #endregion

        #region Seasons
        public IEnumerable<WebTVSeasonBasic> GetAllSeasonsBasic(string seriesId)
        {
            return GetAllSeasonsDetailed(seriesId).Cast<WebTVSeasonBasic>();
        }

        public IEnumerable<WebTVSeasonDetailed> GetAllSeasonsDetailed(string seriesId)
        {
            string sql = "SELECT DISTINCT ID, SeasonIndex, SeriesID, BannerFileNames " +
                         "FROM season " +
                         "WHERE SeriesID = @seriesId";
            return ReadList<WebTVSeasonDetailed>(
                sql,
                delegate(SQLiteDataReader reader)
                {
                    return CreateWebTVSeason<WebTVSeasonDetailed>(reader);
                },
                new SQLiteParameter("@seriesId", seriesId)
            );
        }

        public WebTVSeasonDetailed GetSeasonDetailed(string seriesId, string seasonId)
        {
            string sql = "SELECT DISTINCT ID, SeasonIndex, SeriesID, BannerFileNames " +
                         "FROM season " +
                         "WHERE SeriesID = @seriesId AND ID = @seasonId";
            return ReadRow<WebTVSeasonDetailed>(
                sql,
                delegate(SQLiteDataReader reader)
                {
                    return CreateWebTVSeason<WebTVSeasonDetailed>(reader);
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
                ShowId = reader.ReadString(2),
                BannerPaths = reader.ReadPipeList(3).Select(x => CreateImagePath("banner", x)).ToList(),

                // unavailable
                Title = String.Empty,
                FanArtPaths = new List<string>(),

                // TODO
                DateAdded = new DateTime(1970, 1, 1),
                IsProtected = false,
            };
        }
        #endregion

        #region Episodes
        public IEnumerable<WebTVEpisodeBasic> GetAllEpisodesBasic()
        {
            return GetAllEpisodesDetailed().Cast<WebTVEpisodeBasic>();
        }

        public IEnumerable<WebTVEpisodeDetailed> GetAllEpisodesDetailed()
        {
            string sql = "SELECT e.EpisodeID, e.SeriesID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                            "e.FirstAired, e.GuestStars, e.Director, e.Writer, GROUP_CONCAT(l.EpisodeFilename, '|') AS filename " +
                         "FROM online_episodes e " +
                         "INNER JOIN local_episodes l ON e.ComposieID = l.CompositeID " +
                         "WHERE e.Hidden = 0 " +
                         "GROUP BY e.EpisodeID, e.SeriesID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                            "e.FirstAired, e.GuestStars, e.Director, e.Writer";
            return ReadList<WebTVEpisodeDetailed>(
                sql,
                delegate(SQLiteDataReader reader)
                {
                    return CreateWebTVEpisode<WebTVEpisodeDetailed>(reader);
                }
            );
        }

        public WebTVEpisodeDetailed GetEpisodeDetailed(string episodeId)
        {
            string sql = "SELECT e.EpisodeID, e.SeriesID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                            "e.FirstAired, e.GuestStars, e.Director, e.Writer, GROUP_CONCAT(l.EpisodeFilename, '|') AS filename " +
                         "FROM online_episodes e " +
                         "INNER JOIN local_episodes l ON e.ComposieID = l.CompositeID " +
                         "WHERE e.Hidden = 0 AND e.EpisodeID = @episodeId " +
                         "GROUP BY e.EpisodeID, e.SeriesID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                            "e.FirstAired, e.GuestStars, e.Director, e.Writer";
            return ReadRow<WebTVEpisodeDetailed>(
                sql,
                delegate(SQLiteDataReader reader)
                {
                    return CreateWebTVEpisode<WebTVEpisodeDetailed>(reader);
                },
                new SQLiteParameter("@episodeId", episodeId)
            );
        }

        private T CreateWebTVEpisode<T>(SQLiteDataReader reader) where T : WebTVEpisodeBasic, new()
        {
            return new T()
            {
                Id = reader.ReadString(0),
                ShowId = reader.ReadString(1),
                Title = reader.ReadString(2),
                EpisodeNumber = reader.ReadInt32(3),
                SeasonId = reader.ReadString(1) + "_s" + reader.ReadString(4),
                Watched = reader.ReadBoolean(5),
                Rating = reader.ReadFloat(6),
                BannerPaths = new List<string>() { CreateImagePath("banner", reader.ReadString(7)) },
                FirstAired = reader.ReadDateTime(8),
                GuestStars = reader.ReadPipeList(9),
                Directors = reader.ReadPipeList(10),
                Writers = reader.ReadPipeList(11),
                Path = reader.ReadPipeList(12),
                
                // unavailable
                FanArtPaths = new List<string>(),

                // TODO
                DateAdded = new DateTime(1970, 1, 1),
                IsProtected = false,
            };
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
            throw new NotImplementedException();
        }

        public DirectoryInfo GetSourceRootDirectory()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
