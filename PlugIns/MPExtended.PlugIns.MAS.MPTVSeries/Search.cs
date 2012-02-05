#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.PlugIns.MAS.MPTVSeries
{
    public partial class MPTVSeries : Database, ITVShowLibrary
    {
        public IEnumerable<WebSearchResult> Search(string text)
        {
            using (DatabaseConnection connection = OpenConnection())
            {
                IEnumerable<WebSearchResult> results;
                SQLiteParameter param = new SQLiteParameter("@search", "%" + text + "%");

                // search for shows
                string showSql = "SELECT ID, Pretty_Name FROM online_series WHERE Pretty_Name LIKE @search";
                IEnumerable<WebSearchResult> shows = ReadList<WebSearchResult>(showSql, delegate(SQLiteDataReader reader)
                {
                    string title = reader.ReadString(1);
                    return new WebSearchResult()
                    {
                        Type = WebMediaType.TVShow,
                        Id = reader.ReadIntAsString(0),
                        Title = title,
                        Score = 40 + (int)Math.Round((decimal)text.Length / title.Length * 40),
                    };
                }, param);
                results = shows;

                string actorShowSql = "SELECT ID, Pretty_Name, Actors FROM online_series WHERE Actors LIKE @search";
                IEnumerable<WebSearchResult> actorShows = ReadList<WebSearchResult>(actorShowSql, delegate(SQLiteDataReader reader)
                {
                    var valid = reader.ReadPipeList(2).Where(x => x.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0); // contains is case sensitive
                    if (valid.Count() == 0)
                        return null;
                    return new WebSearchResult()
                    {
                        Type = WebMediaType.TVShow,
                        Id = reader.ReadIntAsString(0),
                        Title = reader.ReadString(1),
                        Score = valid.Max(x => 40 + (int)Math.Round((decimal)text.Length / x.Length * 30))
                    };
                }, param);
                results = results.Concat(actorShows);

                #region Episodes
                // initialize
                List<SQLiteParameter> parameters = new List<SQLiteParameter>() { param };

                // search for episodes
                Regex episodeRegex = new Regex(@"^(.*) ([0-9]{1,3})x([0-9]{1,3})$");
                Match episodeResult = episodeRegex.Match(text);
                string episodeRegexWhere = "0";
                if (episodeResult.Success)
                {
                    episodeRegexWhere = "((os.Pretty_Name LIKE @show OR ls.Parsed_Name LIKE @show) AND e.SeasonIndex = @season AND e.EpisodeIndex = @episode)";
                    parameters.Add(new SQLiteParameter("@show", "%" + episodeResult.Groups[1].Value.Trim() + "%"));
                    parameters.Add(new SQLiteParameter("@season", episodeResult.Groups[2].Value));
                    parameters.Add(new SQLiteParameter("@episode", episodeResult.Groups[3].Value));
                }

                // execute
                string episodeSql =
                    "SELECT e.EpisodeID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.SeriesID, e.GuestStars, os.Pretty_Name, MIN(ls.Parsed_Name) AS Parsed_Name " +
                    "FROM online_episodes e " +
                    "INNER JOIN online_series os ON os.ID = e.SeriesID " +
                    "INNER JOIN local_series ls ON ls.ID = e.SeriesID " +
                    "WHERE e.EpisodeName LIKE @search OR e.GuestStars LIKE @search OR " + episodeRegexWhere + " " +
                    "GROUP BY e.EpisodeID, e.EpisodeName, e.EpisodeIndex, e.SeasonIndex, e.SeriesID, e.GuestStars, os.Pretty_Name ";
                IEnumerable<WebSearchResult> episodes = ReadList<WebSearchResult>(episodeSql, delegate(SQLiteDataReader reader)
                {
                    // read the data
                    string title = reader.ReadString(1);
                    string showTitle = reader.ReadString(6);
                    WebSearchResult result = new WebSearchResult()
                    {
                        Type = WebMediaType.TVEpisode,
                        Id = reader.ReadIntAsString(0),
                        Title = title,
                        Details = new SerializableDictionary<string>()
                    {
                        { "EpisodeNumber", reader.ReadIntAsString(2) },
                        { "SeasonNumber", reader.ReadIntAsString(3) },
                        { "ShowId", reader.ReadIntAsString(4) },
                        { "ShowName", !String.IsNullOrEmpty(showTitle) ? showTitle : reader.ReadString(7) }
                    }
                    };

                    // title
                    int score = title.Contains(text, false) ? 40 + (int)Math.Round((decimal)text.Length / title.Length * 40) : 0;

                    // guest stars
                    var valid = reader.ReadPipeList(5).Where(x => x.Contains(text, false)); // .Contains() is case sensitive
                    score = Math.Max(score, valid.Count() > 0 ? valid.Max(x => 20 + (int)Math.Round((decimal)text.Length / x.Length * 40)) : 0);

                    // regex match
                    if (episodeResult.Success &&
                           (result.Details["ShowName"].Contains(episodeResult.Groups[1].Value.Trim(), false) &&
                            result.Details["SeasonNumber"] == episodeResult.Groups[2].Value &&
                            result.Details["EpisodeNumber"] == episodeResult.Groups[3].Value))
                        score = 100;

                    // set score and return
                    result.Score = score;
                    return result;
                }, parameters.ToArray());


                // when there are multiple matches with 100%, set them all to 95% as we apparantly aren't sure
                if (episodes.Count(x => x.Score == 100) > 1)
                {
                    episodes = episodes.Select(x =>
                    {
                        if (x.Score == 100)
                            x.Score = 95;
                        return x;
                    });
                }
                results = results.Concat(episodes);
                #endregion

                // fancy season search: <showname> s<season>
                Regex seasonRegex = new Regex(@"^(.*) s([0-9]{1,3})$");
                Match seasonResult = seasonRegex.Match(text);
                if (seasonResult.Success)
                {
                    string sql =
                        "SELECT DISTINCT s.ID, s.SeasonIndex, s.SeriesID, o.Pretty_Name, MIN(l.Parsed_Name) AS Parsed_Name " +
                        "FROM season s " +
                        "INNER JOIN online_series o ON s.SeriesID = o.ID " +
                        "INNER JOIN local_series l ON s.SeriesID = l.ID " +
                        "WHERE (o.Pretty_Name = @show OR l.Parsed_Name = @show) AND s.SeasonIndex = @season " +
                        "GROUP BY s.ID, s.SeasonIndex, s.SeriesID, o.Pretty_Name ";
                    results = ReadList<WebSearchResult>(sql, delegate(SQLiteDataReader reader)
                    {
                        string showTitle = !String.IsNullOrEmpty(reader.ReadString(3)) ? reader.ReadString(3) : reader.ReadString(4);
                        return new WebSearchResult()
                        {
                            Type = WebMediaType.TVSeason,
                            Id = reader.ReadString(0),
                            Title = showTitle + " (season " + reader.ReadInt32(1) + ")",
                            Score = 100,
                            Details = new SerializableDictionary<string>()
                        {
                            { "SeasonNumber", reader.ReadIntAsString(1) },
                            { "ShowId", reader.ReadIntAsString(2) },
                            { "ShowName", showTitle }
                        }
                        };
                    }, new SQLiteParameter("@show", seasonResult.Groups[1].Value.Trim()),
                       new SQLiteParameter("@season", seasonResult.Groups[2].Value))
                         .Concat(results);
                }

                // return
                return results;
            }
        }
    }
}