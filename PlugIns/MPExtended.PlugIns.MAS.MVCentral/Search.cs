#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.IO;
using System.Text;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Music;

namespace MPExtended.PlugIns.MAS.MVCentral
{
    public partial class MVCentral : Database, IMusicLibrary
    {
        public IEnumerable<WebSearchResult> Search(string text)
        {
            SQLiteParameter parameter = new SQLiteParameter("@search", "%" + text + "%");

            string artistSql = "SELECT id, artist FROM artist_info WHERE artist LIKE @search";
            IEnumerable<WebSearchResult> artistResults = ReadList<WebSearchResult>(artistSql, delegate(SQLiteDataReader reader)
            {
                string name = reader.ReadString(1);
                return new WebSearchResult()
                {
                    Type = WebMediaType.MusicArtist,
                    Id = reader.ReadString(0),
                    Title = name,
                    Score = (int)Math.Round(40 + (decimal)text.Length / name.Length * 40)
                };
            }, parameter);

            string albumSql = "SELECT a.id, a.album, p.id, p.artist " +
                              "FROM album_info a " +
                              "LEFT JOIN album_info__track_info at ON at.album_info_id = a.id " +
                              "LEFT JOIN artist_info__track_info pt ON pt.track_info_id = at.track_info_id " +
                              "LEFT JOIN artist_info p ON pt.artist_info_id = p.id " +
                              "WHERE a.album LIKE @search OR p.artist LIKE @search " +
                              "GROUP BY a.id, a.album, p.id, p.artist " + 
                              "HAVING COUNT(p.id) > 0 ";
            IEnumerable<WebSearchResult> albumResults = ReadList<WebSearchResult>(albumSql, delegate(SQLiteDataReader reader)
            {
                string name = reader.ReadString(1);
                string artist = reader.ReadString(3);
                int score = name.Contains(text) ?
                    (int)Math.Round(40 + (decimal)text.Length / name.Length * 40) :
                    (int)Math.Round(20 + (decimal)text.Length / artist.Length * 40);
                return new WebSearchResult()
                {
                    Type = WebMediaType.MusicAlbum,
                    Id = reader.ReadString(0),
                    Title = name,
                    Score = score,
                    Details = new WebDictionary<string>()
                    {
                        { "ArtistId", reader.ReadString(2) },
                        { "Artist", artist },
                    }
                };
            }, parameter);

            string trackSql = "SELECT t.id, t.track, MIN(l.duration) AS duration, a.id, a.album, p.id, p.artist " +
                              "FROM track_info t " +
                              "LEFT JOIN local_media__track_info lt ON lt.track_info_id = t.id " +
                              "LEFT JOIN local_media l ON l.id = lt.local_media_id " +
                              "LEFT JOIN album_info__track_info at ON at.track_info_id = t.id " +
                              "LEFT JOIN album_info a ON at.album_info_id = a.id " +
                              "LEFT JOIN artist_info__track_info pt ON pt.track_info_id = t.id " +
                              "LEFT JOIN artist_info p ON pt.artist_info_id = p.id " + 
                              "WHERE t.track LIKE @search OR a.album LIKE @search OR p.artist LIKE @search " +
                              "GROUP BY t.id, t.track ";
            IEnumerable<WebSearchResult> trackResults = ReadList<WebSearchResult>(trackSql, delegate(SQLiteDataReader reader)
            {
                string name = reader.ReadString(1);
                string album = reader.ReadString(4);
                string artist = reader.ReadString(6);
                int score = 0;
                if (name.Contains(text))
                    score = (int)Math.Round(40 + (decimal)text.Length / name.Length * 40);
                if (album.Contains(text))
                    score = (int)Math.Round(20 + (decimal)text.Length / album.Length * 40);
                if (artist.Contains(text))
                    score = (int)Math.Round((decimal)text.Length / artist.Length * 40);
                return new WebSearchResult()
                {
                    Type = WebMediaType.MusicTrack,
                    Id = reader.ReadString(0),
                    Title = name,
                    Score = score,
                    Details = new WebDictionary<string>()
                    {
                        { "Duration", reader.ReadString(2) },
                        { "AlbumId", reader.ReadString(3) },
                        { "Album", album },
                        { "ArtistId", reader.ReadString(5) },
                        { "Artist", artist },
                        { "Year", "" }
                    }
                };
            }, parameter);

            return trackResults.Concat(artistResults).Concat(albumResults);
        }
    }
}
