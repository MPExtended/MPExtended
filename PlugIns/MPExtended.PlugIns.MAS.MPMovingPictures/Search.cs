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
using System.Data.SQLite;
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service.Extensions;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.PlugIns.MAS.MovingPictures
{
    public partial class MPMovingPictures : Database, IMovieLibrary
    {
        public IEnumerable<WebSearchResult> Search(string text)
        {
            string sql =
                "SELECT id, title, actors, year, genres, alternate_titles " +
                "FROM movie_info " +
                "WHERE title LIKE @search OR alternate_titles LIKE @search OR actors LIKE @search";
            return ReadList<WebSearchResult>(sql, delegate(SQLiteDataReader reader)
            {
                var result = new WebSearchResult()
                {
                    Type = WebMediaType.Movie,
                    Id = reader.ReadIntAsString(0),
                    Title = reader.ReadString(1),
                    Details = new WebDictionary<string>()
                    {
                        { "Year", reader.ReadIntAsString(3) },
                        { "Genres", String.Join(", ", reader.ReadPipeList(4)) },
                    }
                };

                // simple score
                int score = result.Title.Contains(text, false) ? (int)Math.Round(40 + (decimal)text.Length / result.Title.Length * 40) : 0;
                var validAlternate = reader.ReadPipeList(5).Where(x => x.Contains(text, false));
                score = Math.Max(score, validAlternate.Count() > 0 ? validAlternate.Max(x => 40 + (int)Math.Round((decimal)text.Length / x.Length * 30)) : 0);

                // actors
                var valid = reader.ReadPipeList(2).Where(x => x.Contains(text, false));
                score = Math.Max(score, valid.Count() > 0 ? valid.Max(x => 40 + (int)Math.Round((decimal)text.Length / x.Length * 30)) : 0);

                // set
                result.Score = score;
                return result;
            }, new SQLiteParameter("@search", "%" + text + "%"));
        }
    }
}
