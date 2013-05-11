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
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.PlugIns.MAS.MPTVSeries
{
    [Export(typeof(ITVShowLibrary))]
    [ExportMetadata("Name", "MP-TVSeries")]
    [ExportMetadata("Id", 6)]
    public partial class MPTVSeries : Database, ITVShowLibrary
    {
        // The mapping of artwork is a bit confusing with MP-TVSeries:
        // - For series, WebArtworkType.Banner is mapped to the -langen-graphical directory (via the BannerFileNames database column)
        // - For series, WebArtworkType.Poster is mapped to the -langen-posters directory (via the PosterFileNames database column)
        // - For series, WebArtworkType.Backdrop is mapped to the fanart in the Fan Art directory and isn't always available (via the fanart database column)
        // - For seasons, WebArtworkType.Banner is mapped to the -langen-seasons directory (via the BannerFileNames database column)
        // - For episodes, WebArtworkType.Banner is mapped to the Episodes directory (via the thumbFilename database column)

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

        private LazyQuery<T> GetAllTVShows<T>() where T : WebTVShowBasic, new()
        {
            /* In the beginning, this code was written and it was simple, and everything was good. Then, someone deleted their local files, and the code
             * broke. It was fixed by ignoring all episodes that don't have local files (INNER JOIN local_episodes), and requiring all shows to have 
             * local files (s.HasLocalFiles = 1), and everything was good again (commit 76191b6).
             * 
             * Then, people started having more exotic databases, and this proved broken too. There were two apparant problems:
             * - The HasLocalFiles flag doesn't work the way we expected it to work. There were episodes for shows with HasLocalFiles = 0 is returned from
             *   the GetAllEpisodes() method below, even though that one still has the INNER JOIN on local_episodes. This caused problems with clients who
             *   rightfully expected the show for an episode to exist. 
             * - Some users wanted to have shows without any local files showing up in their WebMediaPortal. Don't ask wny.
             * So, the requirement for s.HasLocalFiles = 1 was dropped (commit 926841b), and everything was good again.
             * 
             * But it wasn't good enough. People deleted shows from their databases, and people started complaining again: their deleted shows were being
             * shown in WebMediaPortal. The obvious solution would be to require s.HasLocalFiles = 1 again, but the world isn't simple anymore.
             * 
             * So, what we're doing is:
             * - We completely ignore all flags we encounter in the online_series table. They may be right, they may be wrong, they may be misnamed or they
             *   may be obsolete, I've no idea. We'll ignore them so that we don't turn insane. 
             * - For loading the number of (unwatched) episodes, we always use a subquery that queries the online_episodes table with an INNER JOIN on the
             *   local_episodes table. This was actually added somewhere in the middle of the story above, because MP-TVSeries counted episodes without an
             *   entry in local_episodes as an unwatched episode too. That isn't directly problematic, but since we ignore those episodes, the episode count
             *   was bigger than the number of episodes returned from GetAllEpisodes(). That confused some clients, and it didn't make any sense at all.
             * - We don't return any shows or seasons that don't have any episodes using that method. That implies that we don't support seasons or shows 
             *   without any local episodes.
             * - We hope and pray that SQLite is smart enough to optimize the subquery and doesn't execute it for each show. 
             * 
             * And everything is good again. For now.
             */
            string sql = 
                    "SELECT DISTINCT s.ID, MIN(l.Parsed_Name) AS parsed_name, s.Pretty_Name, s.Genre, STRFTIME('%Y', s.FirstAired) AS year, s.Actors, s.Rating, s.ContentRating, " +
                        "s.Summary, s.Status, s.Network, s.AirsDay, s.AirsTime, s.Runtime, s.IMDB_ID, s.added, " +
                        "s.BannerFileNames, s.CurrentBannerFileName, s.PosterFileNames, s.PosterBannerFileName, f.fanart_list, " +
                        "SUM(c.episodes) AS episodes, SUM(c.unwatched) AS unwatched, COUNT(c.SeasonIndex) AS seasons " + 
                    "FROM online_series AS s " +
                    "INNER JOIN local_series AS l ON s.ID = l.ID AND l.Hidden = 0 AND l.DuplicateLocalName = 0 " +
                    "INNER JOIN (" +
                            // this subquery is so ugly, that every time it's executed a kitten dies.
                            "SELECT seriesID, GROUP_CONCAT(LocalPath || '?' || Rating || '?' || id, '|') AS fanart_list " + // the question mark is used because it's forbidden in paths
                            "FROM Fanart " +
                            "WHERE LocalPath != '' " + // there used to be "AND f.SeriesName = 'false'" appended here, but that doesn't seem to make any sense
                            "GROUP BY seriesID " +
                        ") AS f ON s.ID = f.seriesID " + 
                    "LEFT JOIN (" +
                            "SELECT e.SeriesID, e.SeasonIndex, COUNT(*) AS episodes, COUNT(NULLIF(e.Watched, 1)) AS unwatched " +
                            "FROM online_episodes e " +
                            "INNER JOIN local_episodes l ON e.CompositeID = l.CompositeID " +
                            "WHERE e.Hidden = 0 " +
                            "GROUP BY e.SeriesID, e.SeasonIndex " +
                        ") AS c ON s.ID = c.SeriesID " +
                    "WHERE s.ID != 0 AND %where " +
                    "GROUP BY " +
                        "s.ID, s.Pretty_Name, s.Genre, s.FirstAired, s.Actors, s.Rating, s.ContentRating, " +
                        "s.Summary, s.Status, s.Network, s.AirsDay, s.AirsTime, s.Runtime, s.IMDB_ID, s.added, " +
                        "s.BannerFileNames, s.CurrentBannerFileName, s.PosterFileNames, s.PosterBannerFileName, f.fanart_list " + 
                    "HAVING c.episodes > 0 " + 
                    "%order";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>() {
                new SQLFieldMapping("s", "ID", "Id", DataReaders.ReadIntAsString),
                new SQLFieldMapping("s", "ID", "ExternalId", CustomReaders.ExternalIdReader, new ExternalSiteReaderParameters(DataReaders.ReadIntAsString, "TVDB")),
                new SQLFieldMapping("s", "Pretty_Name", "Title", CustomReaders.FixNameReader),
                new SQLFieldMapping("s", "Genre", "Genres", DataReaders.ReadPipeList),
                new SQLFieldMapping("", "year", "Year", DataReaders.ReadStringAsInt),
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
                new SQLFieldMapping("s", "added", "DateAdded", DataReaders.ReadDateTime),
                new SQLFieldMapping("s", "BannerFileNames", "Artwork", CustomReaders.PreferedArtworkReader, new ArtworkReaderParameters(WebFileType.Banner, configuration["banner"])),
                new SQLFieldMapping("s", "PosterFileNames", "Artwork", CustomReaders.PreferedArtworkReader, new ArtworkReaderParameters(WebFileType.Poster, configuration["banner"])),
                new SQLFieldMapping("", "episodes", "EpisodeCount", DataReaders.ReadInt32),
                new SQLFieldMapping("", "unwatched", "UnwatchedEpisodeCount", DataReaders.ReadInt32),
                new SQLFieldMapping("", "seasons", "SeasonCount", DataReaders.ReadInt32),
                new SQLFieldMapping("", "fanart_list", "Artwork", CustomReaders.FanartArtworkReader, new ArtworkReaderParameters(WebFileType.Backdrop, configuration["fanart"])),
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
            // and load seasons
            string sql = 
                    "SELECT DISTINCT s.ID, s.SeriesID, s.SeasonIndex, s.BannerFileNames, s.CurrentBannerFileName, " +
                        "c.episodes, c.unwatched, STRFTIME('%Y', MIN(e.FirstAired)) AS year " +
                    "FROM season s " +
                    "INNER JOIN (" +
                            "SELECT e.SeriesID, e.SeasonIndex, COUNT(*) AS episodes, COUNT(NULLIF(e.Watched, 1)) AS unwatched " +
                            "FROM online_episodes e " +
                            "INNER JOIN local_episodes l ON e.CompositeID = l.CompositeID " +
                            "WHERE e.Hidden = 0 " +
                            "GROUP BY e.SeriesID, e.SeasonIndex" +
                        ") AS c ON s.SeriesID = c.SeriesID AND s.SeasonIndex = c.SeasonIndex " +
                    "LEFT JOIN online_episodes e ON e.SeasonIndex = s.SeasonIndex AND e.SeriesID = s.SeriesID " +
                    "WHERE %where " +
                    "GROUP BY s.ID, s.SeriesID, s.SeasonIndex, s.BannerFileNames, s.CurrentBannerFileName, c.episodes, c.unwatched " + 
                    "HAVING c.episodes > 0 " + 
                    "%order";
            return new LazyQuery<T>(this, sql, new List<SQLFieldMapping>() {
                new SQLFieldMapping("s", "ID", "Id", DataReaders.ReadString),
                new SQLFieldMapping("s", "SeasonIndex", "SeasonNumber", DataReaders.ReadInt32),
                new SQLFieldMapping("s", "SeriesID", "ShowId", DataReaders.ReadIntAsString),
                new SQLFieldMapping("s", "BannerFileNames", "Artwork", CustomReaders.PreferedArtworkReader, new ArtworkReaderParameters(WebFileType.Banner, configuration["banner"])),
                new SQLFieldMapping("c", "episodes", "EpisodeCount", DataReaders.ReadInt32),
                new SQLFieldMapping("c", "unwatched", "UnwatchedEpisodeCount", DataReaders.ReadInt32),
                new SQLFieldMapping("", "year", "Year", DataReaders.ReadStringAsInt),
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
                    "SELECT e.CompositeID, e.EpisodeIndex, e.SeriesID, e.SeasonIndex, e.Watched, e.Rating, e.thumbFilename, " +
                        "e.FirstAired, GROUP_CONCAT(l.EpisodeFilename, '|') AS filename, MIN(l.FileDateAdded) AS dateAdded, " +
                        "e.GuestStars, e.Director, e.Writer, e.IMDB_ID, e.Summary, " +
                        "CASE WHEN LENGTH(TRIM(e.EpisodeName)) > 0 THEN e.EpisodeName ELSE l.LocalEpisodeName END AS name, " + 
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
                new SQLFieldMapping("e", "CompositeID", "Id", DataReaders.ReadString),
                new SQLFieldMapping("e", "EpisodeID", "ExternalId", CustomReaders.ExternalIdReader, new ExternalSiteReaderParameters(DataReaders.ReadIntAsString, "TVDB")),
                new SQLFieldMapping("", "name", "Title", DataReaders.ReadString),
                new SQLFieldMapping("e", "SeriesID", "ShowId", DataReaders.ReadIntAsString),
                new SQLFieldMapping("e", "EpisodeIndex", "EpisodeNumber", DataReaders.ReadInt32),
                new SQLFieldMapping("e", "SeasonIndex", "SeasonId", CustomReaders.ReadSeasonID),
                new SQLFieldMapping("e", "SeasonIndex", "SeasonNumber", DataReaders.ReadInt32),
                new SQLFieldMapping("", "filename", "Path", DataReaders.ReadPipeList),
                new SQLFieldMapping("", "dateAdded", "DateAdded", DataReaders.ReadDateTime),
                new SQLFieldMapping("e", "FirstAired", "FirstAired", DataReaders.ReadDateTime),
                new SQLFieldMapping("e", "Watched", "Watched", DataReaders.ReadBoolean),
                new SQLFieldMapping("e", "Rating", "Rating", DataReaders.ReadFloat),
                new SQLFieldMapping("e", "thumbFilename", "Artwork", CustomReaders.ArtworkReader, new ArtworkReaderParameters(WebFileType.Banner, configuration["banner"])),
                new SQLFieldMapping("e", "GuestStars", "GuestStars", CustomReaders.ActorReader),
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
            return new WebFileInfo(PathUtil.StripFileProtocolPrefix(path));
        }

        public Stream GetFile(string path)
        {
            return new FileStream(path, FileMode.Open);
        }

        public WebDictionary<string> GetExternalMediaInfo(WebMediaType type, string id)
        {
            if (type == WebMediaType.TVSeason)
            {
                var season = GetSeasonBasic(id);
                return new WebDictionary<string>()
                {
                    { "Type", "mptvseries season" },
                    { "SeasonIndex", season.SeasonNumber.ToString() },
                    { "ShowId", season.ShowId },
                };
            }

            return new WebDictionary<string>()
            {
                { "Type", type == WebMediaType.TVShow ? "mptvseries show" : "mptvseries episode" },
                { "Id", id.ToString() }
            };
        }
    }
}